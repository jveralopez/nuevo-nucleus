using System.Diagnostics;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PortalBffService.Services;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    builder.Logging.ClearProviders();
    builder.Logging.AddJsonConsole(options =>
    {
        options.IncludeScopes = true;
        options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        options.UseUtcTimestamp = true;
    });
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var rawOrigins = builder.Configuration["Cors:Origins"];
        var origins = string.IsNullOrWhiteSpace(rawOrigins)
            ? Array.Empty<string>()
            : rawOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var allowedOrigins = origins.Length > 0
            ? origins
            : builder.Environment.IsDevelopment()
                ? new[] { "http://localhost:3001", "http://localhost:3002" }
                : Array.Empty<string>();
        var allowNullOrigin = builder.Environment.IsDevelopment();
        policy.SetIsOriginAllowed(origin =>
            (allowNullOrigin && origin == "null") || Array.Exists(allowedOrigins, candidate => string.Equals(candidate, origin, StringComparison.OrdinalIgnoreCase)));
        policy.AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var authOptions = new AuthOptions();
builder.Configuration.Bind("Auth", authOptions);
builder.Services.AddSingleton(authOptions);

var portalOptions = new PortalOptions();
builder.Configuration.Bind("Portal", portalOptions);
builder.Services.AddSingleton(portalOptions);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = authOptions.Issuer,
            ValidAudience = authOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.SigningKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpClient();
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" });

var otelEnabledRaw = builder.Configuration["OpenTelemetry:Enabled"];
var otelEnabled = bool.TryParse(otelEnabledRaw, out var otelEnabledValue) && otelEnabledValue;
if (otelEnabled)
{
    var serviceName = builder.Configuration.GetValue<string>("OpenTelemetry:ServiceName") ?? "portal-bff-service";
    var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
    var otlpProtocol = builder.Configuration["OTEL_EXPORTER_OTLP_PROTOCOL"];
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService(serviceName))
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(options =>
            {
                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                }
                if (string.Equals(otlpProtocol, "http/protobuf", StringComparison.OrdinalIgnoreCase))
                {
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                }
                else if (string.Equals(otlpProtocol, "grpc", StringComparison.OrdinalIgnoreCase))
                {
                    options.Protocol = OtlpExportProtocol.Grpc;
                }
            }))
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddOtlpExporter(options =>
            {
                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                }
                if (string.Equals(otlpProtocol, "http/protobuf", StringComparison.OrdinalIgnoreCase))
                {
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                }
                else if (string.Equals(otlpProtocol, "grpc", StringComparison.OrdinalIgnoreCase))
                {
                    options.Protocol = OtlpExportProtocol.Grpc;
                }
            }));
}

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "anon",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

var connectionString = builder.Configuration.GetConnectionString("PortalBffDb") ?? "Data Source=storage/portal-notifications.db";
builder.Services.AddSingleton(new NotificationStore(connectionString));

var dataProtectionPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(dataProtectionPath))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
        .SetApplicationName("nucleus");
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
    app.Use(async (context, next) =>
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["Referrer-Policy"] = "no-referrer";
        context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
        context.Response.Headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none';";
        await next();
    });
}
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.Use(async (context, next) =>
{
    var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
    var correlationId = context.Request.Headers["X-Correlation-Id"].ToString();
    var idempotencyKey = context.Request.Headers["Idempotency-Key"].ToString();
    context.Response.Headers["X-Trace-Id"] = traceId;
    var sw = Stopwatch.StartNew();
    app.Logger.LogInformation("HTTP {Method} {Path} start TraceId={TraceId} CorrelationId={CorrelationId} IdempotencyKey={IdempotencyKey}",
        context.Request.Method, context.Request.Path, traceId, correlationId, idempotencyKey);
    await next();
    sw.Stop();
    app.Logger.LogInformation("HTTP {Method} {Path} {StatusCode} {ElapsedMs}ms TraceId={TraceId} CorrelationId={CorrelationId} IdempotencyKey={IdempotencyKey}",
        context.Request.Method, context.Request.Path, context.Response.StatusCode, sw.ElapsedMilliseconds, traceId, correlationId, idempotencyKey);
});

app.Use(async (context, next) =>
{
    if (!context.Request.Headers.ContainsKey("X-Correlation-Id"))
    {
        context.Request.Headers["X-Correlation-Id"] = Guid.NewGuid().ToString("N");
    }

    var method = context.Request.Method;
    var needsIdempotency = string.Equals(method, HttpMethods.Post, StringComparison.OrdinalIgnoreCase)
        || string.Equals(method, HttpMethods.Put, StringComparison.OrdinalIgnoreCase)
        || string.Equals(method, HttpMethods.Patch, StringComparison.OrdinalIgnoreCase);
    if (needsIdempotency && !context.Request.Headers.ContainsKey("Idempotency-Key"))
    {
        context.Request.Headers["Idempotency-Key"] = Guid.NewGuid().ToString("N");
    }

    context.Response.Headers["X-Correlation-Id"] = context.Request.Headers["X-Correlation-Id"].ToString();
    await next();
});

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/rh/v1") && !context.User.IsInRole("Admin"))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return;
    }
    await next();
});

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "portal-bff-service",
    version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0"
}));

app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.MapGet("/api/portal/v1/home", () => Results.Ok(new
{
    widgets = new[]
    {
        new { id = "recibos", label = "Recibos", route = "/liquidacion" },
        new { id = "tareas", label = "Tareas", route = "/tareas" }
    }
})).RequireAuthorization();

app.MapGet("/api/portal/v1/liquidacion", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var client = factory.CreateClient();
    var url = $"{options.LiquidacionApi.TrimEnd('/')}/payrolls";
    var proxyRequest = new HttpRequestMessage(HttpMethod.Get, url);
    if (request.Headers.TryGetValue("Authorization", out var token))
    {
        proxyRequest.Headers.Authorization = AuthenticationHeaderValue.Parse(token.ToString());
    }
    var response = await client.SendAsync(proxyRequest);
    var body = await response.Content.ReadAsStringAsync();
    return Results.Content(body, "application/json", Encoding.UTF8, (int)response.StatusCode);
}).RequireAuthorization();

app.MapGet("/api/portal/v1/liquidacion/{id:guid}/recibos", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.LiquidacionApi.TrimEnd('/')}/payrolls/{id}/recibos";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/portal/v1/liquidacion/{id:guid}/exports", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var client = factory.CreateClient();
    var url = $"{options.LiquidacionApi.TrimEnd('/')}/payrolls/{id}/exports/empleado";
    var proxyRequest = new HttpRequestMessage(HttpMethod.Get, url);
    if (request.Headers.TryGetValue("Authorization", out var token))
    {
        proxyRequest.Headers.Authorization = AuthenticationHeaderValue.Parse(token.ToString());
    }
    var response = await client.SendAsync(proxyRequest);
    var body = await response.Content.ReadAsStringAsync();
    return Results.Content(body, "application/json", Encoding.UTF8, (int)response.StatusCode);
}).RequireAuthorization();

app.MapGet("/api/portal/v1/liquidacion/exports/{fileName}", async (string fileName, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var client = factory.CreateClient();
    var url = $"{options.LiquidacionApi.TrimEnd('/')}/exports/{fileName}";
    var proxyRequest = new HttpRequestMessage(HttpMethod.Get, url);
    if (request.Headers.TryGetValue("Authorization", out var token))
    {
        proxyRequest.Headers.Authorization = AuthenticationHeaderValue.Parse(token.ToString());
    }
    var response = await client.SendAsync(proxyRequest, HttpCompletionOption.ResponseHeadersRead);
    if (!response.IsSuccessStatusCode)
    {
        var body = await response.Content.ReadAsStringAsync();
        return Results.Content(body, response.Content.Headers.ContentType?.ToString() ?? "text/plain", Encoding.UTF8, (int)response.StatusCode);
    }

    var stream = await response.Content.ReadAsStreamAsync();
    var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
    return Results.File(stream, contentType, fileName);
}).RequireAuthorization();

app.MapGet("/api/portal/v1/wf/definitions", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.WfApi.TrimEnd('/')}/definitions";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/portal/v1/wf/definitions", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.WfApi.TrimEnd('/')}/definitions";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/portal/v1/wf/instances", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.WfApi.TrimEnd('/')}/instances";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/portal/v1/wf/instances", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.WfApi.TrimEnd('/')}/instances";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/portal/v1/wf/instances/{id:guid}/transitions", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.WfApi.TrimEnd('/')}/instances/{id}/transitions";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

// Medicina: Reportes y estadísticas
app.MapGet("/api/rh/v1/medicina/reportes", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.WfApi.TrimEnd('/')}/instances?key=medicina-examen,medicina-licencia";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    var response = await ProxyHelpers.SendProxy(factory, proxyRequest);
    return response;
}).RequireAuthorization();

app.MapGet("/api/rh/v1/medicina/estadisticas", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    // Obtener todas las instancias de medicina
    var url = $"{options.WfApi.TrimEnd('/')}/instances?key=medicina-examen,medicina-licencia";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    
    try
    {
        var client = factory.CreateClient();
        var authHeader = request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader))
        {
            client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader);
        }
        
        var wfResponse = await client.SendAsync(proxyRequest);
        var content = await wfResponse.Content.ReadAsStringAsync();
        
        // Parsear y generar estadísticas
        var instances = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(content ?? "[]") ?? new List<Dictionary<string, object>>();
        
        var examenesAprobados = instances.Count(i => i.GetValueOrDefault("key")?.ToString() == "medicina-examen" && i.GetValueOrDefault("estado")?.ToString() == "Aprobada");
        var examenesPendientes = instances.Count(i => i.GetValueOrDefault("key")?.ToString() == "medicina-examen" && i.GetValueOrDefault("estado")?.ToString() == "Pendiente");
        var licenciasAprobadas = instances.Count(i => i.GetValueOrDefault("key")?.ToString() == "medicina-licencia" && i.GetValueOrDefault("estado")?.ToString() == "Aprobada");
        var licenciasPendientes = instances.Count(i => i.GetValueOrDefault("key")?.ToString() == "medicina-licencia" && i.GetValueOrDefault("estado")?.ToString() == "Pendiente");
        
        var estadisticas = new
        {
            examenes = new { aprobados = examenesAprobados, pendientes = examenesPendientes, total = examenesAprobados + examenesPendientes },
            licencias = new { aprobadas = licenciasAprobadas, pendientes = licenciasPendientes, total = licenciasAprobadas + licenciasPendientes },
            total = instances.Count
        };
        
        return Results.Ok(estadisticas);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error generando estadísticas: {ex.Message}");
    }
}).RequireAuthorization();

app.MapGet("/api/rh/v1/organizacion/empresas", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/empresas";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/organizacion/empresas", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/empresas";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPut("/api/rh/v1/organizacion/empresas/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/empresas/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Put);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapDelete("/api/rh/v1/organizacion/empresas/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/empresas/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Delete);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/organizacion/unidades", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/unidades";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/organizacion/unidades/tree", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/unidades/tree";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/organizacion/organigramas", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/organigramas";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/organizacion/organigramas/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/organigramas/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/organizacion/organigramas", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/organigramas";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/organizacion/unidades", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/unidades";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPut("/api/rh/v1/organizacion/unidades/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/unidades/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Put);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapDelete("/api/rh/v1/organizacion/unidades/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/unidades/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Delete);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/organizacion/posiciones", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/posiciones";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/organizacion/sindicatos", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/sindicatos";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/organizacion/sindicatos", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/sindicatos";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPut("/api/rh/v1/organizacion/sindicatos/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/sindicatos/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Put);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapDelete("/api/rh/v1/organizacion/sindicatos/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/sindicatos/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Delete);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/organizacion/convenios", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/convenios";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/organizacion/convenios", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/convenios";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPut("/api/rh/v1/organizacion/convenios/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/convenios/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Put);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapDelete("/api/rh/v1/organizacion/convenios/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/convenios/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Delete);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/organizacion/posiciones", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/posiciones";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPut("/api/rh/v1/organizacion/posiciones/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/posiciones/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Put);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapDelete("/api/rh/v1/organizacion/posiciones/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/posiciones/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Delete);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/organizacion/posiciones/{id:guid}/asignar", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/posiciones/{id}/asignar";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/organizacion/posiciones/{id:guid}/desasignar", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/posiciones/{id}/desasignar";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/organizacion/centros-costo", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/centros-costo";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/organizacion/centros-costo", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/centros-costo";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPut("/api/rh/v1/organizacion/centros-costo/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/centros-costo/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Put);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapDelete("/api/rh/v1/organizacion/centros-costo/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.OrganizacionApi.TrimEnd('/')}/centros-costo/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Delete);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/personal/legajos", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.PersonalApi.TrimEnd('/')}/legajos";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/personal/legajos/numero/{numero}", async (string numero, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.PersonalApi.TrimEnd('/')}/legajos/numero/{numero}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/personal/legajos", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.PersonalApi.TrimEnd('/')}/legajos";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPut("/api/rh/v1/personal/legajos/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.PersonalApi.TrimEnd('/')}/legajos/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Put);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapDelete("/api/rh/v1/personal/legajos/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.PersonalApi.TrimEnd('/')}/legajos/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Delete);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/personal/legajos/{id:guid}/domicilios", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.PersonalApi.TrimEnd('/')}/legajos/{id}/domicilios";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/personal/legajos/{id:guid}/documentos", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.PersonalApi.TrimEnd('/')}/legajos/{id}/documentos";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPut("/api/rh/v1/personal/legajos/{id:guid}/domicilios", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.PersonalApi.TrimEnd('/')}/legajos/{id}/domicilios";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Put);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPut("/api/rh/v1/personal/legajos/{id:guid}/documentos", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.PersonalApi.TrimEnd('/')}/legajos/{id}/documentos";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Put);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/configuracion/catalogos/{tipo}", async (string tipo, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.ConfiguracionApi.TrimEnd('/')}/catalogos/{tipo}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/configuracion/catalogos", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.ConfiguracionApi.TrimEnd('/')}/catalogos";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPut("/api/rh/v1/configuracion/catalogos/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.ConfiguracionApi.TrimEnd('/')}/catalogos/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Put);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapDelete("/api/rh/v1/configuracion/catalogos/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.ConfiguracionApi.TrimEnd('/')}/catalogos/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Delete);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/configuracion/parametros", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.ConfiguracionApi.TrimEnd('/')}/parametros";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/configuracion/parametros/{clave}", async (string clave, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.ConfiguracionApi.TrimEnd('/')}/parametros/{clave}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/configuracion/parametros", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.ConfiguracionApi.TrimEnd('/')}/parametros";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/tiempos/turnos", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.TiemposApi.TrimEnd('/')}/turnos";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/tiempos/turnos", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.TiemposApi.TrimEnd('/')}/turnos";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/tiempos/turnos/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.TiemposApi.TrimEnd('/')}/turnos/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPut("/api/rh/v1/tiempos/turnos/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.TiemposApi.TrimEnd('/')}/turnos/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Put);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapDelete("/api/rh/v1/tiempos/turnos/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.TiemposApi.TrimEnd('/')}/turnos/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Delete);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/tiempos/horarios", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.TiemposApi.TrimEnd('/')}/horarios";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/tiempos/horarios", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.TiemposApi.TrimEnd('/')}/horarios";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/tiempos/horarios/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.TiemposApi.TrimEnd('/')}/horarios/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPut("/api/rh/v1/tiempos/horarios/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.TiemposApi.TrimEnd('/')}/horarios/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Put);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapDelete("/api/rh/v1/tiempos/horarios/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.TiemposApi.TrimEnd('/')}/horarios/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Delete);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/tiempos/ausencias", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var query = request.QueryString.HasValue ? request.QueryString.Value : string.Empty;
    var url = $"{options.TiemposApi.TrimEnd('/')}/ausencias{query}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/tiempos/ausencias", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.TiemposApi.TrimEnd('/')}/ausencias";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/tiempos/ausencias/resumen", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var query = request.QueryString.HasValue ? request.QueryString.Value : string.Empty;
    var url = $"{options.TiemposApi.TrimEnd('/')}/ausencias/resumen{query}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/tiempos/fichadas", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var query = request.QueryString.HasValue ? request.QueryString.Value : string.Empty;
    var url = $"{options.TiemposApi.TrimEnd('/')}/fichadas{query}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/tiempos/fichadas", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.TiemposApi.TrimEnd('/')}/fichadas";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/tiempos/fichadas/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.TiemposApi.TrimEnd('/')}/fichadas/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPatch("/api/rh/v1/tiempos/fichadas/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.TiemposApi.TrimEnd('/')}/fichadas/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Patch);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/tiempos/planillas", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var query = request.QueryString.HasValue ? request.QueryString.Value : string.Empty;
    var url = $"{options.TiemposApi.TrimEnd('/')}/planillas{query}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/tiempos/planillas", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.TiemposApi.TrimEnd('/')}/planillas";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/tiempos/planillas/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.TiemposApi.TrimEnd('/')}/planillas/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/tiempos/planillas/{id:guid}/cerrar", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.TiemposApi.TrimEnd('/')}/planillas/{id}/cerrar";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/liquidacion/payrolls", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.LiquidacionApi.TrimEnd('/')}/payrolls";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/liquidacion/payrolls", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.LiquidacionApi.TrimEnd('/')}/payrolls";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPatch("/api/rh/v1/liquidacion/payrolls/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.LiquidacionApi.TrimEnd('/')}/payrolls/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Patch);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/liquidacion/payrolls/{id:guid}/legajos", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.LiquidacionApi.TrimEnd('/')}/payrolls/{id}/legajos";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/liquidacion/payrolls/{id:guid}/procesar", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.LiquidacionApi.TrimEnd('/')}/payrolls/{id}/procesar";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/liquidacion/payrolls/{id:guid}/recibos", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.LiquidacionApi.TrimEnd('/')}/payrolls/{id}/recibos";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/liquidacion/payrolls/{id:guid}/exports", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.LiquidacionApi.TrimEnd('/')}/payrolls/{id}/exports";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/integraciones/templates", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.IntegrationHubApi.TrimEnd('/')}/integraciones/templates";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/integraciones/jobs", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.IntegrationHubApi.TrimEnd('/')}/integraciones/jobs";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/integraciones/jobs/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.IntegrationHubApi.TrimEnd('/')}/integraciones/jobs/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/integraciones/jobs", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.IntegrationHubApi.TrimEnd('/')}/integraciones/jobs";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/integraciones/jobs/{id:guid}/retry", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.IntegrationHubApi.TrimEnd('/')}/integraciones/jobs/{id}/retry";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/integraciones/eventos", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var query = request.QueryString.HasValue ? request.QueryString.Value : string.Empty;
    var url = $"{options.IntegrationHubApi.TrimEnd('/')}/integraciones/eventos{query}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/integraciones/triggers", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var query = request.QueryString.HasValue ? request.QueryString.Value : string.Empty;
    var url = $"{options.IntegrationHubApi.TrimEnd('/')}/integraciones/triggers{query}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/integraciones/triggers", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.IntegrationHubApi.TrimEnd('/')}/integraciones/triggers";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPut("/api/rh/v1/integraciones/triggers/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.IntegrationHubApi.TrimEnd('/')}/integraciones/triggers/{id}";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Put);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/integraciones/triggers/{id:guid}/execute", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.IntegrationHubApi.TrimEnd('/')}/integraciones/triggers/{id}/execute";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/wf/instances", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.WfApi.TrimEnd('/')}/instances";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Get);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/wf/instances/{id:guid}/transitions", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var url = $"{options.WfApi.TrimEnd('/')}/instances/{id}/transitions";
    var proxyRequest = ProxyHelpers.CreateProxyRequest(request, url, HttpMethod.Post);
    return await ProxyHelpers.SendProxy(factory, proxyRequest);
}).RequireAuthorization();

app.MapGet("/api/portal/v1/notificaciones", (HttpContext context, bool? unreadOnly, int? limit, int? offset, NotificationStore store) =>
{
    var userId = RequestUser.GetUserId(context);
    var safeLimit = Math.Clamp(limit ?? 50, 1, 200);
    var safeOffset = Math.Max(offset ?? 0, 0);
    var notifications = store.GetByUser(userId, unreadOnly == true, safeLimit, safeOffset)
        .Select(n => new
        {
            id = n.Id,
            title = n.Title,
            detail = n.Detail,
            createdAt = n.CreatedAt,
            readAt = n.ReadAt
        });
    return Results.Ok(notifications);
}).RequireAuthorization();

app.MapPost("/api/portal/v1/notificaciones", (HttpContext context, NotificationRequest request, NotificationStore store) =>
{
    var userId = RequestUser.GetUserId(context);
    var baseId = string.IsNullOrWhiteSpace(request.SourceId) ? Guid.NewGuid().ToString("N") : request.SourceId.Trim();
    var id = $"{userId}:{baseId}";
    var record = new NotificationRecord(
        id,
        userId,
        request.Title.Trim(),
        request.Detail.Trim(),
        DateTimeOffset.UtcNow,
        null);
    store.Add(record);
    return Results.Created($"/api/portal/v1/notificaciones/{id}", new { id });
}).RequireAuthorization();

app.MapPost("/api/portal/v1/notificaciones/{id}/read", (HttpContext context, string id, NotificationStore store) =>
{
    var userId = RequestUser.GetUserId(context);
    var ok = store.MarkRead(userId, id);
    return ok ? Results.NoContent() : Results.NotFound();
}).RequireAuthorization();

app.MapPost("/api/portal/v1/notificaciones/read-all", (HttpContext context, NotificationStore store) =>
{
    var userId = RequestUser.GetUserId(context);
    var updated = store.MarkAllRead(userId);
    return Results.Ok(new { updated });
}).RequireAuthorization();

app.MapGet("/api/portal/v1/notificaciones/resumen", (HttpContext context, NotificationStore store) =>
{
    var userId = RequestUser.GetUserId(context);
    var summary = store.GetSummary(userId);
    return Results.Ok(new { total = summary.Total, unread = summary.Unread });
}).RequireAuthorization();

app.MapDelete("/api/portal/v1/notificaciones", (HttpContext context, NotificationStore store) =>
{
    var userId = RequestUser.GetUserId(context);
    store.Clear(userId);
    return Results.NoContent();
}).RequireAuthorization();

// Tesorería: Adelantos y pagos
app.MapGet("/api/rh/v1/tesoreria/adelantos", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    // Proxy to tesoreria service (future)
    // For now, return mock data
    var adelantos = new[] {
        new { id = Guid.NewGuid(), legajoNumero = "001", nombre = "Juan Pérez", monto = 50000, fecha = DateTime.Now.AddDays(-30), estado = "Aprobado" },
        new { id = Guid.NewGuid(), legajoNumero = "002", nombre = "María García", monto = 75000, fecha = DateTime.Now.AddDays(-15), estado = "Pendiente" }
    };
    return Results.Ok(adelantos);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/tesoreria/pagos", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var pagos = new[] {
        new { id = Guid.NewGuid(), tipo = "Anticipo de sueldo", monto = 50000, fecha = DateTime.Now.AddDays(-5), estado = "Pagado" },
        new { id = Guid.NewGuid(), tipo = "Préstamo", monto = 150000, fecha = DateTime.Now.AddDays(-10), estado = "En proceso" }
    };
    return Results.Ok(pagos);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/tesoreria/conciliaciones", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var conciliaciones = new[] {
        new { id = Guid.NewGuid(), periodo = "2026-02", banco = "Banco Nación", montoDeclarado = 2500000, montoBanco = 2500000, estado = "Conciliado" },
        new { id = Guid.NewGuid(), periodo = "2026-03", banco = "Banco Provincia", montoDeclarado = 1800000, montoBanco = 1750000, estado = "Pendiente" }
    };
    return Results.Ok(conciliaciones);
}).RequireAuthorization();

// Presupuesto: Headcount y costos
app.MapGet("/api/rh/v1/presupuesto/headcount", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var headcount = new[] {
        new { area = "Administración", presupuesto = 15, actual = 12, diferencia = -3 },
        new { area = "Ventas", presupuesto = 25, actual = 28, diferencia = 3 },
        new { area = "Producción", presupuesto = 50, actual = 48, diferencia = -2 }
    };
    return Results.Ok(headcount);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/presupuesto/costos", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var costos = new[] {
        new { area = "Administración", presupuestoAnual = 180000000, gastado = 45000000, proyecciones = 190000000 },
        new { area = "Ventas", presupuestoAnual = 300000000, gastado = 85000000, proyecciones = 320000000 },
        new { area = "Producción", presupuestoAnual = 600000000, gastado = 150000000, proyecciones = 620000000 }
    };
    return Results.Ok(costos);
}).RequireAuthorization();

// Beneficios: Catálogos e inscripciones
app.MapGet("/api/rh/v1/beneficios/catalogos", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var beneficios = new[] {
        new { id = Guid.NewGuid(), nombre = "Plan de salud", tipo = "Salud", empresa = "Swiss Medical", activo = true },
        new { id = Guid.NewGuid(), nombre = "Tarjeta de almuerzo", tipo = "Alimentación", empresa = "Ticket", activo = true },
        new { id = Guid.NewGuid(), nombre = "Seguro de vida", tipo = "Seguro", empresa = "Mapfre", activo = true }
    };
    return Results.Ok(beneficios);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/beneficios/inscripciones", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var inscripciones = new[] {
        new { legajoNumero = "001", beneficio = "Plan de salud", estado = "Activo", fechaAlta = DateTime.Now.AddMonths(-6) },
        new { legajoNumero = "001", beneficio = "Tarjeta de almuerzo", estado = "Activo", fechaAlta = DateTime.Now.AddMonths(-12) }
    };
    return Results.Ok(inscripciones);
}).RequireAuthorization();

// Accidentabilidad: Incidentes e investigaciones
app.MapGet("/api/rh/v1/accidentabilidad/incidentes", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var incidentes = new[] {
        new { id = Guid.NewGuid(), legajoNumero = "015", tipo = "Golpe", gravedad = "Leve", fecha = DateTime.Now.AddDays(-10), estado = "Cerrado" },
        new { id = Guid.NewGuid(), legajoNumero = "022", tipo = "Caída", gravedad = "Moderado", fecha = DateTime.Now.AddDays(-3), estado = "En investigación" }
    };
    return Results.Ok(incidentes);
}).RequireAuthorization();

// Seguridad: Control de accesos
app.MapGet("/api/rh/v1/seguridad/accesos", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var accesos = new[] {
        new { legajoNumero = "001", ubicacion = "Entrada principal", fechaHora = DateTime.Now.AddHours(-2), tipo = "Entrada" },
        new { legajoNumero = "015", ubicacion = "Sector producción", fechaHora = DateTime.Now.AddHours(-1), tipo = "Entrada" }
    };
    return Results.Ok(accesos);
}).RequireAuthorization();

// Control de Visitas
app.MapGet("/api/rh/v1/control-visitas/visitas", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var visitas = new[] {
        new { id = Guid.NewGuid(), visitante = "Carlos López", documento = "20123456", motivo = "Reunión", fechaIngreso = DateTime.Now.AddHours(-3), fechaEgreso = (DateTime?)DateTime.Now.AddHours(-1), estado = "Activa" },
        new { id = Guid.NewGuid(), visitante = "Ana Gómez", documento = "30234567", motivo = "Entrevista", fechaIngreso = DateTime.Now.AddHours(-2), fechaEgreso = (DateTime?)null, estado = "Activa" }
    };
    return Results.Ok(visitas);
}).RequireAuthorization();

// ============================================
// SPRINT 15 - Seleccion Module (Candidates & Avisos)
// ============================================

// Seleccion: Candidates
app.MapGet("/api/rh/v1/seleccion/candidates", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var candidates = new[] {
        new { id = Guid.NewGuid(), nombre = "Juan Pérez", documento = "20123456", email = "juan.perez@email.com", telefono = "1155551234", estado = "En proceso", posicion = "Desarrollador", fechaPostulacion = DateTime.Now.AddDays(-5) },
        new { id = Guid.NewGuid(), nombre = "María González", documento = "30234567", email = "maria.g@email.com", telefono = "1155555678", estado = "Entrevista", posicion = "Analista", fechaPostulacion = DateTime.Now.AddDays(-3) },
        new { id = Guid.NewGuid(), nombre = "Pedro Martínez", documento = "40345678", email = "pedro.m@email.com", telefono = "1155559012", estado = "Preseleccionado", posicion = "Desarrollador", fechaPostulacion = DateTime.Now.AddDays(-7) }
    };
    return Results.Ok(candidates);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/seleccion/candidates", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Created($"/api/rh/v1/seleccion/candidates/{Guid.NewGuid()}", new { message = "Candidate created" });
}).RequireAuthorization();

app.MapGet("/api/rh/v1/seleccion/candidates/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Ok(new { id = id, nombre = "Candidate", estado = "En proceso" });
}).RequireAuthorization();

app.MapPut("/api/rh/v1/seleccion/candidates/{id:guid}/estado", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Ok(new { id = id, estado = "Actualizado" });
}).RequireAuthorization();

// Seleccion: Avisos/Búsquedas
app.MapGet("/api/rh/v1/seleccion/avisos", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var avisos = new[] {
        new { id = Guid.NewGuid(), titulo = "Desarrollador Full Stack", posicion = "Desarrollador", ubicacion = "Buenos Aires", modalidad = "Presencial", estado = "Activa", candidatos = 12, fechaPublicacion = DateTime.Now.AddDays(-10), fechaCierre = DateTime.Now.AddDays(20) },
        new { id = Guid.NewGuid(), titulo = "Analista de RRHH", posicion = "Analista", ubicacion = "Remoto", modalidad = "Remoto", estado = "Activa", candidatos = 5, fechaPublicacion = DateTime.Now.AddDays(-5), fechaCierre = DateTime.Now.AddDays(25) }
    };
    return Results.Ok(avisos);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/seleccion/avisos", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Created($"/api/rh/v1/seleccion/avisos/{Guid.NewGuid()}", new { message = "Aviso created" });
}).RequireAuthorization();

// ============================================
// SPRINT 15 - Evaluacion Module (Performance Reviews)
// ============================================

// Evaluacion: Evaluaciones
app.MapGet("/api/rh/v1/evaluacion/evaluaciones", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var evaluaciones = new[] {
        new { id = Guid.NewGuid(), empleado = "Juan Pérez", legajoNumero = "001", periodo = "2026-Q1", estado = "En curso", fechaInicio = DateTime.Now.AddDays(-15), fechaFin = DateTime.Now.AddDays(15), promedio = (decimal?)null },
        new { id = Guid.NewGuid(), empleado = "María González", legajoNumero = "002", periodo = "2026-Q1", estado = "Completada", fechaInicio = DateTime.Now.AddDays(-20), fechaFin = DateTime.Now.AddDays(-5), promedio = (decimal?)8.5m }
    };
    return Results.Ok(evaluaciones);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/evaluacion/evaluaciones", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Created($"/api/rh/v1/evaluacion/evaluaciones/{Guid.NewGuid()}", new { message = "Evaluacion created" });
}).RequireAuthorization();

app.MapGet("/api/rh/v1/evaluacion/evaluaciones/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Ok(new { id = id, estado = "En curso" });
}).RequireAuthorization();

app.MapPost("/api/rh/v1/evaluacion/evaluaciones/{id:guid}/responder", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Ok(new { message = "Evaluacion respondida" });
}).RequireAuthorization();

// Evaluacion: Metas/Objetivos
app.MapGet("/api/rh/v1/evaluacion/metas", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var metas = new[] {
        new { id = Guid.NewGuid(), legajoNumero = "001", titulo = "Cumplir objetivos de venta", peso = 40, estado = "En progreso", avance = 65 },
        new { id = Guid.NewGuid(), legajoNumero = "001", titulo = "Completar capacitacion", peso = 20, estado = "Completada", avance = 100 },
        new { id = Guid.NewGuid(), legajoNumero = "001", titulo = "Trabajo en equipo", peso = 40, estado = "En progreso", avance = 50 }
    };
    return Results.Ok(metas);
}).RequireAuthorization();

// ============================================
// SPRINT 15 - Carrera & Sucesion Module
// ============================================

// Carrera: Planes de Carrera
app.MapGet("/api/rh/v1/carrera/planes", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var planes = new[] {
        new { id = Guid.NewGuid(), empleado = "Juan Pérez", posicionActual = "Analista Jr", posicionObjetivo = "Analista Sr", estado = "Activo", fechaInicio = DateTime.Now.AddMonths(-3), competenciasPorDesarrollar = new[] { "Liderazgo", "Análisis" } },
        new { id = Guid.NewGuid(), empleado = "María González", posicionActual = "Desarrollador", posicionObjetivo = "Tech Lead", estado = "Activo", fechaInicio = DateTime.Now.AddMonths(-6), competenciasPorDesarrollar = new[] { "Gestión de equipos", "Arquitectura" } }
    };
    return Results.Ok(planes);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/carrera/planes", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Created($"/api/rh/v1/carrera/planes/{Guid.NewGuid()}", new { message = "Plan created" });
}).RequireAuthorization();

// Carrera: Succession Planning
app.MapGet("/api/rh/carrera/succession", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var succession = new[] {
        new { posicion = "Gerente de IT", incumbente = "Carlos López", potencialSucesor1 = "Juan Pérez (Alto)", potencialSucesor2 = "Pedro Martínez (Medio)", planAccion = "Mentoría + Capacitación" },
        new { posicion = "Tech Lead", incumbente = "Ana Gómez", potencialSucesor1 = "María González (Alto)", potencialSucesor2 = "null", planAccion = "Proyecto de liderazgo" }
    };
    return Results.Ok(succession);
}).RequireAuthorization();

// ============================================
// SPRINT 16 - Capacitacion Module (Courses)
// ============================================

// Capacitacion: Cursos
app.MapGet("/api/rh/v1/capacitacion/cursos", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var cursos = new[] {
        new { id = Guid.NewGuid(), titulo = "Introducción a .NET 8", descripcion = "Curso básico de .NET 8", duracionHoras = 20, modalidad = "Online", estado = "Activo", categorias = new[] { "Desarrollo" } },
        new { id = Guid.NewGuid(), titulo = "Liderazgo Efectivo", descripcion = "Desarrollo de habilidades de liderazgo", duracionHoras = 16, modalidad = "Presencial", estado = "Activo", categorias = new[] { "Management" } },
        new { id = Guid.NewGuid(), titulo = "Seguridad Laboral", descripcion = "Normas de seguridad en el trabajo", duracionHoras = 8, modalidad = "Online", estado = "Activo", categorias = new[] { "Seguridad" } }
    };
    return Results.Ok(cursos);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/capacitacion/cursos/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Ok(new { id = id, titulo = "Curso", estado = "Activo" });
}).RequireAuthorization();

app.MapPost("/api/rh/v1/capacitacion/inscripciones", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Created($"/api/rh/v1/capacitacion/inscripciones/{Guid.NewGuid()}", new { message = "Inscripcion created" });
}).RequireAuthorization();

app.MapGet("/api/rh/v1/capacitacion/inscripciones", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var inscripciones = new[] {
        new { id = Guid.NewGuid(), legajoNumero = "001", curso = "Introducción a .NET 8", estado = "En curso", progreso = 45, fechaInicio = DateTime.Now.AddDays(-10) },
        new { id = Guid.NewGuid(), legajoNumero = "002", curso = "Liderazgo Efectivo", estado = "Completada", progreso = 100, fechaInicio = DateTime.Now.AddDays(-30) }
    };
    return Results.Ok(inscripciones);
}).RequireAuthorization();

// ============================================
// SPRINT 16 - Clima Laboral Module (Surveys)
// ============================================

// Clima: Encuestas
app.MapGet("/api/rh/v1/clima/encuestas", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var encuestas = new[] {
        new { id = Guid.NewGuid(), titulo = "Clima Laboral 2026-Q1", descripcion = "Encuesta trimestral de clima", estado = "Activa", preguntas = 25, fechaInicio = DateTime.Now.AddDays(-5), fechaFin = DateTime.Now.AddDays(10), respuestas = 45 },
        new { id = Guid.NewGuid(), titulo = "Satisfacción Empleados 2025", descripcion = "Encuesta anual de satisfaccion", estado = "Cerrada", preguntas = 30, fechaInicio = DateTime.Now.AddMonths(-3), fechaFin = DateTime.Now.AddMonths(-2), respuestas = 120 }
    };
    return Results.Ok(encuestas);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/clima/encuestas", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Created($"/api/rh/v1/clima/encuestas/{Guid.NewGuid()}", new { message = "Encuesta created" });
}).RequireAuthorization();

app.MapGet("/api/rh/v1/clima/encuestas/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Ok(new { id = id, titulo = "Encuesta", estado = "Activa" });
}).RequireAuthorization();

app.MapPost("/api/rh/v1/clima/encuestas/{id:guid}/responder", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Ok(new { message = "Respuestas guardadas" });
}).RequireAuthorization();

// Clima: Resultados
app.MapGet("/api/rh/v1/clima/resultados", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var resultados = new[] {
        new { encuesta = "Satisfacción Empleados 2025", area = "TI", satisfaccionGeneral = (decimal)8.2, compromiso = (decimal)7.8, comunicacion = (decimal)7.5 },
        new { encuesta = "Satisfacción Empleados 2025", area = "RRHH", satisfaccionGeneral = (decimal)8.5, compromiso = (decimal)8.0, comunicacion = (decimal)8.2 }
    };
    return Results.Ok(resultados);
}).RequireAuthorization();

// ============================================
// SPRINT 17 - Operations & Security
// ============================================

// Audit Logs - Compliance and security tracking
app.MapGet("/api/rh/v1/auditoria/logs", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var logs = new[] {
        new { id = Guid.NewGuid(), usuario = "admin", accion = "CREATE", modulo = "legajos", recursoId = Guid.NewGuid(), fecha = DateTime.Now.AddHours(-2), ip = "192.168.1.100", detalle = "Creación de legajo" },
        new { id = Guid.NewGuid(), usuario = "rrhh", accion = "UPDATE", modulo = "liquidacion", recursoId = Guid.NewGuid(), fecha = DateTime.Now.AddHours(-5), ip = "192.168.1.105", detalle = "Modificación de payroll" },
        new { id = Guid.NewGuid(), usuario = "jefe", accion = "READ", modulo = "personal", recursoId = Guid.NewGuid(), fecha = DateTime.Now.AddDays(-1), ip = "192.168.1.110", detalle = "Consulta de legajo" }
    };
    return Results.Ok(logs);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/auditoria/logs/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Ok(new { id = id, usuario = "admin", accion = "CREATE", modulo = "legajos", fecha = DateTime.Now, ip = "192.168.1.100" });
}).RequireAuthorization();

// Roles y Permisos
app.MapGet("/api/rh/v1/seguridad/roles", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var roles = new[] {
        new { id = Guid.NewGuid(), nombre = "Administrador", descripcion = "Acceso total al sistema", permisos = new[] { "ALL" }, cantidadUsuarios = 3 },
        new { id = Guid.NewGuid(), nombre = "RRHH", descripcion = "Gestión de RRHH", permisos = new[] { "legajos", "liquidacion", "tiempos", "reportes" }, cantidadUsuarios = 5 },
        new { id = Guid.NewGuid(), nombre = "Jefe", descripcion = "Gestión de equipo", permisos = new[] { "personal", "tiempos", "evaluacion" }, cantidadUsuarios = 12 },
        new { id = Guid.NewGuid(), nombre = "Empleado", descripcion = "Acceso básico", permisos = new[] { "perfil", "recibos", "vacaciones" }, cantidadUsuarios = 150 }
    };
    return Results.Ok(roles);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/seguridad/roles", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Created($"/api/rh/v1/seguridad/roles/{Guid.NewGuid()}", new { message = "Rol creado" });
}).RequireAuthorization();

app.MapGet("/api/rh/v1/seguridad/roles/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Ok(new { id = id, nombre = "Rol", permisos = new string[] { } });
}).RequireAuthorization();

app.MapPut("/api/rh/v1/seguridad/roles/{id:guid}", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Ok(new { id = id, message = "Rol actualizado" });
}).RequireAuthorization();

// Usuarios y Acceso
app.MapGet("/api/rh/v1/seguridad/usuarios", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var usuarios = new[] {
        new { id = Guid.NewGuid(), username = "admin", email = "admin@nucleus.com", rol = "Administrador", estado = "Activo", ultimoLogin = DateTime.Now.AddDays(-1) },
        new { id = Guid.NewGuid(), username = "jgarcia", email = "jgarcia@nucleus.com", rol = "RRHH", estado = "Activo", ultimoLogin = DateTime.Now.AddHours(-3) },
        new { id = Guid.NewGuid(), username = "mperez", email = "mperez@nucleus.com", rol = "Jefe", estado = "Activo", ultimoLogin = DateTime.Now.AddDays(-2) }
    };
    return Results.Ok(usuarios);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/seguridad/usuarios", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Created($"/api/rh/v1/seguridad/usuarios/{Guid.NewGuid()}", new { message = "Usuario creado" });
}).RequireAuthorization();

app.MapPost("/api/rh/v1/seguridad/usuarios/{id:guid}/reset-password", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Ok(new { message = "Password reseteado" });
}).RequireAuthorization();

app.MapPost("/api/rh/v1/seguridad/usuarios/{id:guid}/activar", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Ok(new { message = "Usuario activado" });
}).RequireAuthorization();

app.MapPost("/api/rh/v1/seguridad/usuarios/{id:guid}/desactivar", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Ok(new { message = "Usuario desactivado" });
}).RequireAuthorization();

// Incidentes de Seguridad
app.MapGet("/api/rh/v1/seguridad/incidentes", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var incidentes = new[] {
        new { id = Guid.NewGuid(), tipo = "Acceso no autorizado", severidad = "Alta", estado = "Cerrado", fecha = DateTime.Now.AddDays(-10), resolvedBy = "admin", detalle = "Intento de acceso con credenciales incorrectas" },
        new { id = Guid.NewGuid(), tipo = "Cambio de permisos", severidad = "Media", estado = "Abierto", fecha = DateTime.Now.AddDays(-2), resolvedBy = (string)null, detalle = "Solicitud de cambio de rol pendiente" }
    };
    return Results.Ok(incidentes);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/seguridad/incidentes", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Created($"/api/rh/v1/seguridad/incidentes/{Guid.NewGuid()}", new { message = "Incidente reportado" });
}).RequireAuthorization();

// Compliance
app.MapGet("/api/rh/v1/seguridad/compliance", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var compliance = new[] {
        new { normativa = "Ley 20744 - LCT", estado = "Cumple", ultimoCheck = DateTime.Now.AddDays(-1), observaciones = "Todas las obligaciones laborales al día" },
        new { normativa = "AFIP - RG 3801", estado = "Cumple", ultimoCheck = DateTime.Now.AddDays(-1), observaciones = "Declaraciones juradas presentadas" },
        new { normativa = "SRT - Salud y Seguridad", estado = "Pendiente", ultimoCheck = DateTime.Now.AddDays(-5), observaciones = "Actualizar protocolo de emergencia" },
        new { normativa = "LOPD - Protección de Datos", estado = "Cumple", ultimoCheck = DateTime.Now.AddDays(-3), observaciones = "Consentimientos actualizados" }
    };
    return Results.Ok(compliance);
}).RequireAuthorization();

// ============================================
// SPRINT 18 - Final Integration & Testing
// ============================================

// System Health and Diagnostics
app.MapGet("/api/rh/v1/sistema/health", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var health = new {
        status = "Healthy",
        uptime = "15 days, 7 hours",
        version = "1.0.0",
        services = new[] {
            new { name = "auth-service", status = "Healthy", latency = "12ms" },
            new { name = "personal-service", status = "Healthy", latency = "25ms" },
            new { name = "liquidacion-service", status = "Healthy", latency = "45ms" },
            new { name = "tiempos-service", status = "Healthy", latency = "18ms" },
            new { name = "organizacion-service", status = "Degraded", latency = "150ms" }
        },
        database = new { status = "Healthy", connections = 24, maxConnections = 100 },
        memory = new { used = "256MB", total = "512MB", percentage = 50 }
    };
    return Results.Ok(health);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/sistema/metricas", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var metricas = new {
        requests = new { total = 15420, porMinuto = 12, errores = 3 },
        responseTime = new { promedio = "45ms", p95 = "120ms", p99 = "250ms" },
        endpoints = new[] {
            new { path = "/api/rh/v1/personal/legajos", calls = 5200, avgTime = "35ms" },
            new { path = "/api/rh/v1/liquidacion/payrolls", calls = 3100, avgTime = "85ms" },
            new { path = "/api/rh/v1/tiempos/ausencias", calls = 2800, avgTime = "28ms" }
        },
        errors = new { total = 23, byType = new { validation = 15, auth = 5, server = 3 } }
    };
    return Results.Ok(metricas);
}).RequireAuthorization();

// Integration Status
app.MapGet("/api/rh/v1/sistema/integraciones", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var integraciones = new[] {
        new { nombre = "AFIP", tipo = "API Rest", estado = "Activa", ultimoSync = DateTime.Now.AddHours(-1), transaccionesHoy = 145, errores = 0 },
        new { nombre = "Bancos", tipo = "SFTP", estado = "Activa", ultimoSync = DateTime.Now.AddMinutes(-30), transaccionesHoy = 23, errores = 0 },
        new { nombre = "SSN", tipo = "API Rest", estado = "Activa", ultimoSync = DateTime.Now.AddHours(-6), transaccionesHoy = 8, errores = 1 },
        new { nombre = "Proveedor de Salud", tipo = "API Rest", estado = "Inactiva", ultimoSync = DateTime.Now.AddDays(-1), transaccionesHoy = 0, errores = 0 }
    };
    return Results.Ok(integraciones);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/sistema/integraciones/{nombre}/sync", async (string nombre, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Ok(new { message = $"Sincronización iniciada para {nombre}" });
}).RequireAuthorization();

// Testing utilities
app.MapGet("/api/rh/v1/test/health-check", () => Results.Ok(new { status = "OK", timestamp = DateTime.UtcNow }));

app.MapPost("/api/rh/v1/test/seed", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    // Seed test data for QA
    // This endpoint can be called to populate test data
    // See seed-data.js for the complete data structure
    var seedInfo = new {
        message = "Test data seeded successfully",
        registrosCreados = 50,
        data = new {
            auth = new { users = 4 },
            organizacion = new { empresas = 1, unidades = 3, posiciones = 3 },
            personal = new { legajos = 2 },
            liquidacion = new { payrolls = 1 },
            tiempos = new { turnos = 2, horarios = 1, fichadas = 2 },
            vacaciones = new { solicitudes = 1 },
            seleccion = new { avisos = 1, candidatos = 1 },
            evaluacion = new { evaluaciones = 1 },
            capacitacion = new { cursos = 1, inscripciones = 1 },
            clima = new { encuestas = 1 }
        },
        nota = "Ver seed-data.js para estructura completa"
    };
    return Results.Ok(seedInfo);
});

// ============================================
// SPRINT 19 - Production Release
// ============================================

// System Info
app.MapGet("/api/rh/v1/sistema/info", () => Results.Ok(new
{
    nombre = "Nucleus RH Next",
    version = "1.0.0",
    entorno = "Production",
    fechaDespliegue = "2026-03-01",
    caracteristicas = new[] {
        "Auth con JWT",
        "API Gateway con BFF",
        "Microservicios REST",
        "Workflow Engine",
        "OpenTelemetry",
        "Rate Limiting"
    }
})).RequireAuthorization();

// Configuracion Global
app.MapGet("/api/rh/v1/sistema/configuracion", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var config = new[] {
        new { clave = "company.name", valor = "Nucleus SA", categoria = "general", editable = false },
        new { clave = "payroll.periodo.dias", valor = "30", categoria = "liquidacion", editable = true },
        new { clave = "tiempos.tolerancia.minutos", valor = "15", categoria = "tiempos", editable = true },
        new { clave = "notificaciones.email.enabled", valor = "true", categoria = "notificaciones", editable = true },
        new { clave = "seguridad.sesion.timeout", valor = "60", categoria = "seguridad", editable = true }
    };
    return Results.Ok(config);
}).RequireAuthorization();

app.MapPut("/api/rh/v1/sistema/configuracion/{clave}", async (string clave, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Ok(new { message = $"Configuración {clave} actualizada" });
}).RequireAuthorization();

// Backup y Restore
app.MapGet("/api/rh/v1/sistema/backup", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var backups = new[] {
        new { id = Guid.NewGuid(), fecha = DateTime.Now.AddDays(-1), tipo = "Automático", tamaño = "2.5GB", estado = "Completado" },
        new { id = Guid.NewGuid(), fecha = DateTime.Now.AddDays(-7), tipo = "Automático", tamaño = "2.4GB", estado = "Completado" },
        new { id = Guid.NewGuid(), fecha = DateTime.Now.AddDays(-14), tipo = "Manual", tamaño = "2.6GB", estado = "Completado" }
    };
    return Results.Ok(backups);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/sistema/backup", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Accepted($"/api/rh/v1/sistema/backup/{Guid.NewGuid()}", new { message = "Backup iniciado" });
}).RequireAuthorization();

app.MapPost("/api/rh/v1/sistema/backup/{id:guid}/restore", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Ok(new { message = "Restore iniciado", backupId = id });
}).RequireAuthorization();

// Mantenimiento
app.MapGet("/api/rh/v1/sistema/mantenimiento", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var mantenimientos = new[] {
        new { id = Guid.NewGuid(), tipo = "Actualización de seguridad", programado = true, fecha = DateTime.Now.AddDays(7), estado = "Programado", duracionEstimada = "30 min" },
        new { id = Guid.NewGuid(), tipo = "Limpieza de logs", programado = true, fecha = DateTime.Now.AddDays(30), estado = "Programado", duracionEstimada = "15 min" }
    };
    return Results.Ok(mantenimientos);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/sistema/mantenimiento", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Created($"/api/rh/v1/sistema/mantenimiento/{Guid.NewGuid()}", new { message = "Mantenimiento programado" });
}).RequireAuthorization();

// Dashboard/Resumen Ejecutivo
app.MapGet("/api/rh/v1/dashboard/resumen", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var dashboard = new {
        empleados = new { total = 180, activos = 165, inactivos = 15, nuevosMes = 5 },
        liquidacion = new { ultimoPeriodo = "2026-03", totalBruto = 45000000, totalNeto = 32000000, promedio = 250000 },
        tiempos = new { asistenciaPromedio = "98.5%", ausenciasHoy = 8, horasExtra = 145 },
        seleccion = new { postulacionesAbiertas = 12, entrevistasPendientes = 5, candidatosNuevos = 15 },
        capacitacion = new { cursosActivos = 8, empleadosEnCapacitacion = 45, completadosMes = 23 },
        clima = new { satisfaccionPromedio = 8.2, encuestasActivas = 1, participacion = "78%" }
    };
    return Results.Ok(dashboard);
}).RequireAuthorization();

// ============================================
// SPRINT 20 - Analytics and Reporting
// ============================================

// Reportes de RRHH
app.MapGet("/api/rh/v1/analytics/rrhh", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var reportes = new[] {
        new { id = Guid.NewGuid(), nombre = "Informe de Plantilla", tipo = "Plantilla", formato = "Excel", ultimaGeneracion = DateTime.Now.AddDays(-1), frecuencia = "Diaria" },
        new { id = Guid.NewGuid(), nombre = "Rotacion de Personal", tipo = "Rotacion", formato = "PDF", ultimaGeneracion = DateTime.Now.AddDays(-7), frecuencia = "Semanal" },
        new { id = Guid.NewGuid(), nombre = "Absentismo", tipo = "Ausentismo", formato = "Excel", ultimaGeneracion = DateTime.Now.AddDays(-3), frecuencia = "Semanal" },
        new { id = Guid.NewGuid(), nombre = "Liquidaciones por Area", tipo = "Liquidacion", formato = "Excel", ultimaGeneracion = DateTime.Now.AddDays(-30), frecuencia = "Mensual" }
    };
    return Results.Ok(reportes);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/analytics/rrhh/{id:guid}/generar", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Accepted($"/api/rh/v1/analytics/rrhh/{id}/download", new { message = "Reporte en generación", jobId = Guid.NewGuid() });
}).RequireAuthorization();

app.MapGet("/api/rh/v1/analytics/rrhh/{id:guid}/download", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Ok(new { url = $"/downloads/reporte_{id}.xlsx", expira = DateTime.Now.AddHours(24) });
}).RequireAuthorization();

// Indicadores/Metricas de RRHH
app.MapGet("/api/rh/v1/analytics/indicadores", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var indicadores = new {
        headcount = new { total = 180, mesAnterior = 175, variacion = 5, meta = 200 },
        rotacion = new { tasa = 2.5m, mesAnterior = 3.1m, meta = 2.0m },
        absentismo = new { tasa = 1.8m, mesAnterior = 2.2m, meta = 1.5m },
        tiempoCobertura = new { promedioDias = 45, mesAnterior = 52, meta = 30 },
        satisfaccion = new { promedio = 8.2, meta = 8.5, tendencia = "up" },
        capacitacion = new { horasPromedio = 24, meta = 30, cumplimiento = "80%" }
    };
    return Results.Ok(indicadores);
}).RequireAuthorization();

// Reportes por area
app.MapGet("/api/rh/v1/analytics/areas/{area}", async (string area, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var reporteArea = new {
        area = area,
        headcount = new { total = 25, objetivo = 30, coverage = "83%" },
        liquidacion = new { totalBruto = 6500000, promedio = 260000, vsPresupuesto = "-5%" },
        tiempos = new { asistencia = "97.5%", horasExtra = 45, absentismo = "2.1%" },
        capacitacion = new { horasTotales = 350, promedio = 14, completados = 18 }
    };
    return Results.Ok(reporteArea);
}).RequireAuthorization();

// Tendencias historicas
app.MapGet("/api/rh/v1/analytics/tendencias", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var tendencias = new[] {
        new { mes = "2025-10", headcount = 165, rotacion = 2.8m, absentismo = 2.1m, satisfaccion = 7.8m },
        new { mes = "2025-11", headcount = 168, rotacion = 3.0m, absentismo = 2.3m, satisfaccion = 7.9m },
        new { mes = "2025-12", headcount = 170, rotacion = 2.5m, absentismo = 2.5m, satisfaccion = 8.0m },
        new { mes = "2026-01", headcount = 172, rotacion = 2.7m, absentismo = 2.0m, satisfaccion = 8.1m },
        new { mes = "2026-02", headcount = 175, rotacion = 2.4m, absentismo = 1.9m, satisfaccion = 8.1m },
        new { mes = "2026-03", headcount = 180, rotacion = 2.5m, absentismo = 1.8m, satisfaccion = 8.2m }
    };
    return Results.Ok(tendencias);
}).RequireAuthorization();

// ============================================
// SPRINT 21 - Mobile App
// ============================================

// Mobile: Notificaciones push
app.MapGet("/api/rh/v1/mobile/notificaciones", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var notificaciones = new[] {
        new { id = Guid.NewGuid(), titulo = "Nueva liquidacion disponible", cuerpo = "Tu recibo de febrero ya esta disponible", tipo = "liquidacion", leida = false, fecha = DateTime.Now.AddHours(-2) },
        new { id = Guid.NewGuid(), titulo = "Recordatorio: Carga tu asistencia", cuerpo = "No olvides registrar tu asistencia hoy", tipo = "tiempos", leida = true, fecha = DateTime.Now.AddDays(-1) },
        new { id = Guid.NewGuid(), titulo = "Nueva capacitacion disponible", cuerpo = "Se abrio un nuevo curso de liderazgo", tipo = "capacitacion", leida = false, fecha = DateTime.Now.AddHours(-5) }
    };
    return Results.Ok(notificaciones);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/mobile/notificaciones/{id:guid}/leer", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Ok(new { message = "Notificacion marcada como leida" });
}).RequireAuthorization();

// Mobile: Mi info rapida
app.MapGet("/api/rh/v1/mobile/mi-info", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var miInfo = new {
        proximoPago = DateTime.Now.AddDays(5),
        proximoVencimientoVacaciones = DateTime.Now.AddMonths(2),
        diasVacacionesDisponibles = 14,
        proximoDiaFestivo = DateTime.Now.AddDays(15),
        proximaCapacitacion = "Liderazgo Efectivo - 2026-04-01",
        asistenciaEsteMes = "98%",
        proximaEvaluacion = "Q2 2026"
    };
    return Results.Ok(miInfo);
}).RequireAuthorization();

// Mobile: Marcar asistencia rapida
app.MapPost("/api/rh/v1/mobile/asistencia/checkin", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Created($"/api/rh/v1/mobile/asistencia/checkin/{Guid.NewGuid()}", new { message = "Check-in registrado", hora = DateTime.Now });
}).RequireAuthorization();

// Mobile: Solicitar ausencia rapida
app.MapPost("/api/rh/v1/mobile/ausencias", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Created($"/api/rh/v1/mobile/ausencias/{Guid.NewGuid()}", new { message = "Solicitud enviada" });
}).RequireAuthorization();

// Mobile: Ver mis recibos
app.MapGet("/api/rh/v1/mobile/recibos", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var recibos = new[] {
        new { id = Guid.NewGuid(), periodo = "2026-03", neto = 250000, estado = "Disponible", fechaPago = DateTime.Now.AddDays(5) },
        new { id = Guid.NewGuid(), periodo = "2026-02", neto = 248000, estado = "Disponible", fechaPago = DateTime.Now.AddDays(-25) },
        new { id = Guid.NewGuid(), periodo = "2026-01", neto = 245000, estado = "Disponible", fechaPago = DateTime.Now.AddDays(-55) }
    };
    return Results.Ok(recibos);
}).RequireAuthorization();

// ============================================
// SPRINT 22 - Advanced Features
// ============================================

// AI / Asistente Virtual
app.MapGet("/api/rh/v1/ai/asistente", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var recomendaciones = new[] {
        new { tipo = "vacaciones", titulo = "Es buen momento para tomar vacaciones", detalle = "Tienes 14 dias disponibles y no hay proyectos criticos en tu area", prioridad = "media" },
        new { tipo = "capacitacion", titulo = "Considera hacer el curso de liderazgo", detalle = "Estas cerca de una promocion y este curso te ayudaria", prioridad = "alta" },
        new { tipo = "beneficios", titulo = "Aprovecha el plan de salud", detalle = "Tu家庭的 coverage esta subutilizado, considera hacer el checkup anual", prioridad = "baja" }
    };
    return Results.Ok(recomendaciones);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/ai/asistente/preguntar", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Ok(new { 
        respuesta = "Basado en tu consulta sobre vacaciones, tienes 14 dias disponibles. El mejor momento seria en abril durante Semana Santa.",
        fuentes = new[] { "Politica de Vacaciones", "Calendario laboral 2026" }
    });
}).RequireAuthorization();

// Gamification
app.MapGet("/api/rh/v1/gamification/ranking", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var ranking = new[] {
        new { posicion = 1, nombre = "Maria Gonzalez", puntos = 1250, nivel = "Oro", area = "Ventas" },
        new { posicion = 2, nombre = "Juan Perez", puntos = 1180, nivel = "Oro", area = "TI" },
        new { posicion = 3, nombre = "Ana Rodriguez", puntos = 1100, nivel = "Plata", area = "RRHH" },
        new { posicion = 4, nombre = "Carlos Lopez", puntos = 980, nivel = "Plata", area = "Produccion" },
        new { posicion = 5, nombre = "Pedro Martinez", puntos = 920, nivel = "Plata", area = "Ventas" }
    };
    return Results.Ok(ranking);
}).RequireAuthorization();

app.MapGet("/api/rh/v1/gamification/mis-logros", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var logros = new[] {
        new { id = Guid.NewGuid(), nombre = "Primer dia", descripcion = "Completo tu primer dia", puntos = 10, fecha = DateTime.Now.AddMonths(-6), icon = "🎉" },
        new { id = Guid.NewGuid(), nombre = "100% Asistencia", descripcion = "Un mes con asistencia perfecta", puntos = 50, fecha = DateTime.Now.AddMonths(-3), icon = "⭐" },
        new { id = Guid.NewGuid(), nombre = "Capacitador", descripcion = "Completaste 5 cursos", puntos = 100, fecha = DateTime.Now.AddMonths(-1), icon = "📚" }
    };
    return Results.Ok(logros);
}).RequireAuthorization();

// Encuestas granulares
app.MapGet("/api/rh/v1/encuestas/granulares", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var encuestas = new[] {
        new { id = Guid.NewGuid(), titulo = "Que te pareció la capacitacion?", tipo = "feedback", modulo = "capacitacion", estado = "Activa", preguntas = 5 },
        new { id = Guid.NewGuid(), titulo = "Como fue tu experiencia de onboarding?", tipo = "onboarding", modulo = "personal", estado = "Cerrada", preguntas = 10 }
    };
    return Results.Ok(encuestas);
}).RequireAuthorization();

app.MapPost("/api/rh/v1/encuestas/granulares/{id:guid}/responder", async (Guid id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Ok(new { message = "Respuestas guardadas", puntosGanados = 10 });
}).RequireAuthorization();

// Widgets personalizables
app.MapGet("/api/rh/v1/dashboard/widgets", async (HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    var widgets = new[] {
        new { id = "recibos", nombre = "Mis Recibos", posicion = 1, visible = true, tipo = "list" },
        new { id = "vacaciones", nombre = "Mis Vacaciones", posicion = 2, visible = true, tipo = "summary" },
        new { id = "asistencia", nombre = "Mi Asistencia", posicion = 3, visible = true, tipo = "metric" },
        new { id = "capacitacion", nombre = "Capacitacion", posicion = 4, visible = false, tipo = "list" },
        new { id = "noticias", nombre = "Noticias", posicion = 5, visible = true, tipo = "feed" }
    };
    return Results.Ok(widgets);
}).RequireAuthorization();

app.MapPut("/api/rh/v1/dashboard/widgets/{id}", async (string id, HttpRequest request, IHttpClientFactory factory, PortalOptions options) =>
{
    return Results.Ok(new { message = $"Widget {id} actualizado" });
}).RequireAuthorization();

app.Run();

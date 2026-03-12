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
        if (!string.IsNullOrWhiteSpace(rawOrigins))
        {
            var origins = rawOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
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

var otelEnabled = builder.Configuration.GetValue("OpenTelemetry:Enabled", false);
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
    context.Response.Headers["X-Trace-Id"] = traceId;
    var sw = Stopwatch.StartNew();
    app.Logger.LogInformation("HTTP {Method} {Path} start TraceId={TraceId}", context.Request.Method, context.Request.Path, traceId);
    await next();
    sw.Stop();
    app.Logger.LogInformation("HTTP {Method} {Path} {StatusCode} {ElapsedMs}ms TraceId={TraceId}",
        context.Request.Method, context.Request.Path, context.Response.StatusCode, sw.ElapsedMilliseconds, traceId);
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

app.Run();

public partial class Program { }

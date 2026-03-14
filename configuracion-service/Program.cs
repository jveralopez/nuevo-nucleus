using System.Text;
using ConfiguracionService.Domain.Requests;
using ConfiguracionService.Infrastructure;
using ConfiguracionService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

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

builder.Services.AddDbContext<ConfiguracionDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ConfiguracionDb")));
builder.Services.AddScoped<ConfiguracionCatalogosService>();

var dataProtectionPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(dataProtectionPath))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
        .SetApplicationName("nucleus");
}

var authOptions = new AuthOptions();
builder.Configuration.Bind("Auth", authOptions);
builder.Services.AddSingleton(authOptions);

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
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
    .AddDbContextCheck<ConfiguracionDbContext>("db", tags: new[] { "ready" });

var otelEnabledRaw = builder.Configuration["OpenTelemetry:Enabled"];
var otelEnabled = bool.TryParse(otelEnabledRaw, out var otelEnabledValue) && otelEnabledValue;
if (otelEnabled)
{
    var serviceName = builder.Configuration.GetValue<string>("OpenTelemetry:ServiceName") ?? "configuracion-service";
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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ConfiguracionDbContext>();
    var applyMigrationsRaw = app.Configuration["Database:ApplyMigrations"];
    var applyMigrations = bool.TryParse(applyMigrationsRaw, out var applyMigrationsValue)
        ? applyMigrationsValue
        : app.Environment.IsDevelopment();
    if (applyMigrations)
    {
        db.Database.Migrate();
    }
}

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "configuracion-service",
    version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0"
}));

app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.MapHealthChecks("/readyz", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapGet("/catalogos/{tipo}", async (string tipo, ConfiguracionCatalogosService service) =>
    Results.Ok(await service.GetCatalogosAsync(tipo)))
    .RequireAuthorization();

app.MapPost("/catalogos", async (CreateCatalogoItemRequest request, ConfiguracionCatalogosService service) =>
    Results.Created("/catalogos/" + request.Tipo, await service.CreateCatalogoAsync(request)))
    .RequireAuthorization();

app.MapPut("/catalogos/{id:guid}", async (Guid id, UpdateCatalogoItemRequest request, ConfiguracionCatalogosService service) =>
{
    var result = await service.UpdateCatalogoAsync(id, request);
    return result == null ? Results.NotFound() : Results.Ok(result);
}).RequireAuthorization();

app.MapDelete("/catalogos/{id:guid}", async (Guid id, ConfiguracionCatalogosService service) =>
    await service.DeleteCatalogoAsync(id) ? Results.NoContent() : Results.NotFound())
    .RequireAuthorization();

app.MapGet("/parametros", async (ConfiguracionCatalogosService service) => Results.Ok(await service.GetParametrosAsync()))
    .RequireAuthorization();

app.MapGet("/parametros/{clave}", async (string clave, ConfiguracionCatalogosService service) =>
{
    var parametro = await service.GetParametroAsync(clave);
    return parametro == null ? Results.NotFound() : Results.Ok(parametro);
}).RequireAuthorization();

app.MapPost("/parametros", async (UpsertParametroRequest request, ConfiguracionCatalogosService service) =>
    Results.Ok(await service.UpsertParametroAsync(request)))
    .RequireAuthorization();

// Global exception handler - detailed error responses
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        
        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;
        
        string errorMessage = "Ocurrió un error interno";
        string errorDetail = "";
        string errorCode = "INTERNAL_ERROR";
        
        if (exception != null)
        {
            var (error, detail, code) = GetErrorDetails(exception, "configuración");
            errorMessage = error;
            errorDetail = detail;
            errorCode = code;
            app.Logger.LogError(exception, "Error no manejado en configuracion-service");
        }
        
        await context.Response.WriteAsJsonAsync(new
        {
            error = errorMessage,
            detail = errorDetail,
            code = errorCode,
            timestamp = DateTime.UtcNow,
            path = context.Request.Path
        });
    });
});

app.Run();

static (string Error, string Detail, string Code) GetErrorDetails(Exception ex, string context)
{
    return ex switch
    {
        ArgumentException argEx => ("Parámetro inválido", $"{argEx.Message}", "INVALID_ARGUMENT"),
        KeyNotFoundException _ => ("Recurso no encontrado", $"La {context} solicitada no existe.", "NOT_FOUND"),
        TimeoutException _ => ("Tiempo de espera agotado", "La operación tardó demasiado.", "TIMEOUT"),
        _ => ("Error interno", ex.Message.Length > 400 ? ex.Message.Substring(0, 400) + "..." : ex.Message, "INTERNAL_ERROR")
    };
}

public class AuthOptions
{
    public string Issuer { get; set; } = "nucleus-auth";
    public string Audience { get; set; } = "nucleus-api";
    public string SigningKey { get; set; } = "CHANGE_ME_SUPER_SECRET_KEY";
}

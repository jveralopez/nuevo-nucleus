using System.IO;
using System.Text;
using NucleusWFService.Domain.Requests;
using NucleusWFService.Infrastructure;
using NucleusWFService.Services;
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

builder.Services.AddDbContext<NucleusWfDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("NucleusWfDb")));
builder.Services.AddScoped<IWorkflowRepository, EfWorkflowRepository>();
builder.Services.AddScoped<WorkflowService>();

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
    .AddDbContextCheck<NucleusWfDbContext>("db", tags: new[] { "ready" });

var otelEnabledRaw = builder.Configuration["OpenTelemetry:Enabled"];
var otelEnabled = bool.TryParse(otelEnabledRaw, out var otelEnabledValue) && otelEnabledValue;
if (otelEnabled)
{
    var serviceName = builder.Configuration.GetValue<string>("OpenTelemetry:ServiceName") ?? "nucleuswf-service";
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
    var db = scope.ServiceProvider.GetRequiredService<NucleusWfDbContext>();
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
    service = "nucleuswf-service",
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

app.MapGet("/definitions", async (WorkflowService service) => Results.Ok(await service.GetDefinitionsAsync()))
    .RequireAuthorization();

app.MapGet("/definitions/{id:guid}", async (Guid id, WorkflowService service) =>
{
    var definition = await service.GetDefinitionAsync(id);
    return definition is null ? Results.NotFound() : Results.Ok(definition);
}).RequireAuthorization();

app.MapPost("/definitions", async (CreateDefinitionRequest request, WorkflowService service) =>
{
    var definition = await service.CreateDefinitionAsync(request);
    return Results.Created($"/definitions/{definition.Id}", definition);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPut("/definitions/{id:guid}", async (Guid id, UpdateDefinitionRequest request, WorkflowService service) =>
{
    var definition = await service.UpdateDefinitionAsync(id, request);
    return definition is null ? Results.NotFound() : Results.Ok(definition);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapGet("/instances", async (WorkflowService service) => Results.Ok(await service.GetInstancesAsync()))
    .RequireAuthorization();

app.MapGet("/instances/{id:guid}", async (Guid id, WorkflowService service) =>
{
    var instance = await service.GetInstanceAsync(id);
    return instance is null ? Results.NotFound() : Results.Ok(instance);
}).RequireAuthorization();

app.MapPost("/instances", async (HttpContext context, StartInstanceRequest request, WorkflowService service) =>
{
    var actor = context.User.Identity?.Name ?? context.User.FindFirst("unique_name")?.Value ?? "system";
    var actorRole = context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
    var correlationId = request.CorrelationId ?? context.Request.Headers["X-Correlation-Id"].ToString();
    var idempotencyKey = request.IdempotencyKey ?? context.Request.Headers["Idempotency-Key"].ToString();
    var instance = await service.StartInstanceAsync(
        request with { CorrelationId = correlationId, IdempotencyKey = idempotencyKey },
        actor,
        actorRole);
    return Results.Created($"/instances/{instance.Id}", instance);
}).RequireAuthorization();

app.MapPost("/instances/{id:guid}/transitions", async (HttpContext context, Guid id, TransitionRequest request, WorkflowService service) =>
{
    var actor = context.User.Identity?.Name ?? context.User.FindFirst("unique_name")?.Value ?? "system";
    var actorRole = context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
    var correlationId = request.CorrelationId ?? context.Request.Headers["X-Correlation-Id"].ToString();
    var idempotencyKey = request.IdempotencyKey ?? context.Request.Headers["Idempotency-Key"].ToString();
    var instance = await service.ApplyTransitionAsync(
        id,
        request with { CorrelationId = correlationId, IdempotencyKey = idempotencyKey },
        actor,
        actorRole);
    return instance is null ? Results.NotFound() : Results.Ok(instance);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.Run();

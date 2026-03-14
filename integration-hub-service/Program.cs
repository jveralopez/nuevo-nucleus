using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.RateLimiting;
using IntegrationHubService.Domain.Requests;
using IntegrationHubService.Infrastructure;
using IntegrationHubService.Services;
using IntegrationHubServiceService = IntegrationHubService.Services.IntegrationHubService;
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

var storagePath = Path.Combine(builder.Environment.ContentRootPath, "storage");
Directory.CreateDirectory(storagePath);
builder.Configuration["ConnectionStrings:IntegrationHubDb"] =
    $"Data Source={Path.Combine(storagePath, "integration-hub.db")}";

builder.Services.AddDbContext<IntegrationHubDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("IntegrationHubDb")));
builder.Services.AddScoped<IIntegrationRepository, EfIntegrationRepository>();
builder.Services.AddScoped<IntegrationHubServiceService>();

var dataProtectionPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(dataProtectionPath))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
        .SetApplicationName("nucleus");
}

var schedulerOptions = new SchedulerOptions();
builder.Configuration.Bind("Scheduler", schedulerOptions);
builder.Services.AddSingleton(schedulerOptions);
builder.Services.AddHostedService<IntegrationHubScheduler>();

var secretsOptions = new SecretsOptions();
builder.Configuration.Bind("Secrets", secretsOptions);
builder.Services.AddSingleton(secretsOptions);
builder.Services.AddSingleton<ISecretProvider, FileSecretProvider>();

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
    .AddDbContextCheck<IntegrationHubDbContext>("db", tags: new[] { "ready" });

var otelEnabledRaw = builder.Configuration["OpenTelemetry:Enabled"];
var otelEnabled = bool.TryParse(otelEnabledRaw, out var otelEnabledValue) && otelEnabledValue;
if (otelEnabled)
{
    var serviceName = builder.Configuration.GetValue<string>("OpenTelemetry:ServiceName") ?? "integration-hub-service";
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
                PermitLimit = 120,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
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
    var db = scope.ServiceProvider.GetRequiredService<IntegrationHubDbContext>();
    var applyMigrationsRaw = app.Configuration["Database:ApplyMigrations"];
    var applyMigrations = bool.TryParse(applyMigrationsRaw, out var applyMigrationsValue)
        ? applyMigrationsValue
        : app.Environment.IsDevelopment();
    if (applyMigrations)
    {
        db.Database.Migrate();
    }
    if (app.Environment.IsDevelopment())
    {
        EnsureTriggersSchema(db);
    }
}
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

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "integration-hub-service",
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

app.MapGet("/integraciones/templates", async (IntegrationHubServiceService service) => Results.Ok(await service.GetTemplatesAsync()))
    .RequireAuthorization();

app.MapGet("/integraciones/templates/{id:guid}", async (Guid id, IntegrationHubServiceService service) =>
{
    var template = await service.GetTemplateAsync(id);
    return template is null ? Results.NotFound() : Results.Ok(template);
}).RequireAuthorization();

app.MapPost("/integraciones/templates", async (CreateTemplateRequest request, IntegrationHubServiceService service) =>
{
    var template = await service.CreateTemplateAsync(request);
    return Results.Created($"/integraciones/templates/{template.Id}", template);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPut("/integraciones/templates/{id:guid}", async (Guid id, UpdateTemplateRequest request, IntegrationHubServiceService service) =>
{
    var template = await service.UpdateTemplateAsync(id, request);
    return template is null ? Results.NotFound() : Results.Ok(template);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPost("/integraciones/templates/{id:guid}/publish", async (Guid id, IntegrationHubServiceService service) =>
{
    var template = await service.PublishTemplateAsync(id);
    return template is null ? Results.NotFound() : Results.Ok(template);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapGet("/integraciones/conexiones", async (IntegrationHubServiceService service) => Results.Ok(await service.GetConnectionsAsync()))
    .RequireAuthorization();

app.MapGet("/integraciones/conexiones/{id:guid}", async (Guid id, IntegrationHubServiceService service) =>
{
    var connection = await service.GetConnectionAsync(id);
    return connection is null ? Results.NotFound() : Results.Ok(connection);
}).RequireAuthorization();

app.MapPost("/integraciones/conexiones", async (CreateConnectionRequest request, IntegrationHubServiceService service) =>
{
    var connection = await service.CreateConnectionAsync(request);
    return Results.Created($"/integraciones/conexiones/{connection.Id}", connection);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapGet("/integraciones/jobs", async (Guid? templateId, string? estado, IntegrationHubServiceService service) =>
{
    var jobs = await service.GetJobsAsync();
    var filtered = jobs.AsQueryable();
    if (templateId.HasValue) filtered = filtered.Where(j => j.TemplateId == templateId.Value);
    if (!string.IsNullOrWhiteSpace(estado)) filtered = filtered.Where(j => string.Equals(j.Estado, estado, StringComparison.OrdinalIgnoreCase));
    return Results.Ok(filtered.ToList());
}).RequireAuthorization();

app.MapGet("/integraciones/jobs/{id:guid}", async (Guid id, IntegrationHubServiceService service) =>
{
    var job = await service.GetJobAsync(id);
    return job is null ? Results.NotFound() : Results.Ok(job);
}).RequireAuthorization();

app.MapGet("/integraciones/eventos", async (Guid? jobId, IIntegrationRepository repository) =>
{
    var events = await repository.GetEventsAsync(jobId);
    return Results.Ok(events);
}).RequireAuthorization();

app.MapPost("/integraciones/jobs", async (StartJobRequest request, IntegrationHubServiceService service) =>
{
    var job = await service.StartJobAsync(request);
    return Results.Created($"/integraciones/jobs/{job.Id}", job);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPost("/integraciones/jobs/{id:guid}/retry", async (Guid id, RetryJobRequest request, IntegrationHubServiceService service) =>
{
    var job = await service.RetryJobAsync(id, request);
    return job is null ? Results.NotFound() : Results.Ok(job);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapGet("/integraciones/triggers", async (string? eventName, IntegrationHubServiceService service) =>
    Results.Ok(await service.GetTriggersAsync(eventName)))
    .RequireAuthorization();

app.MapPost("/integraciones/triggers", async (CreateTriggerRequest request, IntegrationHubServiceService service) =>
{
    var trigger = await service.CreateTriggerAsync(request);
    return Results.Created($"/integraciones/triggers/{trigger.Id}", trigger);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPut("/integraciones/triggers/{id:guid}", async (Guid id, CreateTriggerRequest request, IntegrationHubServiceService service) =>
{
    var trigger = await service.UpdateTriggerAsync(id, request);
    return trigger is null ? Results.NotFound() : Results.Ok(trigger);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPost("/integraciones/triggers/{id:guid}/execute", async (Guid id, ExecuteTriggerRequest request, IntegrationHubServiceService service) =>
{
    var job = await service.ExecuteTriggerAsync(id, request);
    return job is null ? Results.NotFound() : Results.Ok(job);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

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
            var (error, detail, code) = GetErrorDetails(exception, "integración");
            errorMessage = error;
            errorDetail = detail;
            errorCode = code;
            app.Logger.LogError(exception, "Error no manejado en integration-hub-service");
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
        TimeoutException _ => ("Tiempo de espera agotado", $"La {context} tardó demasiado. Intente más tarde.", "TIMEOUT"),
        HttpRequestException httpEx => HandleHttpException(httpEx, context),
        _ => ("Error interno", ex.Message.Length > 400 ? ex.Message.Substring(0, 400) + "..." : ex.Message, "INTERNAL_ERROR")
    };
}

static (string Error, string Detail, string Code) HandleHttpException(HttpRequestException ex, string context)
{
    return ex.StatusCode switch
    {
        System.Net.HttpStatusCode.Unauthorized => ("No autorizado", "Credenciales de integración inválidas.", "UNAUTHORIZED"),
        System.Net.HttpStatusCode.NotFound => ("Recurso no encontrado", $"El servicio externo de {context} no existe.", "NOT_FOUND"),
        System.Net.HttpStatusCode.ServiceUnavailable => ("Servicio no disponible", $"El servicio de {context} no está disponible.", "SERVICE_UNAVAILABLE"),
        _ => ("Error de comunicación", $"Error al comunicarse con el servicio de {context}.", "COMMUNICATION_ERROR")
    };
}

static void EnsureTriggersSchema(IntegrationHubDbContext db)
{
    var connection = db.Database.GetDbConnection();
    if (connection.State != ConnectionState.Open)
    {
        connection.Open();
    }

    using var command = connection.CreateCommand();
    command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Triggers';";
    var tableName = command.ExecuteScalar() as string;
    if (string.IsNullOrWhiteSpace(tableName))
    {
        command.CommandText = """
CREATE TABLE IF NOT EXISTS Triggers (
    Id TEXT NOT NULL PRIMARY KEY,
    EventName TEXT NOT NULL,
    TemplateId TEXT NOT NULL,
    Enabled INTEGER NOT NULL,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL
);
CREATE INDEX IF NOT EXISTS IX_Triggers_EventName ON Triggers (EventName);
""";
        command.ExecuteNonQuery();
        return;
    }

    command.CommandText = "PRAGMA table_info(Triggers);";
    using var reader = command.ExecuteReader();
    var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    while (reader.Read())
    {
        var name = reader.GetString(1);
        if (!string.IsNullOrWhiteSpace(name)) columns.Add(name);
    }

    EnsureColumn(connection, columns, "EventName", "TEXT NOT NULL DEFAULT ''");
    EnsureColumn(connection, columns, "TemplateId", "TEXT NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'");
    EnsureColumn(connection, columns, "Enabled", "INTEGER NOT NULL DEFAULT 1");
    EnsureColumn(connection, columns, "CreatedAt", "TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP");
    EnsureColumn(connection, columns, "UpdatedAt", "TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP");

    using var indexCmd = connection.CreateCommand();
    indexCmd.CommandText = "CREATE INDEX IF NOT EXISTS IX_Triggers_EventName ON Triggers (EventName);";
    indexCmd.ExecuteNonQuery();
}

static void EnsureColumn(DbConnection connection, HashSet<string> columns, string name, string sqlType)
{
    if (columns.Contains(name)) return;
    using var command = connection.CreateCommand();
    command.CommandText = $"ALTER TABLE Triggers ADD COLUMN {name} {sqlType};";
    command.ExecuteNonQuery();
}

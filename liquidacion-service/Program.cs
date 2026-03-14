using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Text;
using LiquidacionService.Domain.Requests;
using LiquidacionService.Infrastructure;
using LiquidacionService.Services;
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
builder.Services.AddDbContext<PayrollDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("LiquidacionDb")));
builder.Services.AddScoped<IPayrollRepository, EfPayrollRepository>();
builder.Services.AddSingleton<ConceptRuleEngine>();
builder.Services.AddSingleton<GananciasCalculator>();
builder.Services.AddSingleton<ReceiptCalculator>();
builder.Services.AddSingleton<ReceiptExporter>();
builder.Services.AddSingleton<ConceptCatalogService>();

var dataProtectionPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(dataProtectionPath))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
        .SetApplicationName("nucleus");
}
var workflowOptions = new WorkflowOptions();
builder.Configuration.Bind("Workflow", workflowOptions);
builder.Services.AddSingleton(workflowOptions);
builder.Services.AddHttpClient<WorkflowVacacionesClient>();
builder.Services.AddScoped<PayrollService>();

var integrationOptions = new IntegrationHubOptions();
builder.Configuration.Bind("IntegrationHub", integrationOptions);
builder.Services.AddSingleton(integrationOptions);
builder.Services.AddHttpClient<IntegrationHubClient>();

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
    .AddDbContextCheck<PayrollDbContext>("db", tags: new[] { "ready" });

var otelEnabledRaw = builder.Configuration["OpenTelemetry:Enabled"];
var otelEnabled = bool.TryParse(otelEnabledRaw, out var otelEnabledValue) && otelEnabledValue;
if (otelEnabled)
{
    var serviceName = builder.Configuration.GetValue<string>("OpenTelemetry:ServiceName") ?? "liquidacion-service";
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
    var db = scope.ServiceProvider.GetRequiredService<PayrollDbContext>();
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
        EnsurePayrollSchema(db);
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
    service = "liquidacion-service",
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

app.MapGet("/payrolls", async (PayrollService service) => Results.Ok(await service.GetAllAsync()))
    .RequireAuthorization();

app.MapGet("/payrolls/{id:guid}", async (Guid id, PayrollService service) =>
{
    var payroll = await service.GetAsync(id);
    return payroll is null ? Results.NotFound() : Results.Ok(payroll);
}).RequireAuthorization();

app.MapPost("/payrolls", async (NewPayrollRequest request, PayrollService service) =>
{
    var payroll = await service.CreateAsync(request);
    return Results.Created($"/payrolls/{payroll.Id}", payroll);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPost("/payrolls/{id:guid}/legajos", async (Guid id, UpsertLegajoRequest request, PayrollService service) =>
{
    var payroll = await service.AddLegajoAsync(id, request);
    return Results.Ok(payroll);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapDelete("/payrolls/{id:guid}/legajos/{legajoId:guid}", async (Guid id, Guid legajoId, PayrollService service) =>
{
    var ok = await service.RemoveLegajoAsync(id, legajoId);
    return ok ? Results.NoContent() : Results.NotFound();
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPost("/payrolls/{id:guid}/procesar", async (Guid id, ProcessPayrollRequest request, PayrollService service) =>
{
    var payroll = await service.ProcessAsync(id, request ?? new ProcessPayrollRequest());
    return payroll is null ? Results.NotFound() : Results.Ok(payroll);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPatch("/payrolls/{id:guid}", async (Guid id, NewPayrollRequest request, PayrollService service) =>
{
    var payroll = await service.GetAsync(id);
    if (payroll is null) return Results.NotFound();
    payroll.Periodo = request.Periodo;
    payroll.Tipo = request.Tipo;
    payroll.Descripcion = request.Descripcion;
    await service.UpdateAsync(payroll);
    return Results.Ok(payroll);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapGet("/payrolls/{id:guid}/recibos", async (Guid id, PayrollService service) =>
{
    var recibos = await service.GetRecibosAsync(id);
    return recibos is null ? Results.NotFound() : Results.Ok(recibos);
}).RequireAuthorization();

app.MapGet("/payrolls/{id:guid}/exports", async (Guid id, PayrollService service, ReceiptExporter exporter) =>
{
    var payroll = await service.GetAsync(id);
    if (payroll is null) return Results.NotFound();

    var exports = exporter.ListExports(payroll)
        .Select(file => new { fileName = file, url = $"/exports/{file}" });
    return Results.Ok(exports);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapGet("/payrolls/{id:guid}/exports/empleado", async (Guid id, PayrollService service, ReceiptExporter exporter) =>
{
    var payroll = await service.GetAsync(id);
    if (payroll is null) return Results.NotFound();

    var exports = exporter.ListExports(payroll)
        .Select(file => new { fileName = file, url = $"/exports/{file}" });
    return Results.Ok(exports);
}).RequireAuthorization();

app.MapGet("/catalogos/conceptos", (string? tipo, ConceptCatalogService catalog) =>
{
    var resolved = string.IsNullOrWhiteSpace(tipo) ? "core" : tipo;
    var items = catalog.GetCatalog(resolved);
    return Results.Ok(new { tipo = resolved, total = items.Count, items });
}).RequireAuthorization();

app.MapGet("/catalogos/reglas", (string? convenio, ConceptRuleEngine engine) =>
{
    var rules = engine.GetRules(convenio);
    return Results.Ok(new { convenio = convenio ?? string.Empty, total = rules.Count, rules });
}).RequireAuthorization();

app.MapGet("/exports/{fileName}", (string fileName, ReceiptExporter exporter) =>
{
    var stream = exporter.OpenExport(fileName);
    if (stream is null) return Results.NotFound();
    var contentType = fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) ? "text/csv" : "application/json";
    return Results.File(stream, contentType, fileName);
}).RequireAuthorization();

app.Run();

static void EnsurePayrollSchema(PayrollDbContext db)
{
    var connection = db.Database.GetDbConnection();
    if (connection.State != ConnectionState.Open)
    {
        connection.Open();
    }

    EnsureTable(connection, "Embargo", """
CREATE TABLE IF NOT EXISTS "Embargo" (
    "Id" TEXT NOT NULL PRIMARY KEY,
    "Tipo" TEXT NOT NULL,
    "Porcentaje" TEXT NULL,
    "MontoFijo" TEXT NULL,
    "MontoTotal" TEXT NULL,
    "MontoPendiente" TEXT NULL,
    "BaseCalculo" TEXT NOT NULL,
    "Activo" INTEGER NOT NULL,
    "LegajoEnLoteId" TEXT NOT NULL
);
""");

    EnsureTable(connection, "LicenciaEnLote", """
CREATE TABLE IF NOT EXISTS "LicenciaEnLote" (
    "Id" TEXT NOT NULL PRIMARY KEY,
    "Tipo" TEXT NOT NULL,
    "Dias" INTEGER NOT NULL,
    "ConGoce" INTEGER NOT NULL,
    "LegajoEnLoteId" TEXT NOT NULL
);
""");

    EnsureTable(connection, "ReceiptDetail", """
CREATE TABLE IF NOT EXISTS "ReceiptDetail" (
    "Id" TEXT NOT NULL PRIMARY KEY,
    "Concepto" TEXT NOT NULL,
    "Importe" TEXT NOT NULL,
    "PayrollReceiptId" TEXT NOT NULL
);
""");

    EnsureColumns(connection, "LegajoEnLote", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Convenio"] = "TEXT NULL",
        ["Categoria"] = "TEXT NULL",
        ["Presentismo"] = "TEXT NOT NULL DEFAULT 0",
        ["HorasExtra"] = "TEXT NOT NULL DEFAULT 0",
        ["Premios"] = "TEXT NOT NULL DEFAULT 0",
        ["NoRemunerativo"] = "TEXT NOT NULL DEFAULT 0",
        ["BonosNoRemunerativos"] = "TEXT NOT NULL DEFAULT 0",
        ["AplicaGanancias"] = "INTEGER NOT NULL DEFAULT 1",
        ["OmitirGanancias"] = "INTEGER NOT NULL DEFAULT 0",
        ["ConyugeACargo"] = "INTEGER NOT NULL DEFAULT 0",
        ["CantHijos"] = "INTEGER NOT NULL DEFAULT 0",
        ["CantOtrosFamiliares"] = "INTEGER NOT NULL DEFAULT 0",
        ["DeduccionesAdicionales"] = "TEXT NOT NULL DEFAULT 0",
        ["VacacionesDias"] = "INTEGER NOT NULL DEFAULT 0"
    });

    EnsureColumns(connection, "Embargo", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Tipo"] = "TEXT NOT NULL DEFAULT ''",
        ["Porcentaje"] = "TEXT NULL",
        ["MontoFijo"] = "TEXT NULL",
        ["MontoTotal"] = "TEXT NULL",
        ["MontoPendiente"] = "TEXT NULL",
        ["BaseCalculo"] = "TEXT NOT NULL DEFAULT 'Neto'",
        ["Activo"] = "INTEGER NOT NULL DEFAULT 1",
        ["LegajoEnLoteId"] = "TEXT NOT NULL DEFAULT ''"
    });

    EnsureColumns(connection, "LicenciaEnLote", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Tipo"] = "TEXT NOT NULL DEFAULT ''",
        ["Dias"] = "INTEGER NOT NULL DEFAULT 0",
        ["ConGoce"] = "INTEGER NOT NULL DEFAULT 1",
        ["LegajoEnLoteId"] = "TEXT NOT NULL DEFAULT ''"
    });

    EnsureColumns(connection, "ReceiptDetail", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Concepto"] = "TEXT NOT NULL DEFAULT ''",
        ["Importe"] = "TEXT NOT NULL DEFAULT 0",
        ["PayrollReceiptId"] = "TEXT NOT NULL DEFAULT ''"
    });
}

static void EnsureTable(DbConnection connection, string tableName, string createSql)
{
    using var command = connection.CreateCommand();
    command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=$name;";
    var nameParam = command.CreateParameter();
    nameParam.ParameterName = "$name";
    nameParam.Value = tableName;
    command.Parameters.Add(nameParam);
    var exists = command.ExecuteScalar() as string;
    if (!string.IsNullOrWhiteSpace(exists)) return;

    command.Parameters.Clear();
    command.CommandText = createSql;
    command.ExecuteNonQuery();
}

static void EnsureColumns(DbConnection connection, string tableName, Dictionary<string, string> columnsToEnsure)
{
    var existing = GetColumns(connection, tableName);
    foreach (var entry in columnsToEnsure)
    {
        if (existing.Contains(entry.Key)) continue;
        using var command = connection.CreateCommand();
        command.CommandText = $"ALTER TABLE \"{tableName}\" ADD COLUMN \"{entry.Key}\" {entry.Value};";
        command.ExecuteNonQuery();
    }
}

static HashSet<string> GetColumns(DbConnection connection, string tableName)
{
    using var command = connection.CreateCommand();
    command.CommandText = $"PRAGMA table_info(\"{tableName}\");";
    using var reader = command.ExecuteReader();
    var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    while (reader.Read())
    {
        var name = reader.GetString(1);
        if (!string.IsNullOrWhiteSpace(name)) columns.Add(name);
    }

    return columns;
}

public partial class Program { }

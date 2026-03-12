using System.Data;
using System.Data.Common;
using System.Text;
using PersonalService.Domain.Requests;
using PersonalService.Infrastructure;
using PersonalService.Services;
using PersonalServiceService = PersonalService.Services.PersonalService;
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

builder.Services.AddDbContext<PersonalDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("PersonalDb")));
builder.Services.AddScoped<IPersonalRepository, EfPersonalRepository>();
builder.Services.AddScoped<PersonalServiceService>();

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
    .AddDbContextCheck<PersonalDbContext>("db", tags: new[] { "ready" });

var otelEnabled = builder.Configuration.GetValue("OpenTelemetry:Enabled", false);
if (otelEnabled)
{
    var serviceName = builder.Configuration.GetValue<string>("OpenTelemetry:ServiceName") ?? "personal-service";
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
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PersonalDbContext>();
    var applyMigrations = app.Configuration.GetValue("Database:ApplyMigrations", app.Environment.IsDevelopment());
    if (applyMigrations)
    {
        db.Database.Migrate();
    }
    if (app.Environment.IsDevelopment())
    {
        EnsurePersonalSchema(db);
    }
}

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "personal-service",
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

app.MapGet("/legajos", async (string? estado, string? numero, string? documento, string? cuil, PersonalServiceService service) =>
{
    var legajos = await service.GetLegajosAsync();
    var filtered = legajos.AsQueryable();
    if (!string.IsNullOrWhiteSpace(estado))
    {
        filtered = filtered.Where(l => string.Equals(l.Estado, estado, StringComparison.OrdinalIgnoreCase));
    }
    if (!string.IsNullOrWhiteSpace(numero))
    {
        filtered = filtered.Where(l => string.Equals(l.Numero, numero, StringComparison.OrdinalIgnoreCase));
    }
    if (!string.IsNullOrWhiteSpace(documento))
    {
        filtered = filtered.Where(l => string.Equals(l.Documento, documento, StringComparison.OrdinalIgnoreCase));
    }
    if (!string.IsNullOrWhiteSpace(cuil))
    {
        filtered = filtered.Where(l => string.Equals(l.Cuil, cuil, StringComparison.OrdinalIgnoreCase));
    }
    return Results.Ok(filtered.ToList());
}).RequireAuthorization();

app.MapGet("/legajos/numero/{numero}", async (string numero, PersonalServiceService service) =>
{
    var legajos = await service.GetLegajosAsync();
    var legajo = legajos.FirstOrDefault(l => string.Equals(l.Numero, numero, StringComparison.OrdinalIgnoreCase));
    return legajo is null ? Results.NotFound() : Results.Ok(legajo);
}).RequireAuthorization();

app.MapGet("/legajos/{id:guid}", async (Guid id, PersonalServiceService service) =>
{
    var legajo = await service.GetLegajoAsync(id);
    return legajo is null ? Results.NotFound() : Results.Ok(legajo);
}).RequireAuthorization();

app.MapGet("/legajos/{id:guid}/familiares", async (Guid id, PersonalServiceService service) =>
{
    var legajo = await service.GetLegajoAsync(id);
    return legajo is null ? Results.NotFound() : Results.Ok(legajo.Familiares);
}).RequireAuthorization();

app.MapGet("/legajos/{id:guid}/licencias", async (Guid id, PersonalServiceService service) =>
{
    var legajo = await service.GetLegajoAsync(id);
    return legajo is null ? Results.NotFound() : Results.Ok(legajo.Licencias);
}).RequireAuthorization();

app.MapGet("/legajos/{id:guid}/domicilios", async (Guid id, PersonalServiceService service) =>
{
    var legajo = await service.GetLegajoAsync(id);
    return legajo is null ? Results.NotFound() : Results.Ok(legajo.Domicilios);
}).RequireAuthorization();

app.MapGet("/legajos/{id:guid}/documentos", async (Guid id, PersonalServiceService service) =>
{
    var legajo = await service.GetLegajoAsync(id);
    return legajo is null ? Results.NotFound() : Results.Ok(legajo.Documentos);
}).RequireAuthorization();

app.MapGet("/solicitudes", async (Guid? legajoId, string? estado, PersonalServiceService service) =>
    Results.Ok(await service.GetSolicitudesAsync(legajoId, estado)))
    .RequireAuthorization();

app.MapGet("/solicitudes/{id:guid}", async (Guid id, PersonalServiceService service) =>
{
    var solicitud = await service.GetSolicitudAsync(id);
    return solicitud is null ? Results.NotFound() : Results.Ok(solicitud);
}).RequireAuthorization();

app.MapPost("/legajos", async (CreateLegajoRequest request, PersonalServiceService service) =>
{
    var legajo = await service.CreateLegajoAsync(request);
    return Results.Created($"/legajos/{legajo.Id}", legajo);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPost("/solicitudes", async (CreateSolicitudCambioRequest request, PersonalServiceService service) =>
{
    var solicitud = await service.CreateSolicitudAsync(request);
    return Results.Created($"/solicitudes/{solicitud.Id}", solicitud);
}).RequireAuthorization();

app.MapPut("/legajos/{id:guid}", async (Guid id, UpdateLegajoRequest request, PersonalServiceService service) =>
{
    var legajo = await service.UpdateLegajoAsync(id, request);
    return legajo is null ? Results.NotFound() : Results.Ok(legajo);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPut("/legajos/{id:guid}/familiares", async (Guid id, UpdateFamiliaresRequest request, PersonalServiceService service) =>
{
    var familiares = await service.UpdateFamiliaresAsync(id, request);
    return familiares is null ? Results.NotFound() : Results.Ok(familiares);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPut("/legajos/{id:guid}/licencias", async (Guid id, UpdateLicenciasRequest request, PersonalServiceService service) =>
{
    var licencias = await service.UpdateLicenciasAsync(id, request);
    return licencias is null ? Results.NotFound() : Results.Ok(licencias);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPut("/legajos/{id:guid}/domicilios", async (Guid id, UpdateDomiciliosRequest request, PersonalServiceService service) =>
{
    var domicilios = await service.UpdateDomiciliosAsync(id, request);
    return domicilios is null ? Results.NotFound() : Results.Ok(domicilios);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPut("/legajos/{id:guid}/documentos", async (Guid id, UpdateDocumentosRequest request, PersonalServiceService service) =>
{
    var documentos = await service.UpdateDocumentosAsync(id, request);
    return documentos is null ? Results.NotFound() : Results.Ok(documentos);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPost("/solicitudes/{id:guid}/aprobar", async (Guid id, UpdateSolicitudEstadoRequest request, PersonalServiceService service) =>
{
    var solicitud = await service.UpdateSolicitudEstadoAsync(id, "Aprobada", request);
    return solicitud is null ? Results.NotFound() : Results.Ok(solicitud);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPost("/solicitudes/{id:guid}/rechazar", async (Guid id, UpdateSolicitudEstadoRequest request, PersonalServiceService service) =>
{
    var solicitud = await service.UpdateSolicitudEstadoAsync(id, "Rechazada", request);
    return solicitud is null ? Results.NotFound() : Results.Ok(solicitud);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapDelete("/legajos/{id:guid}", async (Guid id, PersonalServiceService service) =>
{
    var legajo = await service.DeactivateLegajoAsync(id);
    return legajo is null ? Results.NotFound() : Results.Ok(legajo);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.Run();

static void EnsurePersonalSchema(PersonalDbContext db)
{
    var connection = db.Database.GetDbConnection();
    if (connection.State != ConnectionState.Open)
    {
        connection.Open();
    }

    EnsureTable(connection, "Familiares", """
CREATE TABLE IF NOT EXISTS "Familiares" (
    "Id" TEXT NOT NULL PRIMARY KEY,
    "Nombre" TEXT NOT NULL,
    "Apellido" TEXT NOT NULL,
    "Documento" TEXT NOT NULL,
    "Tipo" TEXT NOT NULL,
    "FechaNacimiento" TEXT NULL,
    "Vive" INTEGER NOT NULL,
    "Discapacidad" INTEGER NOT NULL,
    "ACargo" INTEGER NOT NULL,
    "ACargoObraSocial" INTEGER NOT NULL,
    "AplicaGanancias" INTEGER NOT NULL,
    "LegajoId" TEXT NOT NULL
);
""");

    EnsureTable(connection, "Licencias", """
CREATE TABLE IF NOT EXISTS "Licencias" (
    "Id" TEXT NOT NULL PRIMARY KEY,
    "Tipo" TEXT NOT NULL,
    "CodigoSIJP" TEXT NULL,
    "FechaInicio" TEXT NOT NULL,
    "FechaFin" TEXT NOT NULL,
    "ConGoce" INTEGER NOT NULL,
    "CuentaVacaciones" INTEGER NOT NULL,
    "LegajoId" TEXT NOT NULL
);
""");

    EnsureTable(connection, "Domicilios", """
CREATE TABLE IF NOT EXISTS "Domicilios" (
    "Id" TEXT NOT NULL PRIMARY KEY,
    "Tipo" TEXT NOT NULL,
    "Calle" TEXT NOT NULL,
    "Numero" TEXT NOT NULL,
    "Piso" TEXT NULL,
    "Depto" TEXT NULL,
    "Localidad" TEXT NOT NULL,
    "Provincia" TEXT NOT NULL,
    "Pais" TEXT NOT NULL,
    "CodigoPostal" TEXT NULL,
    "Observaciones" TEXT NULL,
    "LegajoId" TEXT NOT NULL
);
""");

    EnsureTable(connection, "Documentos", """
CREATE TABLE IF NOT EXISTS "Documentos" (
    "Id" TEXT NOT NULL PRIMARY KEY,
    "Tipo" TEXT NOT NULL,
    "Numero" TEXT NOT NULL,
    "FechaEmision" TEXT NULL,
    "FechaVencimiento" TEXT NULL,
    "Observaciones" TEXT NULL,
    "LegajoId" TEXT NOT NULL
);
""");

    EnsureTable(connection, "SolicitudesCambio", """
CREATE TABLE IF NOT EXISTS "SolicitudesCambio" (
    "Id" TEXT NOT NULL PRIMARY KEY,
    "LegajoId" TEXT NOT NULL,
    "Tipo" TEXT NOT NULL,
    "Detalle" TEXT NULL,
    "Estado" TEXT NOT NULL,
    "DatosJson" TEXT NULL,
    "Observaciones" TEXT NULL,
    "CreatedAt" TEXT NOT NULL,
    "UpdatedAt" TEXT NOT NULL
);
""");

    EnsureColumns(connection, "Legajos", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["EstadoCivil"] = "TEXT NULL",
        ["FechaIngreso"] = "TEXT NULL",
        ["Convenio"] = "TEXT NULL",
        ["Categoria"] = "TEXT NULL",
        ["ObraSocial"] = "TEXT NULL",
        ["Sindicato"] = "TEXT NULL",
        ["TipoPersonal"] = "TEXT NULL",
        ["Ubicacion"] = "TEXT NULL"
    });

    EnsureColumns(connection, "Familiares", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Nombre"] = "TEXT NOT NULL DEFAULT ''",
        ["Apellido"] = "TEXT NOT NULL DEFAULT ''",
        ["Documento"] = "TEXT NOT NULL DEFAULT ''",
        ["Tipo"] = "TEXT NOT NULL DEFAULT ''",
        ["FechaNacimiento"] = "TEXT NULL",
        ["Vive"] = "INTEGER NOT NULL DEFAULT 1",
        ["Discapacidad"] = "INTEGER NOT NULL DEFAULT 0",
        ["ACargo"] = "INTEGER NOT NULL DEFAULT 0",
        ["ACargoObraSocial"] = "INTEGER NOT NULL DEFAULT 0",
        ["AplicaGanancias"] = "INTEGER NOT NULL DEFAULT 0",
        ["LegajoId"] = "TEXT NOT NULL DEFAULT ''"
    });

    EnsureColumns(connection, "Licencias", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Tipo"] = "TEXT NOT NULL DEFAULT ''",
        ["CodigoSIJP"] = "TEXT NULL",
        ["FechaInicio"] = "TEXT NOT NULL DEFAULT '0001-01-01T00:00:00'",
        ["FechaFin"] = "TEXT NOT NULL DEFAULT '0001-01-01T00:00:00'",
        ["ConGoce"] = "INTEGER NOT NULL DEFAULT 1",
        ["CuentaVacaciones"] = "INTEGER NOT NULL DEFAULT 0",
        ["LegajoId"] = "TEXT NOT NULL DEFAULT ''"
    });

    EnsureColumns(connection, "Domicilios", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Tipo"] = "TEXT NOT NULL DEFAULT ''",
        ["Calle"] = "TEXT NOT NULL DEFAULT ''",
        ["Numero"] = "TEXT NOT NULL DEFAULT ''",
        ["Piso"] = "TEXT NULL",
        ["Depto"] = "TEXT NULL",
        ["Localidad"] = "TEXT NOT NULL DEFAULT ''",
        ["Provincia"] = "TEXT NOT NULL DEFAULT ''",
        ["Pais"] = "TEXT NOT NULL DEFAULT ''",
        ["CodigoPostal"] = "TEXT NULL",
        ["Observaciones"] = "TEXT NULL",
        ["LegajoId"] = "TEXT NOT NULL DEFAULT ''"
    });

    EnsureColumns(connection, "Documentos", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Tipo"] = "TEXT NOT NULL DEFAULT ''",
        ["Numero"] = "TEXT NOT NULL DEFAULT ''",
        ["FechaEmision"] = "TEXT NULL",
        ["FechaVencimiento"] = "TEXT NULL",
        ["Observaciones"] = "TEXT NULL",
        ["LegajoId"] = "TEXT NOT NULL DEFAULT ''"
    });

    EnsureColumns(connection, "SolicitudesCambio", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["LegajoId"] = "TEXT NOT NULL DEFAULT ''",
        ["Tipo"] = "TEXT NOT NULL DEFAULT ''",
        ["Detalle"] = "TEXT NULL",
        ["Estado"] = "TEXT NOT NULL DEFAULT 'PendAprob'",
        ["DatosJson"] = "TEXT NULL",
        ["Observaciones"] = "TEXT NULL",
        ["CreatedAt"] = "TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP",
        ["UpdatedAt"] = "TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP"
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

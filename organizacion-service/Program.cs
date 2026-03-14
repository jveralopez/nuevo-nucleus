using System.Data;
using System.Data.Common;
using System.Text;
using OrganizacionService.Domain.Requests;
using OrganizacionService.Infrastructure;
using OrganizacionService.Services;
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

builder.Services.AddDbContext<OrganizationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("OrganizacionDb")));
builder.Services.AddScoped<IOrganizationRepository, EfOrganizationRepository>();
builder.Services.AddScoped<OrganizationService>();

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
    .AddDbContextCheck<OrganizationDbContext>("db", tags: new[] { "ready" });

var otelEnabledRaw = builder.Configuration["OpenTelemetry:Enabled"];
var otelEnabled = bool.TryParse(otelEnabledRaw, out var otelEnabledValue) && otelEnabledValue;
if (otelEnabled)
{
    var serviceName = builder.Configuration.GetValue<string>("OpenTelemetry:ServiceName") ?? "organizacion-service";
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
    var db = scope.ServiceProvider.GetRequiredService<OrganizationDbContext>();
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
        EnsureOrganizationSchema(db);
    }
}

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "organizacion-service",
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

app.MapGet("/empresas", async (OrganizationService service) => Results.Ok(await service.GetEmpresasAsync()))
    .RequireAuthorization();

app.MapGet("/empresas/{id:guid}", async (Guid id, OrganizationService service) =>
{
    var empresa = await service.GetEmpresaAsync(id);
    return empresa is null ? Results.NotFound() : Results.Ok(empresa);
}).RequireAuthorization();

app.MapPost("/empresas", async (CreateEmpresaRequest request, OrganizationService service) =>
{
    var empresa = await service.CreateEmpresaAsync(request);
    return Results.Created($"/empresas/{empresa.Id}", empresa);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPut("/empresas/{id:guid}", async (Guid id, UpdateEmpresaRequest request, OrganizationService service) =>
{
    var empresa = await service.UpdateEmpresaAsync(id, request);
    return empresa is null ? Results.NotFound() : Results.Ok(empresa);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapDelete("/empresas/{id:guid}", async (Guid id, OrganizationService service) =>
{
    var empresa = await service.DeactivateEmpresaAsync(id);
    return empresa is null ? Results.NotFound() : Results.Ok(empresa);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapGet("/unidades", async (Guid? empresaId, Guid? padreId, string? tipo, OrganizationService service) =>
{
    var unidades = await service.GetUnidadesAsync();
    var filtered = unidades.AsQueryable();
    if (empresaId.HasValue) filtered = filtered.Where(u => u.EmpresaId == empresaId.Value);
    if (padreId.HasValue) filtered = filtered.Where(u => u.PadreId == padreId.Value);
    if (!string.IsNullOrWhiteSpace(tipo)) filtered = filtered.Where(u => string.Equals(u.Tipo, tipo, StringComparison.OrdinalIgnoreCase));
    return Results.Ok(filtered.ToList());
}).RequireAuthorization();

app.MapGet("/unidades/tree", async (Guid? empresaId, OrganizationService service) =>
    Results.Ok(await service.GetUnidadesTreeAsync(empresaId)))
    .RequireAuthorization();

app.MapGet("/organigramas", async (Guid? empresaId, OrganizationService service) =>
    Results.Ok(await service.GetOrganigramasAsync(empresaId)))
    .RequireAuthorization();

app.MapGet("/organigramas/{id:guid}", async (Guid id, OrganizationService service) =>
{
    var organigrama = await service.GetOrganigramaAsync(id);
    return organigrama is null ? Results.NotFound() : Results.Ok(organigrama);
}).RequireAuthorization();

app.MapPost("/organigramas", async (CreateOrganigramaVersionRequest request, OrganizationService service) =>
{
    var organigrama = await service.CreateOrganigramaAsync(request);
    return Results.Created($"/organigramas/{organigrama.Id}", organigrama);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapGet("/unidades/{id:guid}", async (Guid id, OrganizationService service) =>
{
    var unidad = await service.GetUnidadAsync(id);
    return unidad is null ? Results.NotFound() : Results.Ok(unidad);
}).RequireAuthorization();

app.MapPost("/unidades", async (CreateUnidadRequest request, OrganizationService service) =>
{
    var unidad = await service.CreateUnidadAsync(request);
    return Results.Created($"/unidades/{unidad.Id}", unidad);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPut("/unidades/{id:guid}", async (Guid id, UpdateUnidadRequest request, OrganizationService service) =>
{
    var unidad = await service.UpdateUnidadAsync(id, request);
    return unidad is null ? Results.NotFound() : Results.Ok(unidad);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapDelete("/unidades/{id:guid}", async (Guid id, OrganizationService service) =>
{
    var unidad = await service.DeactivateUnidadAsync(id);
    return unidad is null ? Results.NotFound() : Results.Ok(unidad);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapGet("/posiciones", async (Guid? unidadId, string? estado, OrganizationService service) =>
{
    var posiciones = await service.GetPosicionesAsync();
    var filtered = posiciones.AsQueryable();
    if (unidadId.HasValue) filtered = filtered.Where(p => p.UnidadId == unidadId.Value);
    if (!string.IsNullOrWhiteSpace(estado)) filtered = filtered.Where(p => string.Equals(p.Estado, estado, StringComparison.OrdinalIgnoreCase));
    return Results.Ok(filtered.ToList());
}).RequireAuthorization();

app.MapGet("/posiciones/{id:guid}", async (Guid id, OrganizationService service) =>
{
    var posicion = await service.GetPosicionAsync(id);
    return posicion is null ? Results.NotFound() : Results.Ok(posicion);
}).RequireAuthorization();

app.MapPost("/posiciones", async (CreatePosicionRequest request, OrganizationService service) =>
{
    var posicion = await service.CreatePosicionAsync(request);
    return Results.Created($"/posiciones/{posicion.Id}", posicion);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPut("/posiciones/{id:guid}", async (Guid id, UpdatePosicionRequest request, OrganizationService service) =>
{
    var posicion = await service.UpdatePosicionAsync(id, request);
    return posicion is null ? Results.NotFound() : Results.Ok(posicion);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPost("/posiciones/{id:guid}/asignar", async (Guid id, AssignLegajoRequest request, OrganizationService service) =>
{
    var posicion = await service.AssignLegajoAsync(id, request);
    return posicion is null ? Results.NotFound() : Results.Ok(posicion);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPost("/posiciones/{id:guid}/desasignar", async (Guid id, AssignLegajoRequest request, OrganizationService service) =>
{
    var posicion = await service.UnassignLegajoAsync(id, request);
    return posicion is null ? Results.NotFound() : Results.Ok(posicion);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapDelete("/posiciones/{id:guid}", async (Guid id, OrganizationService service) =>
{
    var posicion = await service.DeactivatePosicionAsync(id);
    return posicion is null ? Results.NotFound() : Results.Ok(posicion);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapGet("/centros-costo", async (Guid? empresaId, string? estado, OrganizationService service) =>
{
    var centros = await service.GetCentrosCostoAsync();
    var filtered = centros.AsQueryable();
    if (empresaId.HasValue) filtered = filtered.Where(c => c.EmpresaId == empresaId.Value);
    if (!string.IsNullOrWhiteSpace(estado)) filtered = filtered.Where(c => string.Equals(c.Estado, estado, StringComparison.OrdinalIgnoreCase));
    return Results.Ok(filtered.ToList());
}).RequireAuthorization();

app.MapGet("/centros-costo/{id:guid}", async (Guid id, OrganizationService service) =>
{
    var centro = await service.GetCentroCostoAsync(id);
    return centro is null ? Results.NotFound() : Results.Ok(centro);
}).RequireAuthorization();

app.MapPost("/centros-costo", async (CreateCentroCostoRequest request, OrganizationService service) =>
{
    var centro = await service.CreateCentroCostoAsync(request);
    return Results.Created($"/centros-costo/{centro.Id}", centro);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPut("/centros-costo/{id:guid}", async (Guid id, UpdateCentroCostoRequest request, OrganizationService service) =>
{
    var centro = await service.UpdateCentroCostoAsync(id, request);
    return centro is null ? Results.NotFound() : Results.Ok(centro);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapDelete("/centros-costo/{id:guid}", async (Guid id, OrganizationService service) =>
{
    var centro = await service.DeactivateCentroCostoAsync(id);
    return centro is null ? Results.NotFound() : Results.Ok(centro);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapGet("/sindicatos", async (OrganizationService service) => Results.Ok(await service.GetSindicatosAsync()))
    .RequireAuthorization();

app.MapGet("/sindicatos/{id:guid}", async (Guid id, OrganizationService service) =>
{
    var sindicato = await service.GetSindicatoAsync(id);
    return sindicato is null ? Results.NotFound() : Results.Ok(sindicato);
}).RequireAuthorization();

app.MapPost("/sindicatos", async (CreateSindicatoRequest request, OrganizationService service) =>
{
    var sindicato = await service.CreateSindicatoAsync(request);
    return Results.Created($"/sindicatos/{sindicato.Id}", sindicato);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPut("/sindicatos/{id:guid}", async (Guid id, UpdateSindicatoRequest request, OrganizationService service) =>
{
    var sindicato = await service.UpdateSindicatoAsync(id, request);
    return sindicato is null ? Results.NotFound() : Results.Ok(sindicato);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapDelete("/sindicatos/{id:guid}", async (Guid id, OrganizationService service) =>
{
    var sindicato = await service.DeactivateSindicatoAsync(id);
    return sindicato is null ? Results.NotFound() : Results.Ok(sindicato);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapGet("/convenios", async (Guid? sindicatoId, OrganizationService service) =>
    Results.Ok(await service.GetConveniosAsync(sindicatoId)))
    .RequireAuthorization();

app.MapGet("/convenios/{id:guid}", async (Guid id, OrganizationService service) =>
{
    var convenio = await service.GetConvenioAsync(id);
    return convenio is null ? Results.NotFound() : Results.Ok(convenio);
}).RequireAuthorization();

app.MapPost("/convenios", async (CreateConvenioRequest request, OrganizationService service) =>
{
    var convenio = await service.CreateConvenioAsync(request);
    return Results.Created($"/convenios/{convenio.Id}", convenio);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPut("/convenios/{id:guid}", async (Guid id, UpdateConvenioRequest request, OrganizationService service) =>
{
    var convenio = await service.UpdateConvenioAsync(id, request);
    return convenio is null ? Results.NotFound() : Results.Ok(convenio);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapDelete("/convenios/{id:guid}", async (Guid id, OrganizationService service) =>
{
    var convenio = await service.DeactivateConvenioAsync(id);
    return convenio is null ? Results.NotFound() : Results.Ok(convenio);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.Run();

static void EnsureOrganizationSchema(OrganizationDbContext db)
{
    var connection = db.Database.GetDbConnection();
    if (connection.State != ConnectionState.Open)
    {
        connection.Open();
    }

    EnsureTable(connection, "Sindicatos", """
CREATE TABLE IF NOT EXISTS "Sindicatos" (
    "Id" TEXT NOT NULL PRIMARY KEY,
    "Nombre" TEXT NOT NULL,
    "Codigo" TEXT NULL,
    "Jurisdiccion" TEXT NULL,
    "Estado" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "UpdatedAt" TEXT NOT NULL
);
""");

    EnsureTable(connection, "Convenios", """
CREATE TABLE IF NOT EXISTS "Convenios" (
    "Id" TEXT NOT NULL PRIMARY KEY,
    "SindicatoId" TEXT NOT NULL,
    "Nombre" TEXT NOT NULL,
    "Numero" TEXT NULL,
    "VigenciaDesde" TEXT NULL,
    "VigenciaHasta" TEXT NULL,
    "Estado" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "UpdatedAt" TEXT NOT NULL
);
""");
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

public partial class Program { }

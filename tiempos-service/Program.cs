using System.Text;
using TiemposService.Domain.Requests;
using TiemposService.Infrastructure;
using TiemposService.Services;
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

builder.Services.AddDbContext<TiemposDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("TiemposDb")));
builder.Services.AddScoped<ITiemposRepository, EfTiemposRepository>();
builder.Services.AddScoped<TiemposService.Services.TiemposService>();

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
    .AddDbContextCheck<TiemposDbContext>("db", tags: new[] { "ready" });

var otelEnabledRaw = builder.Configuration["OpenTelemetry:Enabled"];
var otelEnabled = bool.TryParse(otelEnabledRaw, out var otelEnabledValue) && otelEnabledValue;
if (otelEnabled)
{
    var serviceName = builder.Configuration.GetValue<string>("OpenTelemetry:ServiceName") ?? "tiempos-service";
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
    var db = scope.ServiceProvider.GetRequiredService<TiemposDbContext>();
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
    service = "tiempos-service",
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

app.MapGet("/turnos", async (TiemposService.Services.TiemposService service) =>
    Results.Ok(await service.GetTurnosAsync()))
    .RequireAuthorization();

app.MapGet("/turnos/{id:guid}", async (Guid id, TiemposService.Services.TiemposService service) =>
{
    var turno = await service.GetTurnoAsync(id);
    return turno is null ? Results.NotFound() : Results.Ok(turno);
}).RequireAuthorization();

app.MapPost("/turnos", async (CreateTurnoRequest request, TiemposService.Services.TiemposService service) =>
{
    var turno = await service.CreateTurnoAsync(request);
    return Results.Created($"/turnos/{turno.Id}", turno);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPut("/turnos/{id:guid}", async (Guid id, UpdateTurnoRequest request, TiemposService.Services.TiemposService service) =>
{
    var turno = await service.UpdateTurnoAsync(id, request);
    return turno is null ? Results.NotFound() : Results.Ok(turno);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapDelete("/turnos/{id:guid}", async (Guid id, TiemposService.Services.TiemposService service) =>
{
    var turno = await service.DeactivateTurnoAsync(id);
    return turno is null ? Results.NotFound() : Results.Ok(turno);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapGet("/horarios", async (TiemposService.Services.TiemposService service) =>
    Results.Ok(await service.GetHorariosAsync()))
    .RequireAuthorization();

app.MapGet("/horarios/{id:guid}", async (Guid id, TiemposService.Services.TiemposService service) =>
{
    var horario = await service.GetHorarioAsync(id);
    return horario is null ? Results.NotFound() : Results.Ok(horario);
}).RequireAuthorization();

app.MapPost("/horarios", async (CreateHorarioRequest request, TiemposService.Services.TiemposService service) =>
{
    var horario = await service.CreateHorarioAsync(request);
    return Results.Created($"/horarios/{horario.Id}", horario);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPut("/horarios/{id:guid}", async (Guid id, UpdateHorarioRequest request, TiemposService.Services.TiemposService service) =>
{
    var horario = await service.UpdateHorarioAsync(id, request);
    return horario is null ? Results.NotFound() : Results.Ok(horario);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapDelete("/horarios/{id:guid}", async (Guid id, TiemposService.Services.TiemposService service) =>
{
    var horario = await service.DeactivateHorarioAsync(id);
    return horario is null ? Results.NotFound() : Results.Ok(horario);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapGet("/fichadas", async (Guid? legajoId, DateTimeOffset? desde, DateTimeOffset? hasta, TiemposService.Services.TiemposService service) =>
    Results.Ok(await service.GetFichadasAsync(legajoId, desde, hasta)))
    .RequireAuthorization();

app.MapGet("/fichadas/{id:guid}", async (Guid id, TiemposService.Services.TiemposService service) =>
{
    var fichada = await service.GetFichadaAsync(id);
    return fichada is null ? Results.NotFound() : Results.Ok(fichada);
}).RequireAuthorization();

app.MapPost("/fichadas", async (CreateFichadaRequest request, TiemposService.Services.TiemposService service) =>
{
    var fichada = await service.CreateFichadaAsync(request);
    return Results.Created($"/fichadas/{fichada.Id}", fichada);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPatch("/fichadas/{id:guid}", async (Guid id, UpdateFichadaRequest request, TiemposService.Services.TiemposService service) =>
{
    var fichada = await service.UpdateFichadaAsync(id, request);
    return fichada is null ? Results.NotFound() : Results.Ok(fichada);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapGet("/planillas", async (Guid? empresaId, string? periodo, TiemposService.Services.TiemposService service) =>
    Results.Ok(await service.GetPlanillasAsync(empresaId, periodo)))
    .RequireAuthorization();

app.MapGet("/planillas/{id:guid}", async (Guid id, TiemposService.Services.TiemposService service) =>
{
    var planilla = await service.GetPlanillaAsync(id);
    return planilla is null ? Results.NotFound() : Results.Ok(planilla);
}).RequireAuthorization();

app.MapPost("/planillas", async (CreatePlanillaRequest request, TiemposService.Services.TiemposService service) =>
{
    var planilla = await service.CreatePlanillaAsync(request);
    return Results.Created($"/planillas/{planilla.Id}", planilla);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPost("/planillas/{id:guid}/cerrar", async (Guid id, TiemposService.Services.TiemposService service) =>
{
    var planilla = await service.ClosePlanillaAsync(id);
    return planilla is null ? Results.NotFound() : Results.Ok(planilla);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapGet("/ausencias", async (Guid? legajoId, string? legajoNumero, DateTimeOffset? desde, DateTimeOffset? hasta, string? tipo, TiemposService.Services.TiemposService service) =>
    Results.Ok(await service.GetAusenciasAsync(legajoId, legajoNumero, desde, hasta, tipo)))
    .RequireAuthorization();

app.MapPost("/ausencias", async (CreateAusenciaRequest request, TiemposService.Services.TiemposService service) =>
{
    var ausencia = await service.CreateAusenciaAsync(request);
    return Results.Created($"/ausencias/{ausencia.Id}", ausencia);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapGet("/ausencias/resumen", async (Guid? legajoId, string? legajoNumero, DateTimeOffset? desde, DateTimeOffset? hasta, string? tipo, TiemposService.Services.TiemposService service) =>
    Results.Ok(await service.GetAusenciasResumenAsync(legajoId, legajoNumero, desde, hasta, tipo)))
    .RequireAuthorization(policy => policy.RequireRole("Admin"));

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
            var (error, detail, code) = GetErrorDetails(exception, "control de tiempos");
            errorMessage = error;
            errorDetail = detail;
            errorCode = code;
            app.Logger.LogError(exception, "Error no manejado en tiempos-service");
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
    var dbEx = ex as Microsoft.EntityFrameworkCore.DbUpdateException;
    var innerMessage = dbEx?.InnerException?.Message ?? ex.Message;
    
    if (innerMessage.Contains("UNIQUE constraint") || innerMessage.Contains("duplicate"))
        return ("Registro duplicado", $"Ya existe {context} con este código. Verifique que no esté repetido.", "DUPLICATE_KEY");
    if (innerMessage.Contains("FOREIGN KEY"))
        return ("Referencia inválida", $"No se puede procesar porque hace referencia a un legajo que no existe.", "FOREIGN_KEY_VIOLATION");
    
    return ex switch
    {
        ArgumentException argEx => ("Parámetro inválido", $"{argEx.Message}", "INVALID_ARGUMENT"),
        KeyNotFoundException _ => ("Recurso no encontrado", $"El {context} solicitado no existe.", "NOT_FOUND"),
        TimeoutException _ => ("Tiempo de espera agotado", "La operación tardó demasiado.", "TIMEOUT"),
        _ => ("Error interno", ex.Message.Length > 400 ? ex.Message.Substring(0, 400) + "..." : ex.Message, "INTERNAL_ERROR")
    };
}

public partial class Program { }

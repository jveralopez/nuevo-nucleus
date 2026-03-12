using System.Diagnostics;
using System.IO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Domain.Requests;
using AuthService.Infrastructure;
using AuthService.Services;
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

var authOptions = new AuthOptions();
builder.Configuration.Bind("Auth", authOptions);
builder.Services.AddSingleton(authOptions);

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("AuthDb")));
builder.Services.AddScoped<IAuthRepository, EfAuthRepository>();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddScoped<AuthService.Services.AuthService>();

var dataProtectionPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(dataProtectionPath))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
        .SetApplicationName("nucleus");
}

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
    .AddDbContextCheck<AuthDbContext>("db", tags: new[] { "ready" });

var otelEnabled = builder.Configuration.GetValue("OpenTelemetry:Enabled", false);
if (otelEnabled)
{
    var serviceName = builder.Configuration.GetValue<string>("OpenTelemetry:ServiceName") ?? "auth-service";
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
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    var applyMigrations = app.Configuration.GetValue("Database:ApplyMigrations", app.Environment.IsDevelopment());
    if (applyMigrations)
    {
        db.Database.Migrate();
    }
}
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

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "auth-service",
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

app.MapPost("/login", async (LoginRequest request, AuthService.Services.AuthService auth, TokenService tokens) =>
{
    var user = await auth.AuthenticateAsync(request);
    if (user is null) return Results.Unauthorized();
    var token = tokens.CreateToken(user);
    return Results.Ok(new { token, user = new { user.Id, user.Username, user.Role } });
});

app.MapPost("/users", async (CreateUserRequest request, AuthService.Services.AuthService auth) =>
{
    var user = await auth.CreateUserAsync(request);
    return Results.Created($"/users/{user.Id}", new { user.Id, user.Username, user.Role, user.Estado });
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapGet("/me", (ClaimsPrincipal user) =>
{
    var id = user.FindFirstValue(JwtRegisteredClaimNames.Sub);
    var username = user.Identity?.Name ?? user.FindFirstValue(JwtRegisteredClaimNames.UniqueName);
    var role = user.FindFirstValue(ClaimTypes.Role);
    return Results.Ok(new { id, username, role });
}).RequireAuthorization();

using (var scope = app.Services.CreateScope())
{
    var auth = scope.ServiceProvider.GetRequiredService<AuthService.Services.AuthService>();
    await auth.EnsureSeedAdminAsync();
}

app.Run();

using LiquidacionService.Domain.Models;
using LiquidacionService.Domain.Requests;
using LiquidacionService.Infrastructure;
using LiquidacionService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});
builder.Services.AddSingleton<IPayrollRepository, FilePayrollRepository>();
builder.Services.AddSingleton<ReceiptCalculator>();
builder.Services.AddSingleton<ReceiptExporter>();
builder.Services.AddScoped<PayrollService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseCors();

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "liquidacion-service",
    version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0"
}));

app.MapGet("/payrolls", async (PayrollService service) => Results.Ok(await service.GetAllAsync()));

app.MapGet("/payrolls/{id:guid}", async (Guid id, PayrollService service) =>
{
    var payroll = await service.GetAsync(id);
    return payroll is null ? Results.NotFound() : Results.Ok(payroll);
});

app.MapPost("/payrolls", async (NewPayrollRequest request, PayrollService service) =>
{
    var payroll = await service.CreateAsync(request);
    return Results.Created($"/payrolls/{payroll.Id}", payroll);
});

app.MapPost("/payrolls/{id:guid}/legajos", async (Guid id, UpsertLegajoRequest request, PayrollService service) =>
{
    var payroll = await service.AddLegajoAsync(id, request);
    return Results.Ok(payroll);
});

app.MapDelete("/payrolls/{id:guid}/legajos/{legajoId:guid}", async (Guid id, Guid legajoId, PayrollService service) =>
{
    var ok = await service.RemoveLegajoAsync(id, legajoId);
    return ok ? Results.NoContent() : Results.NotFound();
});

app.MapPost("/payrolls/{id:guid}/procesar", async (Guid id, ProcessPayrollRequest request, PayrollService service) =>
{
    var payroll = await service.ProcessAsync(id, request?.Exportar ?? false);
    return payroll is null ? Results.NotFound() : Results.Ok(payroll);
});

app.MapGet("/payrolls/{id:guid}/recibos", async (Guid id, PayrollService service) =>
{
    var recibos = await service.GetRecibosAsync(id);
    return recibos is null ? Results.NotFound() : Results.Ok(recibos);
});

app.MapGet("/exports/{fileName}", async (string fileName, ReceiptExporter exporter) =>
{
    var stream = exporter.OpenExport(fileName);
    if (stream is null) return Results.NotFound();
    var contentType = fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) ? "text/csv" : "application/json";
    return Results.File(stream, contentType, fileName);
});

app.Run();

using LiquidacionService.Domain.Models;
using LiquidacionService.Domain.Requests;
using LiquidacionService.Services;
using LiquidacionService.Tests.Helpers;
using Xunit;

namespace LiquidacionService.Tests;

public class ReceiptExporterTests
{
    [Fact]
    public async Task ListExports_ReturnsGeneratedFiles()
    {
        var env = new TestHostEnvironment();
        var exportRoot = Path.Combine(env.ContentRootPath, "storage");
        if (Directory.Exists(exportRoot))
        {
            Directory.Delete(exportRoot, recursive: true);
        }

        var exporter = new ReceiptExporter(env);
        var payroll = PayrollRun.Create(new NewPayrollRequest("2026-04", "Mensual", "Test"));
        payroll.SetRecibos(new[]
        {
            new PayrollReceipt
            {
                PayrollRunId = payroll.Id,
                LegajoId = Guid.NewGuid(),
                LegajoNumero = "100",
                LegajoNombre = "Ana Perez",
                Remunerativo = 1200,
                Deducciones = 200,
                Neto = 1000
            }
        });

        var before = exporter.ListExports(payroll);

        await exporter.ExportAsync(payroll);
        var after = exporter.ListExports(payroll);

        Assert.Empty(before);
        Assert.Contains(after, file => file.EndsWith(".json", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(after, file => file.EndsWith(".csv", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task OpenExport_RejectsPathTraversal()
    {
        var env = new TestHostEnvironment();
        var exportRoot = Path.Combine(env.ContentRootPath, "storage");
        if (Directory.Exists(exportRoot))
        {
            Directory.Delete(exportRoot, recursive: true);
        }

        var exporter = new ReceiptExporter(env);
        var payroll = PayrollRun.Create(new NewPayrollRequest("2026-05", "Mensual", "Test"));
        payroll.SetRecibos(new[]
        {
            new PayrollReceipt
            {
                PayrollRunId = payroll.Id,
                LegajoId = Guid.NewGuid(),
                LegajoNumero = "101",
                LegajoNombre = "Juan Gomez",
                Remunerativo = 900,
                Deducciones = 150,
                Neto = 750
            }
        });

        await exporter.ExportAsync(payroll);

        Assert.Null(exporter.OpenExport("../export.csv"));
        Assert.Null(exporter.OpenExport("..\\export.csv"));
    }
}

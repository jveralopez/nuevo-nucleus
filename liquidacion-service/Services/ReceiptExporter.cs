using System.Text;
using System.Text.Json;
using LiquidacionService.Domain.Models;

namespace LiquidacionService.Services;

public class ReceiptExporter
{
    private readonly string _exportDir;
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public ReceiptExporter(IHostEnvironment env)
    {
        _exportDir = Path.Combine(env.ContentRootPath, "storage", "exports");
        Directory.CreateDirectory(_exportDir);
    }

    public async Task<IReadOnlyList<string>> ExportAsync(PayrollRun payroll)
    {
        var fileBase = $"{payroll.Periodo}-{payroll.Id.ToString("N").Substring(0, 8)}";
        var jsonPath = Path.Combine(_exportDir, fileBase + ".json");
        await using (var stream = File.Create(jsonPath))
        {
            await JsonSerializer.SerializeAsync(stream, payroll.Recibos, _options);
        }

        var csvPath = Path.Combine(_exportDir, fileBase + ".csv");
        var csv = BuildCsv(payroll);
        await File.WriteAllTextAsync(csvPath, csv, Encoding.UTF8);

        return new[] { Path.GetFileName(jsonPath), Path.GetFileName(csvPath) };
    }

    public Stream? OpenExport(string fileName)
    {
        var fullPath = Path.Combine(_exportDir, fileName);
        return File.Exists(fullPath) ? File.OpenRead(fullPath) : null;
    }

    private static string BuildCsv(PayrollRun payroll)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Legajo,Nombre,Remunerativo,Deducciones,Neto");
        foreach (var recibo in payroll.Recibos)
        {
            sb.AppendLine($"{recibo.LegajoNumero},{recibo.LegajoNombre},{recibo.Remunerativo:F2},{recibo.Deducciones:F2},{recibo.Neto:F2}");
        }
        return sb.ToString();
    }
}

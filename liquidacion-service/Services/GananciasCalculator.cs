using System.Text.Json;
using LiquidacionService.Domain.Models;

namespace LiquidacionService.Services;

public class GananciasCalculator
{
    private readonly string _art94Path;
    private readonly string _art30Path;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public GananciasCalculator(IHostEnvironment env)
    {
        _art94Path = Path.Combine(env.ContentRootPath, "..", "data", "reglas", "ganancias", "art94.json");
        _art30Path = Path.Combine(env.ContentRootPath, "..", "data", "reglas", "ganancias", "art30.json");
    }

    public decimal CalcularGanancias(decimal remunerativo, decimal deduccionesAfectan, LegajoEnLote legajo)
    {
        if (!legajo.AplicaGanancias || legajo.OmitirGanancias)
        {
            return 0m;
        }

        var deduccionesPersonales = GetDeduccionesPersonales(legajo);
        var baseImponible = remunerativo - deduccionesAfectan - deduccionesPersonales;
        if (baseImponible <= 0)
        {
            return 0m;
        }

        var escala = LoadArt94();
        if (escala.Rows.Count == 0)
        {
            return 0m;
        }

        var tramo = escala.Rows.FirstOrDefault(r => baseImponible >= r.Desde && baseImponible <= r.Hasta)
                     ?? escala.Rows.OrderByDescending(r => r.Desde).First();

        var impuesto = tramo.CuotaFija + (baseImponible - tramo.Desde) * tramo.Alicuota;
        return Math.Max(0m, Math.Round(impuesto, 2, MidpointRounding.AwayFromZero));
    }

    private decimal GetDeduccionesPersonales(LegajoEnLote legajo)
    {
        var tabla = LoadArt30();
        if (tabla.Rows.Count == 0)
        {
            return legajo.DeduccionesAdicionales;
        }

        decimal total = 0m;
        total += GetImporte(tabla, "GAN_NO_IMPONIBLE");
        total += GetImporte(tabla, "DED_ESPECIAL");
        if (legajo.ConyugeACargo)
        {
            total += GetImporte(tabla, "CONYUGE");
        }

        if (legajo.CantHijos > 0)
        {
            total += GetImporte(tabla, "HIJO") * legajo.CantHijos;
        }

        if (legajo.CantOtrosFamiliares > 0)
        {
            total += GetImporte(tabla, "OTRO_FAMILIAR") * legajo.CantOtrosFamiliares;
        }

        total += legajo.DeduccionesAdicionales;
        return Math.Max(0m, Math.Round(total, 2, MidpointRounding.AwayFromZero));
    }

    private decimal GetImporte(Art30Table table, string codigo)
    {
        var row = table.Rows.FirstOrDefault(r => string.Equals(r.Codigo, codigo, StringComparison.OrdinalIgnoreCase));
        return row?.Importe ?? 0m;
    }

    private Art94Table LoadArt94()
    {
        if (!File.Exists(_art94Path))
        {
            return new Art94Table();
        }

        using var stream = File.OpenRead(_art94Path);
        var table = JsonSerializer.Deserialize<Art94Table>(stream, _jsonOptions);
        return table ?? new Art94Table();
    }

    private Art30Table LoadArt30()
    {
        if (!File.Exists(_art30Path))
        {
            return new Art30Table();
        }

        using var stream = File.OpenRead(_art30Path);
        var table = JsonSerializer.Deserialize<Art30Table>(stream, _jsonOptions);
        return table ?? new Art30Table();
    }

    private sealed class Art94Table
    {
        public List<Art94Row> Rows { get; set; } = new();
    }

    private sealed class Art94Row
    {
        public decimal Desde { get; set; }
        public decimal Hasta { get; set; }
        public decimal CuotaFija { get; set; }
        public decimal Alicuota { get; set; }
    }

    private sealed class Art30Table
    {
        public List<Art30Row> Rows { get; set; } = new();
    }

    private sealed class Art30Row
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Importe { get; set; }
        public string Unidad { get; set; } = "ARS";
    }
}

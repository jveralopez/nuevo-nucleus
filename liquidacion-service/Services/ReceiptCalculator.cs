using LiquidacionService.Domain.Models;

namespace LiquidacionService.Services;

public class ReceiptCalculator
{
    private const decimal Jubilacion = 0.11m;
    private const decimal ObraSocial = 0.03m;
    private const decimal Sindicato = 0.02m;

    public PayrollReceipt BuildReceipt(PayrollRun payroll, LegajoEnLote legajo)
    {
        var remunerativo = legajo.Basico + legajo.Antiguedad + legajo.Adicionales;
        remunerativo = Math.Round(Math.Max(remunerativo, 0), 2, MidpointRounding.AwayFromZero);

        var deduccionesAuto = Math.Round(remunerativo * (Jubilacion + ObraSocial + Sindicato), 2, MidpointRounding.AwayFromZero);
        var totalesDeducciones = Math.Max(0, legajo.Descuentos + deduccionesAuto);
        var neto = Math.Max(0, remunerativo - totalesDeducciones);

        var detalle = new List<ReceiptDetail>
        {
            new("Básico", legajo.Basico),
            new("Antigüedad", legajo.Antiguedad),
            new("Adicionales", legajo.Adicionales),
            new("Descuentos manuales", -legajo.Descuentos),
            new("Aportes jubilación", -Math.Round(remunerativo * Jubilacion, 2)),
            new("Obra social", -Math.Round(remunerativo * ObraSocial, 2)),
            new("Sindicato", -Math.Round(remunerativo * Sindicato, 2))
        };

        return new PayrollReceipt
        {
            PayrollRunId = payroll.Id,
            LegajoId = legajo.Id,
            LegajoNumero = legajo.Numero,
            LegajoNombre = legajo.Nombre,
            Remunerativo = remunerativo,
            Deducciones = totalesDeducciones,
            Neto = neto,
            Detalle = detalle
        };
    }
}

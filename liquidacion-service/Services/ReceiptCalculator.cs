using LiquidacionService.Domain.Models;

namespace LiquidacionService.Services;

public class ReceiptCalculator
{
    private readonly ConceptRuleEngine _rules;
    private readonly GananciasCalculator _ganancias;

    public ReceiptCalculator(ConceptRuleEngine rules, GananciasCalculator ganancias)
    {
        _rules = rules;
        _ganancias = ganancias;
    }

    public PayrollReceipt BuildReceipt(PayrollRun payroll, LegajoEnLote legajo)
    {
        var rules = _rules.GetRules(legajo.Convenio).Where(r => r.Activo).ToList();
        var detalle = new List<ReceiptDetail>();

        var remunerativo = 0m;
        var noRemunerativo = legajo.NoRemunerativo + legajo.BonosNoRemunerativos;
        var deducciones = 0m;
        var deduccionesAfectanGanancias = 0m;

        foreach (var rule in rules.Where(r => r.Tipo != ConceptRuleType.Deduccion))
        {
            var importe = EvaluateRule(rule, legajo, remunerativo);
            if (importe == 0m)
            {
                continue;
            }

            switch (rule.Tipo)
            {
                case ConceptRuleType.Remunerativo:
                    remunerativo += importe;
                    detalle.Add(new ReceiptDetail(rule.Descripcion, importe));
                    break;
                case ConceptRuleType.NoRemunerativo:
                    noRemunerativo += importe;
                    detalle.Add(new ReceiptDetail(rule.Descripcion, importe));
                    break;
            }
        }

        foreach (var rule in rules.Where(r => r.Tipo == ConceptRuleType.Deduccion))
        {
            var importe = EvaluateRule(rule, legajo, remunerativo);
            if (importe == 0m)
            {
                continue;
            }

            deducciones += importe;
            if (rule.AfectaGanancias)
            {
                deduccionesAfectanGanancias += importe;
            }
            detalle.Add(new ReceiptDetail(rule.Descripcion, -importe));
        }

        if (noRemunerativo > 0)
        {
            detalle.Add(new ReceiptDetail("No remunerativo", noRemunerativo));
        }

        if (legajo.VacacionesDias > 0)
        {
            var vacaciones = Math.Round((legajo.Basico / 30m) * legajo.VacacionesDias, 2, MidpointRounding.AwayFromZero);
            remunerativo += vacaciones;
            detalle.Add(new ReceiptDetail("Vacaciones", vacaciones));
        }

        var diasSinGoce = legajo.Licencias.Where(l => !l.ConGoce).Sum(l => l.Dias);
        if (diasSinGoce > 0)
        {
            var descuentoSinGoce = Math.Round((legajo.Basico / 30m) * diasSinGoce, 2, MidpointRounding.AwayFromZero);
            deducciones += descuentoSinGoce;
            detalle.Add(new ReceiptDetail("Licencia sin goce", -descuentoSinGoce));
        }

        deducciones += Math.Max(0m, legajo.Descuentos);
        if (legajo.Descuentos > 0)
        {
            detalle.Add(new ReceiptDetail("Descuentos manuales", -legajo.Descuentos));
        }

        var impuestoGanancias = _ganancias.CalcularGanancias(remunerativo, deduccionesAfectanGanancias, legajo);
        if (impuestoGanancias > 0)
        {
            deducciones += impuestoGanancias;
            detalle.Add(new ReceiptDetail("Impuesto a las ganancias", -impuestoGanancias));
        }

        var netoPreEmbargos = Math.Max(0m, remunerativo - deducciones + noRemunerativo);
        var embargosTotal = CalcularEmbargos(legajo.Embargos, remunerativo, netoPreEmbargos);
        if (embargosTotal > 0)
        {
            deducciones += embargosTotal;
            detalle.Add(new ReceiptDetail("Embargos", -embargosTotal));
        }

        var neto = Math.Max(0m, remunerativo - deducciones + noRemunerativo);
        var contribuciones = (legajo.ContribucionesPatronales ?? new List<LegajoEmployerContribution>())
            .Select(c => new EmployerContribution(c.Concepto, c.Importe, c.Grupo))
            .ToList();
        var contribucionesTotal = contribuciones.Sum(c => c.Importe);

        return new PayrollReceipt
        {
            PayrollRunId = payroll.Id,
            LegajoId = legajo.Id,
            LegajoNumero = legajo.Numero,
            LegajoNombre = legajo.Nombre,
            Remunerativo = Math.Round(Math.Max(remunerativo, 0), 2, MidpointRounding.AwayFromZero),
            Deducciones = Math.Round(Math.Max(deducciones, 0), 2, MidpointRounding.AwayFromZero),
            Neto = Math.Round(neto, 2, MidpointRounding.AwayFromZero),
            ContribucionesPatronalesTotal = Math.Round(Math.Max(contribucionesTotal, 0), 2, MidpointRounding.AwayFromZero),
            ContribucionesPatronales = contribuciones,
            Detalle = detalle
        };
    }

    private static decimal EvaluateRule(ConceptRule rule, LegajoEnLote legajo, decimal remunerativoActual)
    {
        switch (rule.Formula)
        {
            case ConceptFormulaType.LegajoField:
                return rule.LegajoField?.ToLowerInvariant() switch
                {
                    "basico" => legajo.Basico,
                    "antiguedad" => legajo.Antiguedad,
                    "adicionales" => legajo.Adicionales,
                    "presentismo" => legajo.Presentismo,
                    "horasextra" => legajo.HorasExtra,
                    "premios" => legajo.Premios,
                    _ => 0m
                };
            case ConceptFormulaType.PercentOf:
                var baseValue = rule.Base?.ToLowerInvariant() switch
                {
                    "remunerativo" => remunerativoActual,
                    "basico" => legajo.Basico,
                    _ => remunerativoActual
                };
                return Math.Round(baseValue * (rule.Rate ?? 0m), 2, MidpointRounding.AwayFromZero);
            case ConceptFormulaType.Fixed:
                return rule.Amount ?? 0m;
            default:
                return 0m;
        }
    }

    private static decimal CalcularEmbargos(IEnumerable<Embargo> embargos, decimal remunerativo, decimal neto)
    {
        decimal total = 0m;
        foreach (var embargo in embargos.Where(e => e.Activo))
        {
            var baseCalculo = embargo.BaseCalculo.Equals("Bruto", StringComparison.OrdinalIgnoreCase)
                ? remunerativo
                : neto;
            var monto = 0m;
            if (embargo.Porcentaje.HasValue && embargo.Porcentaje.Value > 0)
            {
                monto = baseCalculo * embargo.Porcentaje.Value;
            }
            else if (embargo.MontoFijo.HasValue)
            {
                monto = embargo.MontoFijo.Value;
            }

            if (embargo.MontoPendiente.HasValue)
            {
                monto = Math.Min(monto, embargo.MontoPendiente.Value);
            }
            else if (embargo.MontoTotal.HasValue)
            {
                monto = Math.Min(monto, embargo.MontoTotal.Value);
            }

            total += Math.Max(0m, Math.Round(monto, 2, MidpointRounding.AwayFromZero));
        }

        return total;
    }
}

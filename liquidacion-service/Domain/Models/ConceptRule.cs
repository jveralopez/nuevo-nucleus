namespace LiquidacionService.Domain.Models;

public class ConceptRule
{
    public string Codigo { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public ConceptRuleType Tipo { get; init; } = ConceptRuleType.Remunerativo;
    public ConceptFormulaType Formula { get; init; } = ConceptFormulaType.LegajoField;
    public string? LegajoField { get; init; }
    public string? Base { get; init; }
    public decimal? Rate { get; init; }
    public decimal? Amount { get; init; }
    public bool Activo { get; init; } = true;
    public bool AfectaGanancias { get; init; }
    public string? Source { get; init; }
}

public enum ConceptRuleType
{
    Remunerativo,
    NoRemunerativo,
    Deduccion
}

public enum ConceptFormulaType
{
    LegajoField,
    PercentOf,
    Fixed
}

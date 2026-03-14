namespace LiquidacionService.Domain.Models;

public class LegajoEmployerContribution
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Concepto { get; init; } = string.Empty;
    public decimal Importe { get; init; }
    public string? Grupo { get; init; }
}

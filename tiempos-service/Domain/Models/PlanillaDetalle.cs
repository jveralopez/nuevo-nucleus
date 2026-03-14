namespace TiemposService.Domain.Models;

public class PlanillaDetalle
{
    public Guid Id { get; set; }
    public Guid PlanillaId { get; set; }
    public Guid LegajoId { get; set; }
    public decimal HorasNormales { get; set; }
    public decimal HorasExtra { get; set; }
    public decimal HorasAusencia { get; set; }
    public string? Observaciones { get; set; }
}

namespace TiemposService.Domain.Requests;

public class PlanillaDetalleRequest
{
    public Guid LegajoId { get; set; }
    public decimal HorasNormales { get; set; }
    public decimal HorasExtra { get; set; }
    public decimal HorasAusencia { get; set; }
    public string? Observaciones { get; set; }
}

namespace TiemposService.Domain.Models;

public class Ausencia
{
    public Guid Id { get; set; }
    public Guid? LegajoId { get; set; }
    public string LegajoNumero { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public DateTimeOffset FechaDesde { get; set; }
    public DateTimeOffset FechaHasta { get; set; }
    public string Origen { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

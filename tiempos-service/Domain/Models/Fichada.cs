namespace TiemposService.Domain.Models;

public class Fichada
{
    public Guid Id { get; set; }
    public Guid LegajoId { get; set; }
    public DateTimeOffset FechaHora { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Origen { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

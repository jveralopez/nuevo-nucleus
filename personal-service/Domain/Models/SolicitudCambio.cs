namespace PersonalService.Domain.Models;

public class SolicitudCambio
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LegajoId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string? Detalle { get; set; }
    public string Estado { get; set; } = "PendAprob";
    public string? DatosJson { get; set; }
    public string? Observaciones { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

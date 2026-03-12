namespace PersonalService.Domain.Models;

public class DocumentoPersonal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Tipo { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public DateTime? FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public string? Observaciones { get; set; }
    public Guid LegajoId { get; set; }
}

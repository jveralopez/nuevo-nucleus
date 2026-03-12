namespace PersonalService.Domain.Requests;

public class DocumentoPersonalRequest
{
    public string Tipo { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public DateTime? FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public string? Observaciones { get; set; }
}

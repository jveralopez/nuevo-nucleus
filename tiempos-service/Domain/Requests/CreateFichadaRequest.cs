namespace TiemposService.Domain.Requests;

public class CreateFichadaRequest
{
    public Guid LegajoId { get; set; }
    public DateTimeOffset FechaHora { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Origen { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
}

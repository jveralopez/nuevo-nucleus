namespace TiemposService.Domain.Requests;

public class UpdateFichadaRequest
{
    public DateTimeOffset? FechaHora { get; set; }
    public string? Tipo { get; set; }
    public string? Origen { get; set; }
    public string? Observaciones { get; set; }
}

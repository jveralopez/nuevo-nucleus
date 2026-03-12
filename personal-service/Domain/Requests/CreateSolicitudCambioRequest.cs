namespace PersonalService.Domain.Requests;

public class CreateSolicitudCambioRequest
{
    public Guid LegajoId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string? Detalle { get; set; }
    public string? DatosJson { get; set; }
}

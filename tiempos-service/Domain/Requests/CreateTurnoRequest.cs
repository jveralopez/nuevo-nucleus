namespace TiemposService.Domain.Requests;

public class CreateTurnoRequest
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public TimeSpan HoraInicio { get; set; }
    public TimeSpan HoraFin { get; set; }
    public int ToleranciaMinutos { get; set; }
    public bool Activo { get; set; } = true;
}

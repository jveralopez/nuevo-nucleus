namespace TiemposService.Domain.Requests;

public class CreateHorarioRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string DiasSemana { get; set; } = string.Empty;
    public Guid TurnoId { get; set; }
    public bool Activo { get; set; } = true;
}

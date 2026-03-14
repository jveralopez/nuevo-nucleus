namespace TiemposService.Domain.Models;

public class Horario
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string DiasSemana { get; set; } = string.Empty;
    public Guid TurnoId { get; set; }
    public bool Activo { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

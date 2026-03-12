namespace OrganizacionService.Domain.Models;

public class Posicion
{
    public Guid Id { get; set; }
    public Guid UnidadId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Nivel { get; set; }
    public string? Perfil { get; set; }
    public string Estado { get; set; } = "Activa";
    public List<Guid> LegajoIds { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

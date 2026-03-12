namespace OrganizacionService.Domain.Models;

public class OrganigramaVersion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmpresaId { get; set; }
    public int Version { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string UnidadesJson { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

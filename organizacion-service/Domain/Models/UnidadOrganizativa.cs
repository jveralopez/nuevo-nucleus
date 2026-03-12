namespace OrganizacionService.Domain.Models;

public class UnidadOrganizativa
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public Guid? PadreId { get; set; }
    public Guid? CentroCostoId { get; set; }
    public string Estado { get; set; } = "Activa";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

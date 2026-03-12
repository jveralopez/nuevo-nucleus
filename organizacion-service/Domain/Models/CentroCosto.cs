namespace OrganizacionService.Domain.Models;

public class CentroCosto
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Moneda { get; set; } = string.Empty;
    public string Estado { get; set; } = "Activa";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

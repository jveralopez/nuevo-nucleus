namespace OrganizacionService.Domain.Models;

public class Empresa
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
    public string Moneda { get; set; } = string.Empty;
    public string Estado { get; set; } = "Activa";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

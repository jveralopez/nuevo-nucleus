namespace OrganizacionService.Domain.Models;

public class Sindicato
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public string? Jurisdiccion { get; set; }
    public string Estado { get; set; } = "Activo";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

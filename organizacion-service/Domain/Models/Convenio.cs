namespace OrganizacionService.Domain.Models;

public class Convenio
{
    public Guid Id { get; set; }
    public Guid SindicatoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Numero { get; set; }
    public DateTimeOffset? VigenciaDesde { get; set; }
    public DateTimeOffset? VigenciaHasta { get; set; }
    public string Estado { get; set; } = "Activo";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

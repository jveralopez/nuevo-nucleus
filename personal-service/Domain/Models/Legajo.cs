namespace PersonalService.Domain.Models;

public class Legajo
{
    public Guid Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string Cuil { get; set; } = string.Empty;
    public string? EstadoCivil { get; set; }
    public DateTime? FechaIngreso { get; set; }
    public string? Convenio { get; set; }
    public string? Categoria { get; set; }
    public string? ObraSocial { get; set; }
    public string? Sindicato { get; set; }
    public string? TipoPersonal { get; set; }
    public string? Ubicacion { get; set; }
    public string? Email { get; set; }
    public string Estado { get; set; } = "Activo";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<Familiar> Familiares { get; set; } = new();
    public List<Licencia> Licencias { get; set; } = new();
    public List<Domicilio> Domicilios { get; set; } = new();
    public List<DocumentoPersonal> Documentos { get; set; } = new();
    public List<SolicitudCambio> Solicitudes { get; set; } = new();
}

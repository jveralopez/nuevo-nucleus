using OrganizacionService.Domain.Models;

namespace OrganizacionService.Infrastructure;

public class OrganizationStore
{
    public List<Empresa> Empresas { get; set; } = new();
    public List<UnidadOrganizativa> Unidades { get; set; } = new();
    public List<Posicion> Posiciones { get; set; } = new();
    public List<CentroCosto> CentrosCosto { get; set; } = new();
    public List<Sindicato> Sindicatos { get; set; } = new();
    public List<Convenio> Convenios { get; set; } = new();
    public List<OrganigramaVersion> Organigramas { get; set; } = new();
}

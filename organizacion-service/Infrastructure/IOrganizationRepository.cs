using OrganizacionService.Domain.Models;

namespace OrganizacionService.Infrastructure;

public interface IOrganizationRepository
{
    Task<IReadOnlyCollection<Empresa>> GetEmpresasAsync();
    Task<Empresa?> GetEmpresaAsync(Guid id);
    Task SaveEmpresaAsync(Empresa empresa);

    Task<IReadOnlyCollection<UnidadOrganizativa>> GetUnidadesAsync();
    Task<UnidadOrganizativa?> GetUnidadAsync(Guid id);
    Task SaveUnidadAsync(UnidadOrganizativa unidad);

    Task<IReadOnlyCollection<Posicion>> GetPosicionesAsync();
    Task<Posicion?> GetPosicionAsync(Guid id);
    Task SavePosicionAsync(Posicion posicion);

    Task<IReadOnlyCollection<CentroCosto>> GetCentrosCostoAsync();
    Task<CentroCosto?> GetCentroCostoAsync(Guid id);
    Task SaveCentroCostoAsync(CentroCosto centroCosto);

    Task<IReadOnlyCollection<Sindicato>> GetSindicatosAsync();
    Task<Sindicato?> GetSindicatoAsync(Guid id);
    Task SaveSindicatoAsync(Sindicato sindicato);

    Task<IReadOnlyCollection<Convenio>> GetConveniosAsync(Guid? sindicatoId);
    Task<Convenio?> GetConvenioAsync(Guid id);
    Task SaveConvenioAsync(Convenio convenio);

    Task<IReadOnlyCollection<OrganigramaVersion>> GetOrganigramasAsync(Guid? empresaId);
    Task<OrganigramaVersion?> GetOrganigramaAsync(Guid id);
    Task SaveOrganigramaAsync(OrganigramaVersion version);
}

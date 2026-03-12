using Microsoft.EntityFrameworkCore;
using OrganizacionService.Domain.Models;

namespace OrganizacionService.Infrastructure;

public class EfOrganizationRepository : IOrganizationRepository
{
    private readonly OrganizationDbContext _db;

    public EfOrganizationRepository(OrganizationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<Empresa>> GetEmpresasAsync() =>
        await _db.Empresas.AsNoTracking().ToListAsync();

    public async Task<Empresa?> GetEmpresaAsync(Guid id) =>
        await _db.Empresas.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

    public async Task SaveEmpresaAsync(Empresa empresa)
    {
        var exists = await _db.Empresas.AnyAsync(e => e.Id == empresa.Id);
        if (exists) _db.Empresas.Update(empresa);
        else _db.Empresas.Add(empresa);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<UnidadOrganizativa>> GetUnidadesAsync() =>
        await _db.Unidades.AsNoTracking().ToListAsync();

    public async Task<UnidadOrganizativa?> GetUnidadAsync(Guid id) =>
        await _db.Unidades.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

    public async Task SaveUnidadAsync(UnidadOrganizativa unidad)
    {
        var exists = await _db.Unidades.AnyAsync(u => u.Id == unidad.Id);
        if (exists) _db.Unidades.Update(unidad);
        else _db.Unidades.Add(unidad);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<Posicion>> GetPosicionesAsync() =>
        await _db.Posiciones.AsNoTracking().ToListAsync();

    public async Task<Posicion?> GetPosicionAsync(Guid id) =>
        await _db.Posiciones.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

    public async Task SavePosicionAsync(Posicion posicion)
    {
        var exists = await _db.Posiciones.AnyAsync(p => p.Id == posicion.Id);
        if (exists) _db.Posiciones.Update(posicion);
        else _db.Posiciones.Add(posicion);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<CentroCosto>> GetCentrosCostoAsync() =>
        await _db.CentrosCosto.AsNoTracking().ToListAsync();

    public async Task<CentroCosto?> GetCentroCostoAsync(Guid id) =>
        await _db.CentrosCosto.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

    public async Task SaveCentroCostoAsync(CentroCosto centroCosto)
    {
        var exists = await _db.CentrosCosto.AnyAsync(c => c.Id == centroCosto.Id);
        if (exists) _db.CentrosCosto.Update(centroCosto);
        else _db.CentrosCosto.Add(centroCosto);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<Sindicato>> GetSindicatosAsync() =>
        await _db.Sindicatos.AsNoTracking().ToListAsync();

    public async Task<Sindicato?> GetSindicatoAsync(Guid id) =>
        await _db.Sindicatos.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);

    public async Task SaveSindicatoAsync(Sindicato sindicato)
    {
        var exists = await _db.Sindicatos.AnyAsync(s => s.Id == sindicato.Id);
        if (exists) _db.Sindicatos.Update(sindicato);
        else _db.Sindicatos.Add(sindicato);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<Convenio>> GetConveniosAsync(Guid? sindicatoId)
    {
        var query = _db.Convenios.AsNoTracking().AsQueryable();
        if (sindicatoId.HasValue)
        {
            query = query.Where(c => c.SindicatoId == sindicatoId.Value);
        }
        return await query.ToListAsync();
    }

    public async Task<Convenio?> GetConvenioAsync(Guid id) =>
        await _db.Convenios.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

    public async Task SaveConvenioAsync(Convenio convenio)
    {
        var exists = await _db.Convenios.AnyAsync(c => c.Id == convenio.Id);
        if (exists) _db.Convenios.Update(convenio);
        else _db.Convenios.Add(convenio);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<OrganigramaVersion>> GetOrganigramasAsync(Guid? empresaId)
    {
        var query = _db.Organigramas.AsNoTracking().AsQueryable();
        if (empresaId.HasValue) query = query.Where(o => o.EmpresaId == empresaId.Value);
        return await query.OrderByDescending(o => o.Version).ToListAsync();
    }

    public async Task<OrganigramaVersion?> GetOrganigramaAsync(Guid id) =>
        await _db.Organigramas.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);

    public async Task SaveOrganigramaAsync(OrganigramaVersion version)
    {
        var exists = await _db.Organigramas.AnyAsync(o => o.Id == version.Id);
        if (exists) _db.Organigramas.Update(version);
        else _db.Organigramas.Add(version);
        await _db.SaveChangesAsync();
    }
}

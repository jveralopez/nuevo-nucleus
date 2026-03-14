using Microsoft.EntityFrameworkCore;
using TiemposService.Domain.Models;

namespace TiemposService.Infrastructure;

public class EfTiemposRepository : ITiemposRepository
{
    private readonly TiemposDbContext _db;

    public EfTiemposRepository(TiemposDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<Turno>> GetTurnosAsync() =>
        await _db.Turnos.AsNoTracking().ToListAsync();

    public async Task<Turno?> GetTurnoAsync(Guid id) =>
        await _db.Turnos.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);

    public async Task<Turno?> GetTurnoByCodigoAsync(string codigo) =>
        await _db.Turnos.AsNoTracking().FirstOrDefaultAsync(t => t.Codigo.ToLower() == codigo.ToLower());

    public async Task SaveTurnoAsync(Turno turno)
    {
        var exists = await _db.Turnos.AnyAsync(t => t.Id == turno.Id);
        if (exists) _db.Turnos.Update(turno);
        else _db.Turnos.Add(turno);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<Horario>> GetHorariosAsync() =>
        await _db.Horarios.AsNoTracking().ToListAsync();

    public async Task<Horario?> GetHorarioAsync(Guid id) =>
        await _db.Horarios.AsNoTracking().FirstOrDefaultAsync(h => h.Id == id);

    public async Task SaveHorarioAsync(Horario horario)
    {
        var exists = await _db.Horarios.AnyAsync(h => h.Id == horario.Id);
        if (exists) _db.Horarios.Update(horario);
        else _db.Horarios.Add(horario);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<Fichada>> GetFichadasAsync(Guid? legajoId, DateTimeOffset? desde, DateTimeOffset? hasta)
    {
        var query = _db.Fichadas.AsNoTracking().AsQueryable();
        if (legajoId.HasValue) query = query.Where(f => f.LegajoId == legajoId.Value);
        if (desde.HasValue) query = query.Where(f => f.FechaHora >= desde.Value);
        if (hasta.HasValue) query = query.Where(f => f.FechaHora <= hasta.Value);
        var items = await query.ToListAsync();
        return items.OrderByDescending(f => f.FechaHora).ToList();
    }

    public async Task<Fichada?> GetFichadaAsync(Guid id) =>
        await _db.Fichadas.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);

    public async Task SaveFichadaAsync(Fichada fichada)
    {
        var exists = await _db.Fichadas.AnyAsync(f => f.Id == fichada.Id);
        if (exists) _db.Fichadas.Update(fichada);
        else _db.Fichadas.Add(fichada);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<PlanillaHoras>> GetPlanillasAsync(Guid? empresaId, string? periodo)
    {
        var query = _db.Planillas.AsNoTracking().Include(p => p.Detalles).AsQueryable();
        if (empresaId.HasValue) query = query.Where(p => p.EmpresaId == empresaId.Value);
        if (!string.IsNullOrWhiteSpace(periodo)) query = query.Where(p => p.Periodo == periodo);
        return await query.OrderByDescending(p => p.Periodo).ToListAsync();
    }

    public async Task<PlanillaHoras?> GetPlanillaAsync(Guid id) =>
        await _db.Planillas.AsNoTracking().Include(p => p.Detalles).FirstOrDefaultAsync(p => p.Id == id);

    public async Task SavePlanillaAsync(PlanillaHoras planilla)
    {
        var exists = await _db.Planillas.AnyAsync(p => p.Id == planilla.Id);
        if (exists) _db.Planillas.Update(planilla);
        else _db.Planillas.Add(planilla);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<Ausencia>> GetAusenciasAsync(Guid? legajoId, string? legajoNumero, DateTimeOffset? desde, DateTimeOffset? hasta, string? tipo)
    {
        var query = _db.Ausencias.AsNoTracking().AsQueryable();
        if (legajoId.HasValue) query = query.Where(a => a.LegajoId == legajoId.Value);
        if (!string.IsNullOrWhiteSpace(legajoNumero)) query = query.Where(a => a.LegajoNumero == legajoNumero);
        if (desde.HasValue) query = query.Where(a => a.FechaHasta >= desde.Value);
        if (hasta.HasValue) query = query.Where(a => a.FechaDesde <= hasta.Value);
        if (!string.IsNullOrWhiteSpace(tipo)) query = query.Where(a => a.Tipo == tipo);
        return await query.OrderByDescending(a => a.FechaDesde).ToListAsync();
    }

    public async Task SaveAusenciaAsync(Ausencia ausencia)
    {
        var exists = await _db.Ausencias.AnyAsync(a => a.Id == ausencia.Id);
        if (exists) _db.Ausencias.Update(ausencia);
        else _db.Ausencias.Add(ausencia);
        await _db.SaveChangesAsync();
    }
}

using Microsoft.EntityFrameworkCore;
using PersonalService.Domain.Models;

namespace PersonalService.Infrastructure;

public class EfPersonalRepository : IPersonalRepository
{
    private readonly PersonalDbContext _db;

    public EfPersonalRepository(PersonalDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<Legajo>> GetLegajosAsync() =>
        await _db.Legajos.AsNoTracking()
            .Include(l => l.Familiares)
            .Include(l => l.Licencias)
            .Include(l => l.Domicilios)
            .Include(l => l.Documentos)
            .Include(l => l.Solicitudes)
            .ToListAsync();

    public async Task<Legajo?> GetLegajoAsync(Guid id) =>
        await _db.Legajos.AsNoTracking()
            .Include(l => l.Familiares)
            .Include(l => l.Licencias)
            .Include(l => l.Domicilios)
            .Include(l => l.Documentos)
            .Include(l => l.Solicitudes)
            .FirstOrDefaultAsync(l => l.Id == id);

    public async Task<Legajo?> GetLegajoByNumeroAsync(string numero) =>
        await _db.Legajos.AsNoTracking().FirstOrDefaultAsync(l => l.Numero.ToLower() == numero.ToLower());

    public async Task SaveLegajoAsync(Legajo legajo)
    {
        var exists = await _db.Legajos.AnyAsync(l => l.Id == legajo.Id);
        if (exists) _db.Legajos.Update(legajo);
        else _db.Legajos.Add(legajo);
        await _db.SaveChangesAsync();
    }
}

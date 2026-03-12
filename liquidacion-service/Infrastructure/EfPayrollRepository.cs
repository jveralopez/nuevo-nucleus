using LiquidacionService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace LiquidacionService.Infrastructure;

public class EfPayrollRepository : IPayrollRepository
{
    private readonly PayrollDbContext _db;

    public EfPayrollRepository(PayrollDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<PayrollRun>> GetAllAsync() =>
        await _db.Payrolls
            .Include(p => p.Legajos)
            .Include(p => p.Recibos).ThenInclude(r => r.Detalle)
            .AsNoTracking()
            .ToListAsync();

    public async Task<PayrollRun?> GetAsync(Guid id) =>
        await _db.Payrolls
            .Include(p => p.Legajos)
            .Include(p => p.Recibos).ThenInclude(r => r.Detalle)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task SaveAsync(PayrollRun payroll)
    {
        var entry = _db.Entry(payroll);
        if (entry.State == EntityState.Detached)
        {
            var existing = await _db.Payrolls
                .Include(p => p.Legajos)
                .Include(p => p.Recibos).ThenInclude(r => r.Detalle)
                .FirstOrDefaultAsync(p => p.Id == payroll.Id);
            if (existing is null)
            {
                _db.Payrolls.Add(payroll);
            }
            else
            {
                _db.Entry(existing).CurrentValues.SetValues(payroll);

                existing.Legajos.Clear();
                foreach (var legajo in payroll.Legajos)
                {
                    existing.Legajos.Add(legajo);
                    _db.Entry(legajo).State = EntityState.Added;
                }

                existing.Recibos.Clear();
                foreach (var recibo in payroll.Recibos)
                {
                    existing.Recibos.Add(recibo);
                    _db.Entry(recibo).State = EntityState.Added;
                    foreach (var detalle in recibo.Detalle)
                    {
                        _db.Entry(detalle).State = EntityState.Added;
                    }
                }
            }
        }

        foreach (var legajo in payroll.Legajos)
        {
            var legajoEntry = _db.Entry(legajo);
            if (legajoEntry.State == EntityState.Detached || legajoEntry.State == EntityState.Modified)
            {
                legajoEntry.State = EntityState.Added;
            }
        }

        foreach (var recibo in payroll.Recibos)
        {
            var reciboEntry = _db.Entry(recibo);
            if (reciboEntry.State == EntityState.Detached || reciboEntry.State == EntityState.Modified)
            {
                reciboEntry.State = EntityState.Added;
            }

            foreach (var detalle in recibo.Detalle)
            {
                var detalleEntry = _db.Entry(detalle);
                if (detalleEntry.State == EntityState.Detached || detalleEntry.State == EntityState.Modified)
                {
                    detalleEntry.State = EntityState.Added;
                }
            }
        }

        await _db.SaveChangesAsync();
    }
}

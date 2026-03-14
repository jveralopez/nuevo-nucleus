using LiquidacionService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace LiquidacionService.Infrastructure;

public class PayrollDbContext : DbContext
{
    public PayrollDbContext(DbContextOptions<PayrollDbContext> options)
        : base(options)
    {
    }

    public DbSet<PayrollRun> Payrolls => Set<PayrollRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PayrollRun>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.OwnsMany(p => p.Legajos, legajo =>
            {
                legajo.WithOwner().HasForeignKey("PayrollRunId");
                legajo.HasKey(l => l.Id);
                legajo.OwnsMany(l => l.ContribucionesPatronales, contrib =>
                {
                    contrib.WithOwner().HasForeignKey("LegajoEnLoteId");
                    contrib.HasKey("Id");
                });
                legajo.OwnsMany(l => l.Licencias, licencia =>
                {
                    licencia.WithOwner().HasForeignKey("LegajoEnLoteId");
                    licencia.HasKey("Id");
                });
                legajo.OwnsMany(l => l.Embargos, embargo =>
                {
                    embargo.WithOwner().HasForeignKey("LegajoEnLoteId");
                    embargo.HasKey("Id");
                });
            });
            entity.OwnsMany(p => p.Recibos, recibo =>
            {
                recibo.WithOwner().HasForeignKey("PayrollRunId");
                recibo.HasKey(r => r.Id);
                recibo.OwnsMany(r => r.Detalle, detalle =>
                {
                    detalle.WithOwner().HasForeignKey("PayrollReceiptId");
                    detalle.HasKey("Id");
                });
                recibo.OwnsMany(r => r.ContribucionesPatronales, contrib =>
                {
                    contrib.WithOwner().HasForeignKey("PayrollReceiptId");
                    contrib.HasKey("Id");
                });
            });
        });
    }
}

using Microsoft.EntityFrameworkCore;
using TiemposService.Domain.Models;

namespace TiemposService.Infrastructure;

public class TiemposDbContext : DbContext
{
    public TiemposDbContext(DbContextOptions<TiemposDbContext> options)
        : base(options)
    {
    }

    public DbSet<Turno> Turnos => Set<Turno>();
    public DbSet<Horario> Horarios => Set<Horario>();
    public DbSet<Fichada> Fichadas => Set<Fichada>();
    public DbSet<PlanillaHoras> Planillas => Set<PlanillaHoras>();
    public DbSet<PlanillaDetalle> PlanillaDetalles => Set<PlanillaDetalle>();
    public DbSet<Ausencia> Ausencias => Set<Ausencia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Turno>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => t.Codigo).IsUnique();
            entity.Property(t => t.Codigo).IsRequired();
            entity.Property(t => t.Nombre).IsRequired();
        });

        modelBuilder.Entity<Horario>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.Property(h => h.Nombre).IsRequired();
            entity.Property(h => h.DiasSemana).IsRequired();
            entity.HasIndex(h => h.TurnoId);
        });

        modelBuilder.Entity<Fichada>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Tipo).IsRequired();
            entity.Property(f => f.Origen).IsRequired();
            entity.HasIndex(f => f.LegajoId);
            entity.HasIndex(f => f.FechaHora);
        });

        modelBuilder.Entity<PlanillaHoras>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Periodo).IsRequired();
            entity.HasIndex(p => new { p.EmpresaId, p.Periodo }).IsUnique();
            entity.HasMany(p => p.Detalles)
                .WithOne()
                .HasForeignKey(d => d.PlanillaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlanillaDetalle>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.HasIndex(d => d.PlanillaId);
            entity.HasIndex(d => d.LegajoId);
        });

        modelBuilder.Entity<Ausencia>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.LegajoNumero).IsRequired();
            entity.Property(a => a.Tipo).IsRequired();
            entity.Property(a => a.Origen).IsRequired();
            entity.Property(a => a.Estado).IsRequired();
            entity.HasIndex(a => a.LegajoId);
            entity.HasIndex(a => a.LegajoNumero);
            entity.HasIndex(a => a.FechaDesde);
            entity.HasIndex(a => a.FechaHasta);
        });
    }
}

using Microsoft.EntityFrameworkCore;
using OrganizacionService.Domain.Models;

namespace OrganizacionService.Infrastructure;

public class OrganizationDbContext : DbContext
{
    public OrganizationDbContext(DbContextOptions<OrganizationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<UnidadOrganizativa> Unidades => Set<UnidadOrganizativa>();
    public DbSet<Posicion> Posiciones => Set<Posicion>();
    public DbSet<CentroCosto> CentrosCosto => Set<CentroCosto>();
    public DbSet<Sindicato> Sindicatos => Set<Sindicato>();
    public DbSet<Convenio> Convenios => Set<Convenio>();
    public DbSet<OrganigramaVersion> Organigramas => Set<OrganigramaVersion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Empresa>(entity => entity.HasKey(e => e.Id));
        modelBuilder.Entity<UnidadOrganizativa>(entity => entity.HasKey(u => u.Id));
        modelBuilder.Entity<Posicion>(entity => entity.HasKey(p => p.Id));
        modelBuilder.Entity<CentroCosto>(entity => entity.HasKey(c => c.Id));
        modelBuilder.Entity<Sindicato>(entity => entity.HasKey(s => s.Id));
        modelBuilder.Entity<Convenio>(entity => entity.HasKey(c => c.Id));
        modelBuilder.Entity<OrganigramaVersion>(entity =>
        {
            entity.ToTable("Organigramas");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Nombre).IsRequired();
            entity.Property(o => o.UnidadesJson).IsRequired();
            entity.HasIndex(o => new { o.EmpresaId, o.Version }).IsUnique();
        });
    }
}

using ConfiguracionService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfiguracionService.Infrastructure;

public class ConfiguracionDbContext : DbContext
{
    public ConfiguracionDbContext(DbContextOptions<ConfiguracionDbContext> options) : base(options)
    {
    }

    public DbSet<CatalogoItem> Catalogos => Set<CatalogoItem>();
    public DbSet<Parametro> Parametros => Set<Parametro>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CatalogoItem>(entity =>
        {
            entity.ToTable("Catalogos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Tipo).IsRequired();
            entity.Property(e => e.Codigo).IsRequired();
            entity.Property(e => e.Nombre).IsRequired();
            entity.HasIndex(e => new { e.Tipo, e.Codigo }).IsUnique();
        });

        modelBuilder.Entity<Parametro>(entity =>
        {
            entity.ToTable("Parametros");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Clave).IsRequired();
            entity.Property(e => e.Valor).IsRequired();
            entity.HasIndex(e => e.Clave).IsUnique();
        });
    }
}

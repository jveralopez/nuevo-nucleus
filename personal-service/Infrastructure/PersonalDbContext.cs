using Microsoft.EntityFrameworkCore;
using PersonalService.Domain.Models;

namespace PersonalService.Infrastructure;

public class PersonalDbContext : DbContext
{
    public PersonalDbContext(DbContextOptions<PersonalDbContext> options)
        : base(options)
    {
    }

    public DbSet<Legajo> Legajos => Set<Legajo>();
    public DbSet<Familiar> Familiares => Set<Familiar>();
    public DbSet<Licencia> Licencias => Set<Licencia>();
    public DbSet<Domicilio> Domicilios => Set<Domicilio>();
    public DbSet<DocumentoPersonal> Documentos => Set<DocumentoPersonal>();
    public DbSet<SolicitudCambio> Solicitudes => Set<SolicitudCambio>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Legajo>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.HasIndex(l => l.Numero).IsUnique();
            entity.HasMany(l => l.Familiares)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(l => l.Licencias)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(l => l.Domicilios)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(l => l.Documentos)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(l => l.Solicitudes)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Domicilio>(entity =>
        {
            entity.ToTable("Domicilios");
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Tipo).IsRequired();
            entity.Property(d => d.Calle).IsRequired();
            entity.Property(d => d.Numero).IsRequired();
            entity.Property(d => d.Localidad).IsRequired();
            entity.Property(d => d.Provincia).IsRequired();
            entity.Property(d => d.Pais).IsRequired();
        });

        modelBuilder.Entity<DocumentoPersonal>(entity =>
        {
            entity.ToTable("Documentos");
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Tipo).IsRequired();
            entity.Property(d => d.Numero).IsRequired();
        });

        modelBuilder.Entity<SolicitudCambio>(entity =>
        {
            entity.ToTable("SolicitudesCambio");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Tipo).IsRequired();
            entity.Property(s => s.Estado).IsRequired();
            entity.HasIndex(s => s.LegajoId);
            entity.HasIndex(s => s.Estado);
        });
    }
}

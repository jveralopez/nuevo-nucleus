using IntegrationHubService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace IntegrationHubService.Infrastructure;

public class IntegrationHubDbContext : DbContext
{
    public IntegrationHubDbContext(DbContextOptions<IntegrationHubDbContext> options)
        : base(options)
    {
    }

    public DbSet<IntegrationTemplate> Templates => Set<IntegrationTemplate>();
    public DbSet<IntegrationConnection> Connections => Set<IntegrationConnection>();
    public DbSet<IntegrationJob> Jobs => Set<IntegrationJob>();
    public DbSet<IntegrationEvent> Events => Set<IntegrationEvent>();
    public DbSet<IntegrationTrigger> Triggers => Set<IntegrationTrigger>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IntegrationTemplate>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.OwnsOne(t => t.Source);
            entity.OwnsOne(t => t.Transform);
            entity.OwnsOne(t => t.Destination);
        });

        modelBuilder.Entity<IntegrationConnection>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => c.Name).IsUnique();
        });

        modelBuilder.Entity<IntegrationJob>(entity =>
        {
            entity.HasKey(j => j.Id);
        });

        modelBuilder.Entity<IntegrationEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.JobId);
        });

        modelBuilder.Entity<IntegrationTrigger>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => t.EventName);
        });
    }
}

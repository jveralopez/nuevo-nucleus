using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NucleusWFService.Domain.Models;

namespace NucleusWFService.Infrastructure;

public class NucleusWfDbContext : DbContext
{
    public NucleusWfDbContext(DbContextOptions<NucleusWfDbContext> options)
        : base(options)
    {
    }

    public DbSet<WorkflowDefinition> Definitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowInstance> Instances => Set<WorkflowInstance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkflowDefinition>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.OwnsMany(d => d.Transiciones, trans =>
            {
                trans.WithOwner().HasForeignKey("WorkflowDefinitionId");
                trans.HasKey("WorkflowDefinitionId", "From", "To", "Evento");
            });
        });

        var dictConverter = new ValueConverter<Dictionary<string, string>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new());
        var dictComparer = new ValueComparer<Dictionary<string, string>>(
            (l, r) => (l ?? new Dictionary<string, string>()).SequenceEqual(r ?? new Dictionary<string, string>()),
            v => (v ?? new Dictionary<string, string>()).Aggregate(0, (acc, pair) => HashCode.Combine(acc, pair.Key, pair.Value)),
            v => (v ?? new Dictionary<string, string>()).ToDictionary(entry => entry.Key, entry => entry.Value));

        modelBuilder.Entity<WorkflowInstance>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Datos)
                .HasConversion(dictConverter)
                .Metadata.SetValueComparer(dictComparer);
            entity.OwnsMany(i => i.Historial, hist =>
            {
                hist.WithOwner().HasForeignKey("WorkflowInstanceId");
                hist.HasKey("WorkflowInstanceId", "Timestamp", "Evento");
            });
        });
    }
}

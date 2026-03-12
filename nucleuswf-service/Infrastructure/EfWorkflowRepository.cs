using Microsoft.EntityFrameworkCore;
using NucleusWFService.Domain.Models;

namespace NucleusWFService.Infrastructure;

public class EfWorkflowRepository : IWorkflowRepository
{
    private readonly NucleusWfDbContext _db;

    public EfWorkflowRepository(NucleusWfDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<WorkflowDefinition>> GetDefinitionsAsync() =>
        await _db.Definitions.Include(d => d.Transiciones).AsNoTracking().ToListAsync();

    public async Task<WorkflowDefinition?> GetDefinitionAsync(Guid id) =>
        await _db.Definitions.Include(d => d.Transiciones).AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);

    public async Task<WorkflowDefinition?> GetDefinitionByKeyAsync(string key, string version) =>
        await _db.Definitions.Include(d => d.Transiciones).AsNoTracking()
            .FirstOrDefaultAsync(d => d.Key.ToLower() == key.ToLower() && d.Version.ToLower() == version.ToLower());

    public async Task SaveDefinitionAsync(WorkflowDefinition definition)
    {
        var exists = await _db.Definitions.AnyAsync(d => d.Id == definition.Id);
        if (exists) _db.Definitions.Update(definition);
        else _db.Definitions.Add(definition);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<WorkflowInstance>> GetInstancesAsync() =>
        await _db.Instances.Include(i => i.Historial).AsNoTracking().ToListAsync();

    public async Task<WorkflowInstance?> GetInstanceAsync(Guid id) =>
        await _db.Instances.Include(i => i.Historial).AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);

    public async Task SaveInstanceAsync(WorkflowInstance instance)
    {
        var exists = await _db.Instances.AnyAsync(i => i.Id == instance.Id);
        if (exists) _db.Instances.Update(instance);
        else _db.Instances.Add(instance);
        await _db.SaveChangesAsync();
    }
}

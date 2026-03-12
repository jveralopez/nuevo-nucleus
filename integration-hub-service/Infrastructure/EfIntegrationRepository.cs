using IntegrationHubService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace IntegrationHubService.Infrastructure;

public class EfIntegrationRepository : IIntegrationRepository
{
    private readonly IntegrationHubDbContext _db;

    public EfIntegrationRepository(IntegrationHubDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<IntegrationTemplate>> GetTemplatesAsync() =>
        await _db.Templates.AsNoTracking().ToListAsync();

    public async Task<IntegrationTemplate?> GetTemplateAsync(Guid id) =>
        await _db.Templates.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);

    public async Task SaveTemplateAsync(IntegrationTemplate template)
    {
        var exists = await _db.Templates.AnyAsync(t => t.Id == template.Id);
        if (exists) _db.Templates.Update(template);
        else _db.Templates.Add(template);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<IntegrationConnection>> GetConnectionsAsync() =>
        await _db.Connections.AsNoTracking().ToListAsync();

    public async Task<IntegrationConnection?> GetConnectionAsync(Guid id) =>
        await _db.Connections.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IntegrationConnection?> GetConnectionByNameAsync(string name) =>
        await _db.Connections.AsNoTracking().FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());

    public async Task SaveConnectionAsync(IntegrationConnection connection)
    {
        var exists = await _db.Connections.AnyAsync(c => c.Id == connection.Id);
        if (exists) _db.Connections.Update(connection);
        else _db.Connections.Add(connection);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<IntegrationJob>> GetJobsAsync() =>
        await _db.Jobs.AsNoTracking().ToListAsync();

    public async Task<IntegrationJob?> GetJobAsync(Guid id) =>
        await _db.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.Id == id);

    public async Task SaveJobAsync(IntegrationJob job)
    {
        var exists = await _db.Jobs.AnyAsync(j => j.Id == job.Id);
        if (exists) _db.Jobs.Update(job);
        else _db.Jobs.Add(job);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<IntegrationEvent>> GetEventsAsync(Guid? jobId)
    {
        var query = _db.Events.AsNoTracking().AsQueryable();
        if (jobId.HasValue) query = query.Where(e => e.JobId == jobId.Value);
        var items = await query.ToListAsync();
        return items.OrderByDescending(e => e.CreatedAt).ToList();
    }

    public async Task SaveEventAsync(IntegrationEvent integrationEvent)
    {
        _db.Events.Add(integrationEvent);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<IntegrationTrigger>> GetTriggersAsync(string? eventName)
    {
        var query = _db.Triggers.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(eventName))
        {
            query = query.Where(t => t.EventName.ToLower() == eventName.ToLower());
        }
        var items = await query.ToListAsync();
        return items.OrderByDescending(t => t.CreatedAt).ToList();
    }

    public async Task<IntegrationTrigger?> GetTriggerAsync(Guid id)
    {
        return await _db.Triggers.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task SaveTriggerAsync(IntegrationTrigger trigger)
    {
        var exists = await _db.Triggers.AnyAsync(t => t.Id == trigger.Id);
        if (exists) _db.Triggers.Update(trigger);
        else _db.Triggers.Add(trigger);
        await _db.SaveChangesAsync();
    }
}

using IntegrationHubService.Domain.Models;
using IntegrationHubService.Infrastructure;

namespace IntegrationHubService.Tests.Helpers;

public class InMemoryIntegrationRepository : IIntegrationRepository
{
    public List<IntegrationTemplate> Templates { get; } = new();
    public List<IntegrationConnection> Connections { get; } = new();
    public List<IntegrationJob> Jobs { get; } = new();
    public List<IntegrationEvent> Events { get; } = new();
    public List<IntegrationTrigger> Triggers { get; } = new();

    public Task<IReadOnlyCollection<IntegrationTemplate>> GetTemplatesAsync() => Task.FromResult<IReadOnlyCollection<IntegrationTemplate>>(Templates);

    public Task<IntegrationTemplate?> GetTemplateAsync(Guid id) => Task.FromResult(Templates.FirstOrDefault(t => t.Id == id));


    public Task SaveTemplateAsync(IntegrationTemplate template)
    {
        var index = Templates.FindIndex(t => t.Id == template.Id);
        if (index >= 0) Templates[index] = template; else Templates.Add(template);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<IntegrationConnection>> GetConnectionsAsync() => Task.FromResult<IReadOnlyCollection<IntegrationConnection>>(Connections);

    public Task<IntegrationConnection?> GetConnectionAsync(Guid id) => Task.FromResult(Connections.FirstOrDefault(c => c.Id == id));

    public Task<IntegrationConnection?> GetConnectionByNameAsync(string name) =>
        Task.FromResult(Connections.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)));

    public Task SaveConnectionAsync(IntegrationConnection connection)
    {
        var index = Connections.FindIndex(c => c.Id == connection.Id);
        if (index >= 0) Connections[index] = connection; else Connections.Add(connection);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<IntegrationJob>> GetJobsAsync() => Task.FromResult<IReadOnlyCollection<IntegrationJob>>(Jobs);

    public Task<IntegrationJob?> GetJobAsync(Guid id) => Task.FromResult(Jobs.FirstOrDefault(j => j.Id == id));

    public Task SaveJobAsync(IntegrationJob job)
    {
        var index = Jobs.FindIndex(j => j.Id == job.Id);
        if (index >= 0) Jobs[index] = job; else Jobs.Add(job);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<IntegrationEvent>> GetEventsAsync(Guid? jobId)
    {
        var query = Events.AsEnumerable();
        if (jobId.HasValue) query = query.Where(e => e.JobId == jobId.Value);
        return Task.FromResult<IReadOnlyCollection<IntegrationEvent>>(query.ToList());
    }

    public Task SaveEventAsync(IntegrationEvent integrationEvent)
    {
        Events.Add(integrationEvent);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<IntegrationTrigger>> GetTriggersAsync(string? eventName)
    {
        var query = Triggers.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(eventName))
        {
            query = query.Where(t => string.Equals(t.EventName, eventName, StringComparison.OrdinalIgnoreCase));
        }
        return Task.FromResult<IReadOnlyCollection<IntegrationTrigger>>(query.ToList());
    }

    public Task<IntegrationTrigger?> GetTriggerAsync(Guid id) => Task.FromResult(Triggers.FirstOrDefault(t => t.Id == id));

    public Task SaveTriggerAsync(IntegrationTrigger trigger)
    {
        var index = Triggers.FindIndex(t => t.Id == trigger.Id);
        if (index >= 0) Triggers[index] = trigger; else Triggers.Add(trigger);
        return Task.CompletedTask;
    }
}

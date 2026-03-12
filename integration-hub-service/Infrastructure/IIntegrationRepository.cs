using IntegrationHubService.Domain.Models;

namespace IntegrationHubService.Infrastructure;

public interface IIntegrationRepository
{
    Task<IReadOnlyCollection<IntegrationTemplate>> GetTemplatesAsync();
    Task<IntegrationTemplate?> GetTemplateAsync(Guid id);
    Task SaveTemplateAsync(IntegrationTemplate template);

    Task<IReadOnlyCollection<IntegrationConnection>> GetConnectionsAsync();
    Task<IntegrationConnection?> GetConnectionAsync(Guid id);
    Task<IntegrationConnection?> GetConnectionByNameAsync(string name);
    Task SaveConnectionAsync(IntegrationConnection connection);

    Task<IReadOnlyCollection<IntegrationJob>> GetJobsAsync();
    Task<IntegrationJob?> GetJobAsync(Guid id);
    Task SaveJobAsync(IntegrationJob job);

    Task<IReadOnlyCollection<IntegrationEvent>> GetEventsAsync(Guid? jobId);
    Task SaveEventAsync(IntegrationEvent integrationEvent);

    Task<IReadOnlyCollection<IntegrationTrigger>> GetTriggersAsync(string? eventName);
    Task<IntegrationTrigger?> GetTriggerAsync(Guid id);
    Task SaveTriggerAsync(IntegrationTrigger trigger);
}

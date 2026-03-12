using IntegrationHubService.Domain.Models;

namespace IntegrationHubService.Infrastructure;

public class IntegrationStore
{
    public List<IntegrationTemplate> Templates { get; set; } = new();
    public List<IntegrationConnection> Connections { get; set; } = new();
    public List<IntegrationJob> Jobs { get; set; } = new();
    public List<IntegrationEvent> Events { get; set; } = new();
    public List<IntegrationTrigger> Triggers { get; set; } = new();
}

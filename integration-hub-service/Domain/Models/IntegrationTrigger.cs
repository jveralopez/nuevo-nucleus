namespace IntegrationHubService.Domain.Models;

public class IntegrationTrigger
{
    public Guid Id { get; set; }
    public string EventName { get; set; } = string.Empty;
    public Guid TemplateId { get; set; }
    public bool Enabled { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

namespace IntegrationHubService.Domain.Models;

public class IntegrationTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public TemplateSource Source { get; set; } = new();
    public TemplateTransform Transform { get; set; } = new();
    public TemplateDestination Destination { get; set; } = new();
    public string Estado { get; set; } = "Draft";
    public DateTimeOffset? LastRunAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

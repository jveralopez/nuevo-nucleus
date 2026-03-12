namespace IntegrationHubService.Domain.Models;

public class IntegrationJob
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public string Trigger { get; set; } = string.Empty;
    public string? Periodo { get; set; }
    public string? ArchivoGenerado { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
    public DateTimeOffset? LastRetryAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

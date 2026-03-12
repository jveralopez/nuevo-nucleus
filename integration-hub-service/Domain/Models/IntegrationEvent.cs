namespace IntegrationHubService.Domain.Models;

public class IntegrationEvent
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Detalle { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

namespace IntegrationHubService.Domain.Requests;

public record StartJobRequest(Guid TemplateId, string? Periodo, string Trigger);

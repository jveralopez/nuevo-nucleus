namespace IntegrationHubService.Domain.Requests;

public record CreateTriggerRequest(string EventName, Guid TemplateId, bool Enabled);

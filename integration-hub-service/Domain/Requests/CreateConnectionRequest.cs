namespace IntegrationHubService.Domain.Requests;

public record CreateConnectionRequest(string Name, string Type, string Host, string Username, string SecretId);

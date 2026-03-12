namespace IntegrationHubService.Services;

public interface ISecretProvider
{
    Task<string> GetSecretAsync(string secretId);
}

using IntegrationHubService.Services;

namespace IntegrationHubService.Tests.Helpers;

public class FakeSecretProvider : ISecretProvider
{
    public Task<string> GetSecretAsync(string secretId) => Task.FromResult("test");
}

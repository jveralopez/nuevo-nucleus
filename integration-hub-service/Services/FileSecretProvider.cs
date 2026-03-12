using System.Text.Json;

namespace IntegrationHubService.Services;

public class FileSecretProvider : ISecretProvider
{
    private readonly SecretsOptions _options;

    public FileSecretProvider(SecretsOptions options)
    {
        _options = options;
    }

    public async Task<string> GetSecretAsync(string secretId)
    {
        if (secretId.StartsWith("plain:", StringComparison.OrdinalIgnoreCase))
        {
            return secretId.Substring("plain:".Length);
        }

        var filePath = Path.IsPathRooted(_options.SecretsFile)
            ? _options.SecretsFile
            : Path.Combine(Directory.GetCurrentDirectory(), _options.SecretsFile);

        if (!File.Exists(filePath))
        {
            throw new InvalidOperationException("Secrets file no encontrado");
        }

        var json = await File.ReadAllTextAsync(filePath);
        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
        if (!data.TryGetValue(secretId, out var value) || string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("SecretId inexistente en vault");
        }

        return value;
    }
}

using System.Net.Http.Headers;
using System.Text.Json;

namespace LiquidacionService.Services;

public class WorkflowVacacionesClient
{
    private readonly HttpClient _client;
    private readonly WorkflowOptions _options;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public WorkflowVacacionesClient(HttpClient client, WorkflowOptions options)
    {
        _client = client;
        _options = options;
    }

    public async Task<IReadOnlyCollection<WorkflowInstanceDto>> GetVacacionesAprobadasAsync()
    {
        var baseUrl = string.IsNullOrWhiteSpace(_options.BaseUrl) ? "http://localhost:5051" : _options.BaseUrl.TrimEnd('/');
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/instances");
        if (!string.IsNullOrWhiteSpace(_options.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);
        }

        using var response = await _client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<WorkflowInstanceDto>();
        }

        await using var stream = await response.Content.ReadAsStreamAsync();
        var instances = await JsonSerializer.DeserializeAsync<List<WorkflowInstanceDto>>(stream, _jsonOptions);
        return instances
            ?.Where(i => string.Equals(i.Key, "vacaciones", StringComparison.OrdinalIgnoreCase)
                         && string.Equals(i.Estado, "aprobado", StringComparison.OrdinalIgnoreCase))
            .ToList() ?? new List<WorkflowInstanceDto>();
    }
}

public class WorkflowOptions
{
    public string BaseUrl { get; set; } = "http://localhost:5051";
    public string? AccessToken { get; set; }
}

public class WorkflowInstanceDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public Dictionary<string, string> Datos { get; set; } = new();
}

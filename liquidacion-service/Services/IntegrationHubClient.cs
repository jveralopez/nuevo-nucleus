using System.Net.Http.Headers;

namespace LiquidacionService.Services;

public class IntegrationHubClient
{
    private readonly HttpClient _httpClient;
    private readonly IntegrationHubOptions _options;
    private readonly ILogger<IntegrationHubClient> _logger;

    public IntegrationHubClient(HttpClient httpClient, IntegrationHubOptions options, ILogger<IntegrationHubClient> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public async Task TriggerJobAsync(string periodo)
    {
        if (string.IsNullOrWhiteSpace(_options.TemplateId))
        {
            _logger.LogWarning("IntegrationHub TemplateId no configurado, se omite trigger.");
            return;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.BaseUrl.TrimEnd('/')}/integraciones/jobs")
        {
            Content = JsonContent.Create(new
            {
                templateId = _options.TemplateId,
                periodo,
                trigger = "liquidacion"
            })
        };

        if (!string.IsNullOrWhiteSpace(_options.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);
        }

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("IntegrationHub fallo: {StatusCode} {Body}", response.StatusCode, body);
        }
    }
}

namespace LiquidacionService.Services;

public class IntegrationHubOptions
{
    public string BaseUrl { get; set; } = "http://localhost:5050";
    public string TemplateId { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
}

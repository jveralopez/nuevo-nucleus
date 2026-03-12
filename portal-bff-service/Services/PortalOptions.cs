namespace PortalBffService.Services;

public class PortalOptions
{
    public string LiquidacionApi { get; set; } = "http://localhost:5188";
    public string WfApi { get; set; } = "http://localhost:5051";
    public string OrganizacionApi { get; set; } = "http://localhost:5100";
    public string PersonalApi { get; set; } = "http://localhost:5200";
    public string IntegrationHubApi { get; set; } = "http://localhost:5050";
    public string ConfiguracionApi { get; set; } = "http://localhost:5300";
}

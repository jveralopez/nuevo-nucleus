using IntegrationHubService.Domain.Models;

namespace IntegrationHubService.Domain.Requests;

public record CreateTemplateRequest(
    string Name,
    string Version,
    string Schedule,
    TemplateSource Source,
    TemplateTransform Transform,
    TemplateDestination Destination);

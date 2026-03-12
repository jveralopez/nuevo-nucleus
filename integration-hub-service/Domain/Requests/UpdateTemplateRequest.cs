using IntegrationHubService.Domain.Models;

namespace IntegrationHubService.Domain.Requests;

public record UpdateTemplateRequest(
    string Schedule,
    TemplateSource Source,
    TemplateTransform Transform,
    TemplateDestination Destination,
    string? Estado);

namespace NucleusWFService.Domain.Requests;

public record StartInstanceRequest(
    string Key,
    string Version,
    Dictionary<string, string>? Datos,
    string? CorrelationId = null,
    string? IdempotencyKey = null);

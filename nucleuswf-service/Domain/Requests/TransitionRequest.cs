namespace NucleusWFService.Domain.Requests;

public record TransitionRequest(
    string Evento,
    string? Actor,
    Dictionary<string, string>? Datos,
    string? CorrelationId = null,
    string? IdempotencyKey = null);

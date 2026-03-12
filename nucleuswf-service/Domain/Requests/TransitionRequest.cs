namespace NucleusWFService.Domain.Requests;

public record TransitionRequest(string Evento, string? Actor, Dictionary<string, string>? Datos);

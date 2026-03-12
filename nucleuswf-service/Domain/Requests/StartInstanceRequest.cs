namespace NucleusWFService.Domain.Requests;

public record StartInstanceRequest(string Key, string Version, Dictionary<string, string>? Datos);

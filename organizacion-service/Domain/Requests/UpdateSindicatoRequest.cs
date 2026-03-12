namespace OrganizacionService.Domain.Requests;

public record UpdateSindicatoRequest(string Nombre, string? Codigo, string? Jurisdiccion, string Estado);

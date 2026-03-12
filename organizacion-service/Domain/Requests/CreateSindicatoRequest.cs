namespace OrganizacionService.Domain.Requests;

public record CreateSindicatoRequest(string Nombre, string? Codigo, string? Jurisdiccion);

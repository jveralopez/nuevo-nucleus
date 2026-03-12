namespace OrganizacionService.Domain.Requests;

public record UpdatePosicionRequest(string Nombre, string? Nivel, string? Perfil, string? Estado);

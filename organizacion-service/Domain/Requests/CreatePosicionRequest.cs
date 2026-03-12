namespace OrganizacionService.Domain.Requests;

public record CreatePosicionRequest(Guid UnidadId, string Nombre, string? Nivel, string? Perfil, string? Estado);

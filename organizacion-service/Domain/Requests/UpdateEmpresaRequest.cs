namespace OrganizacionService.Domain.Requests;

public record UpdateEmpresaRequest(string Nombre, string Pais, string Moneda, string? Estado);

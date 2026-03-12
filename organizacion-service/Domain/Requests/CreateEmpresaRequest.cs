namespace OrganizacionService.Domain.Requests;

public record CreateEmpresaRequest(string Nombre, string Pais, string Moneda, string? Estado);

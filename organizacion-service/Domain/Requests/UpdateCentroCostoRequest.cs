namespace OrganizacionService.Domain.Requests;

public record UpdateCentroCostoRequest(string Codigo, string Descripcion, string Moneda, string? Estado);

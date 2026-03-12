namespace OrganizacionService.Domain.Requests;

public record CreateCentroCostoRequest(Guid EmpresaId, string Codigo, string Descripcion, string Moneda, string? Estado);

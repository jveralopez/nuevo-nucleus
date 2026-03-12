namespace OrganizacionService.Domain.Requests;

public record CreateUnidadRequest(Guid EmpresaId, string Nombre, string Tipo, Guid? PadreId, Guid? CentroCostoId, string? Estado);

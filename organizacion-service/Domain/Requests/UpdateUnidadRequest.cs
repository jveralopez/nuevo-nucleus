namespace OrganizacionService.Domain.Requests;

public record UpdateUnidadRequest(string Nombre, string Tipo, Guid? PadreId, Guid? CentroCostoId, string? Estado);

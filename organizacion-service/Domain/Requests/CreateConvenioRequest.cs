namespace OrganizacionService.Domain.Requests;

public record CreateConvenioRequest(Guid SindicatoId, string Nombre, string? Numero, DateTimeOffset? VigenciaDesde, DateTimeOffset? VigenciaHasta);

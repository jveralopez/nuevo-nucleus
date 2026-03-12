namespace OrganizacionService.Domain.Requests;

public record UpdateConvenioRequest(Guid SindicatoId, string Nombre, string? Numero, DateTimeOffset? VigenciaDesde, DateTimeOffset? VigenciaHasta, string Estado);

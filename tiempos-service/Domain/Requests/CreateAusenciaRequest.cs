namespace TiemposService.Domain.Requests;

public record CreateAusenciaRequest(
    Guid? LegajoId,
    string LegajoNumero,
    string Tipo,
    DateTimeOffset FechaDesde,
    DateTimeOffset FechaHasta,
    string? Origen,
    string? Observaciones
);

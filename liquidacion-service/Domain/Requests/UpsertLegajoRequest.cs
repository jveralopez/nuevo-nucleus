namespace LiquidacionService.Domain.Requests;

public record UpsertLegajoRequest
(
    string Numero,
    string Nombre,
    string Cuil,
    string? Convenio,
    string? Categoria,
    decimal Basico,
    decimal Antiguedad,
    decimal Adicionales,
    decimal Presentismo,
    decimal HorasExtra,
    decimal Premios,
    decimal Descuentos,
    decimal NoRemunerativo,
    decimal BonosNoRemunerativos,
    bool AplicaGanancias,
    bool OmitirGanancias,
    bool ConyugeACargo,
    int CantHijos,
    int CantOtrosFamiliares,
    decimal DeduccionesAdicionales,
    int VacacionesDias,
    IReadOnlyCollection<LicenciaRequest>? Licencias,
    IReadOnlyCollection<EmbargoRequest>? Embargos,
    IReadOnlyCollection<EmployerContributionRequest>? ContribucionesPatronales
);

public record LicenciaRequest(string Tipo, int Dias, bool ConGoce);

public record EmbargoRequest(
    string Tipo,
    decimal? Porcentaje,
    decimal? MontoFijo,
    decimal? MontoTotal,
    decimal? MontoPendiente,
    string BaseCalculo,
    bool Activo
);

public record EmployerContributionRequest(
    string Concepto,
    decimal Importe,
    string? Grupo
);

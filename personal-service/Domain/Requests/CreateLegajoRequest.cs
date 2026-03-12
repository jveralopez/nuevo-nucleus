namespace PersonalService.Domain.Requests;

public record CreateLegajoRequest(
    string Numero,
    string Nombre,
    string Apellido,
    string Documento,
    string Cuil,
    string? Email,
    string? Estado,
    string? EstadoCivil,
    DateTime? FechaIngreso,
    string? Convenio,
    string? Categoria,
    string? ObraSocial,
    string? Sindicato,
    string? TipoPersonal,
    string? Ubicacion,
    IReadOnlyCollection<FamiliarRequest>? Familiares,
    IReadOnlyCollection<LicenciaRequest>? Licencias,
    IReadOnlyCollection<DomicilioRequest>? Domicilios,
    IReadOnlyCollection<DocumentoPersonalRequest>? Documentos
);

public record FamiliarRequest(
    string Nombre,
    string Apellido,
    string Documento,
    string Tipo,
    DateTime? FechaNacimiento,
    bool Vive,
    bool Discapacidad,
    bool ACargo,
    bool ACargoObraSocial,
    bool AplicaGanancias
);

public record LicenciaRequest(
    string Tipo,
    string? CodigoSIJP,
    DateTime FechaInicio,
    DateTime FechaFin,
    bool ConGoce,
    bool CuentaVacaciones
);

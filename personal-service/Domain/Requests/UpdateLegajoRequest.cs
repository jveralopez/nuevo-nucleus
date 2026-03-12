namespace PersonalService.Domain.Requests;

public record UpdateLegajoRequest(
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

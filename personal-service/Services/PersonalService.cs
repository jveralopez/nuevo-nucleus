using PersonalService.Domain.Models;
using PersonalService.Domain.Requests;
using PersonalService.Infrastructure;

namespace PersonalService.Services;

public class PersonalService
{
    private readonly IPersonalRepository _repository;
    private readonly ILogger<PersonalService> _logger;

    public PersonalService(IPersonalRepository repository, ILogger<PersonalService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public Task<IReadOnlyCollection<Legajo>> GetLegajosAsync() => _repository.GetLegajosAsync();

    public Task<Legajo?> GetLegajoAsync(Guid id) => _repository.GetLegajoAsync(id);

    public async Task<Legajo> CreateLegajoAsync(CreateLegajoRequest request)
    {
        ValidateRequired(request.Numero, nameof(request.Numero));
        ValidateRequired(request.Nombre, nameof(request.Nombre));
        ValidateRequired(request.Apellido, nameof(request.Apellido));
        ValidateRequired(request.Documento, nameof(request.Documento));
        ValidateRequired(request.Cuil, nameof(request.Cuil));

        var existente = await _repository.GetLegajoByNumeroAsync(request.Numero.Trim());
        if (existente is not null)
        {
            _logger.LogWarning("Legajo {Numero} ya existe", request.Numero);
            throw new InvalidOperationException("El número de legajo ya existe");
        }

        var now = DateTimeOffset.UtcNow;
        var legajo = new Legajo
        {
            Id = Guid.NewGuid(),
            Numero = request.Numero.Trim(),
            Nombre = request.Nombre.Trim(),
            Apellido = request.Apellido.Trim(),
            Documento = request.Documento.Trim(),
            Cuil = request.Cuil.Trim(),
            Email = request.Email?.Trim(),
            EstadoCivil = request.EstadoCivil?.Trim(),
            FechaIngreso = request.FechaIngreso,
            Convenio = request.Convenio?.Trim(),
            Categoria = request.Categoria?.Trim(),
            ObraSocial = request.ObraSocial?.Trim(),
            Sindicato = request.Sindicato?.Trim(),
            TipoPersonal = request.TipoPersonal?.Trim(),
            Ubicacion = request.Ubicacion?.Trim(),
            Estado = NormalizeEstado(request.Estado),
            CreatedAt = now,
            UpdatedAt = now,
            Familiares = request.Familiares?.Select(MapFamiliar).ToList() ?? new List<Familiar>(),
            Licencias = request.Licencias?.Select(MapLicencia).ToList() ?? new List<Licencia>(),
            Domicilios = request.Domicilios?.Select(MapDomicilio).ToList() ?? new List<Domicilio>(),
            Documentos = request.Documentos?.Select(MapDocumento).ToList() ?? new List<DocumentoPersonal>()
        };

        await _repository.SaveLegajoAsync(legajo);
        return legajo;
    }

    public async Task<Legajo?> UpdateLegajoAsync(Guid id, UpdateLegajoRequest request)
    {
        ValidateRequired(request.Numero, nameof(request.Numero));
        ValidateRequired(request.Nombre, nameof(request.Nombre));
        ValidateRequired(request.Apellido, nameof(request.Apellido));
        ValidateRequired(request.Documento, nameof(request.Documento));
        ValidateRequired(request.Cuil, nameof(request.Cuil));

        var legajo = await _repository.GetLegajoAsync(id);
        if (legajo is null) return null;

        var existingByNumero = await _repository.GetLegajoByNumeroAsync(request.Numero.Trim());
        if (existingByNumero is not null && existingByNumero.Id != id)
        {
            _logger.LogWarning("Legajo {Numero} ya existe", request.Numero);
            throw new InvalidOperationException("El número de legajo ya existe");
        }

        legajo.Numero = request.Numero.Trim();
        legajo.Nombre = request.Nombre.Trim();
        legajo.Apellido = request.Apellido.Trim();
        legajo.Documento = request.Documento.Trim();
        legajo.Cuil = request.Cuil.Trim();
        legajo.Email = request.Email?.Trim();
        legajo.EstadoCivil = request.EstadoCivil?.Trim();
        legajo.FechaIngreso = request.FechaIngreso;
        legajo.Convenio = request.Convenio?.Trim();
        legajo.Categoria = request.Categoria?.Trim();
        legajo.ObraSocial = request.ObraSocial?.Trim();
        legajo.Sindicato = request.Sindicato?.Trim();
        legajo.TipoPersonal = request.TipoPersonal?.Trim();
        legajo.Ubicacion = request.Ubicacion?.Trim();
        legajo.Estado = NormalizeEstado(request.Estado);
        legajo.UpdatedAt = DateTimeOffset.UtcNow;
        legajo.Familiares = request.Familiares?.Select(MapFamiliar).ToList() ?? new List<Familiar>();
        legajo.Licencias = request.Licencias?.Select(MapLicencia).ToList() ?? new List<Licencia>();
        legajo.Domicilios = request.Domicilios?.Select(MapDomicilio).ToList() ?? new List<Domicilio>();
        legajo.Documentos = request.Documentos?.Select(MapDocumento).ToList() ?? new List<DocumentoPersonal>();

        await _repository.SaveLegajoAsync(legajo);
        return legajo;
    }

    public async Task<Legajo?> DeactivateLegajoAsync(Guid id)
    {
        var legajo = await _repository.GetLegajoAsync(id);
        if (legajo is null) return null;

        legajo.Estado = "Inactivo";
        legajo.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.SaveLegajoAsync(legajo);
        return legajo;
    }

    public async Task<IReadOnlyCollection<Familiar>?> UpdateFamiliaresAsync(Guid id, UpdateFamiliaresRequest request)
    {
        var legajo = await _repository.GetLegajoAsync(id);
        if (legajo is null) return null;

        legajo.Familiares = request.Familiares.Select(MapFamiliar).ToList();
        legajo.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.SaveLegajoAsync(legajo);
        return legajo.Familiares;
    }

    public async Task<IReadOnlyCollection<Licencia>?> UpdateLicenciasAsync(Guid id, UpdateLicenciasRequest request)
    {
        var legajo = await _repository.GetLegajoAsync(id);
        if (legajo is null) return null;

        legajo.Licencias = request.Licencias.Select(MapLicencia).ToList();
        legajo.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.SaveLegajoAsync(legajo);
        return legajo.Licencias;
    }

    public async Task<IReadOnlyCollection<SolicitudCambio>> GetSolicitudesAsync(Guid? legajoId, string? estado)
    {
        var legajos = await _repository.GetLegajosAsync();
        var allSolicitudes = legajos.SelectMany(l => l.Solicitudes).ToList();
        var filtered = allSolicitudes.AsQueryable();
        if (legajoId.HasValue) filtered = filtered.Where(s => s.LegajoId == legajoId.Value);
        if (!string.IsNullOrWhiteSpace(estado))
        {
            filtered = filtered.Where(s => string.Equals(s.Estado, estado, StringComparison.OrdinalIgnoreCase));
        }
        return filtered.OrderByDescending(s => s.CreatedAt).ToList();
    }

    public async Task<SolicitudCambio?> GetSolicitudAsync(Guid id)
    {
        var legajos = await _repository.GetLegajosAsync();
        return legajos.SelectMany(l => l.Solicitudes).FirstOrDefault(s => s.Id == id);
    }

    public async Task<SolicitudCambio> CreateSolicitudAsync(CreateSolicitudCambioRequest request)
    {
        ValidateRequired(request.Tipo, nameof(request.Tipo));
        var legajo = await _repository.GetLegajoAsync(request.LegajoId);
        if (legajo is null) throw new InvalidOperationException("Legajo no encontrado");

        var solicitud = new SolicitudCambio
        {
            Id = Guid.NewGuid(),
            LegajoId = request.LegajoId,
            Tipo = request.Tipo.Trim(),
            Detalle = request.Detalle?.Trim(),
            DatosJson = request.DatosJson,
            Estado = "PendAprob",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        legajo.Solicitudes.Add(solicitud);
        legajo.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.SaveLegajoAsync(legajo);
        return solicitud;
    }

    public async Task<SolicitudCambio?> UpdateSolicitudEstadoAsync(Guid id, string estado, UpdateSolicitudEstadoRequest request)
    {
        var legajos = await _repository.GetLegajosAsync();
        var legajo = legajos.FirstOrDefault(l => l.Solicitudes.Any(s => s.Id == id));
        if (legajo is null) return null;

        var solicitud = legajo.Solicitudes.First(s => s.Id == id);
        solicitud.Estado = estado;
        solicitud.Observaciones = request.Observaciones?.Trim();
        solicitud.UpdatedAt = DateTimeOffset.UtcNow;
        legajo.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.SaveLegajoAsync(legajo);
        return solicitud;
    }

    public async Task<IReadOnlyCollection<Domicilio>?> UpdateDomiciliosAsync(Guid id, UpdateDomiciliosRequest request)
    {
        var legajo = await _repository.GetLegajoAsync(id);
        if (legajo is null) return null;

        legajo.Domicilios = request.Domicilios.Select(MapDomicilio).ToList();
        legajo.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.SaveLegajoAsync(legajo);
        return legajo.Domicilios;
    }

    public async Task<IReadOnlyCollection<DocumentoPersonal>?> UpdateDocumentosAsync(Guid id, UpdateDocumentosRequest request)
    {
        var legajo = await _repository.GetLegajoAsync(id);
        if (legajo is null) return null;

        legajo.Documentos = request.Documentos.Select(MapDocumento).ToList();
        legajo.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.SaveLegajoAsync(legajo);
        return legajo.Documentos;
    }

    private static void ValidateRequired(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{field} es requerido");
        }
    }

    private static string NormalizeEstado(string? estado)
    {
        var normalized = estado?.Trim();
        return string.IsNullOrEmpty(normalized) ? "Activo" : normalized;
    }

    private static Familiar MapFamiliar(FamiliarRequest request) => new()
    {
        Nombre = request.Nombre.Trim(),
        Apellido = request.Apellido.Trim(),
        Documento = request.Documento.Trim(),
        Tipo = request.Tipo.Trim(),
        FechaNacimiento = request.FechaNacimiento,
        Vive = request.Vive,
        Discapacidad = request.Discapacidad,
        ACargo = request.ACargo,
        ACargoObraSocial = request.ACargoObraSocial,
        AplicaGanancias = request.AplicaGanancias
    };

    private static Licencia MapLicencia(LicenciaRequest request) => new()
    {
        Tipo = request.Tipo.Trim(),
        CodigoSIJP = request.CodigoSIJP?.Trim(),
        FechaInicio = request.FechaInicio,
        FechaFin = request.FechaFin,
        ConGoce = request.ConGoce,
        CuentaVacaciones = request.CuentaVacaciones
    };

    private static Domicilio MapDomicilio(DomicilioRequest request) => new()
    {
        Tipo = request.Tipo.Trim(),
        Calle = request.Calle.Trim(),
        Numero = request.Numero.Trim(),
        Piso = request.Piso?.Trim(),
        Depto = request.Depto?.Trim(),
        Localidad = request.Localidad.Trim(),
        Provincia = request.Provincia.Trim(),
        Pais = request.Pais.Trim(),
        CodigoPostal = request.CodigoPostal?.Trim(),
        Observaciones = request.Observaciones?.Trim()
    };

    private static DocumentoPersonal MapDocumento(DocumentoPersonalRequest request) => new()
    {
        Tipo = request.Tipo.Trim(),
        Numero = request.Numero.Trim(),
        FechaEmision = request.FechaEmision,
        FechaVencimiento = request.FechaVencimiento,
        Observaciones = request.Observaciones?.Trim()
    };

}

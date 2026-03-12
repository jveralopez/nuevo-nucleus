using OrganizacionService.Domain.Models;
using OrganizacionService.Domain.Requests;
using OrganizacionService.Infrastructure;

namespace OrganizacionService.Services;

public class OrganizationService
{
    private readonly IOrganizationRepository _repository;
    private readonly ILogger<OrganizationService> _logger;

    public OrganizationService(IOrganizationRepository repository, ILogger<OrganizationService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public Task<IReadOnlyCollection<Empresa>> GetEmpresasAsync() => _repository.GetEmpresasAsync();

    public Task<Empresa?> GetEmpresaAsync(Guid id) => _repository.GetEmpresaAsync(id);

    public async Task<Empresa> CreateEmpresaAsync(CreateEmpresaRequest request)
    {
        ValidateRequired(request.Nombre, nameof(request.Nombre));
        ValidateRequired(request.Pais, nameof(request.Pais));
        ValidateRequired(request.Moneda, nameof(request.Moneda));

        var now = DateTimeOffset.UtcNow;
        var empresa = new Empresa
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre.Trim(),
            Pais = request.Pais.Trim(),
            Moneda = request.Moneda.Trim(),
            Estado = NormalizeEstado(request.Estado),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repository.SaveEmpresaAsync(empresa);
        return empresa;
    }

    public async Task<Empresa?> UpdateEmpresaAsync(Guid id, UpdateEmpresaRequest request)
    {
        ValidateRequired(request.Nombre, nameof(request.Nombre));
        ValidateRequired(request.Pais, nameof(request.Pais));
        ValidateRequired(request.Moneda, nameof(request.Moneda));

        var empresa = await _repository.GetEmpresaAsync(id);
        if (empresa is null) return null;

        empresa.Nombre = request.Nombre.Trim();
        empresa.Pais = request.Pais.Trim();
        empresa.Moneda = request.Moneda.Trim();
        empresa.Estado = NormalizeEstado(request.Estado);
        empresa.UpdatedAt = DateTimeOffset.UtcNow;

        await _repository.SaveEmpresaAsync(empresa);
        return empresa;
    }

    public async Task<Empresa?> DeactivateEmpresaAsync(Guid id)
    {
        var empresa = await _repository.GetEmpresaAsync(id);
        if (empresa is null) return null;

        empresa.Estado = "Inactiva";
        empresa.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.SaveEmpresaAsync(empresa);
        return empresa;
    }

    public Task<IReadOnlyCollection<UnidadOrganizativa>> GetUnidadesAsync() => _repository.GetUnidadesAsync();

    public Task<UnidadOrganizativa?> GetUnidadAsync(Guid id) => _repository.GetUnidadAsync(id);

    public async Task<UnidadOrganizativa> CreateUnidadAsync(CreateUnidadRequest request)
    {
        ValidateRequired(request.Nombre, nameof(request.Nombre));
        ValidateRequired(request.Tipo, nameof(request.Tipo));
        await RequireEmpresaAsync(request.EmpresaId);

        if (request.PadreId.HasValue)
        {
            await RequireUnidadAsync(request.PadreId.Value);
        }

        var now = DateTimeOffset.UtcNow;
        var unidad = new UnidadOrganizativa
        {
            Id = Guid.NewGuid(),
            EmpresaId = request.EmpresaId,
            Nombre = request.Nombre.Trim(),
            Tipo = request.Tipo.Trim(),
            PadreId = request.PadreId,
            CentroCostoId = request.CentroCostoId,
            Estado = NormalizeEstado(request.Estado),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repository.SaveUnidadAsync(unidad);
        return unidad;
    }

    public async Task<UnidadOrganizativa?> UpdateUnidadAsync(Guid id, UpdateUnidadRequest request)
    {
        ValidateRequired(request.Nombre, nameof(request.Nombre));
        ValidateRequired(request.Tipo, nameof(request.Tipo));

        var unidad = await _repository.GetUnidadAsync(id);
        if (unidad is null) return null;

        if (request.PadreId.HasValue && request.PadreId.Value != unidad.Id)
        {
            await RequireUnidadAsync(request.PadreId.Value);
        }

        unidad.Nombre = request.Nombre.Trim();
        unidad.Tipo = request.Tipo.Trim();
        unidad.PadreId = request.PadreId;
        unidad.CentroCostoId = request.CentroCostoId;
        unidad.Estado = NormalizeEstado(request.Estado);
        unidad.UpdatedAt = DateTimeOffset.UtcNow;

        await _repository.SaveUnidadAsync(unidad);
        return unidad;
    }

    public async Task<UnidadOrganizativa?> DeactivateUnidadAsync(Guid id)
    {
        var unidad = await _repository.GetUnidadAsync(id);
        if (unidad is null) return null;

        unidad.Estado = "Inactiva";
        unidad.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.SaveUnidadAsync(unidad);
        return unidad;
    }

    public async Task<IReadOnlyCollection<UnidadNode>> GetUnidadesTreeAsync(Guid? empresaId)
    {
        var unidades = await _repository.GetUnidadesAsync();
        var filtered = empresaId.HasValue
            ? unidades.Where(u => u.EmpresaId == empresaId.Value).ToList()
            : unidades.ToList();

        var lookup = filtered.ToDictionary(u => u.Id, u => new UnidadNode
        {
            Id = u.Id,
            EmpresaId = u.EmpresaId,
            Nombre = u.Nombre,
            Tipo = u.Tipo,
            PadreId = u.PadreId,
            CentroCostoId = u.CentroCostoId,
            Estado = u.Estado
        });

        var roots = new List<UnidadNode>();
        foreach (var node in lookup.Values)
        {
            if (node.PadreId.HasValue && lookup.TryGetValue(node.PadreId.Value, out var parent))
            {
                parent.Hijos.Add(node);
            }
            else
            {
                roots.Add(node);
            }
        }

        return roots;
    }

    public Task<IReadOnlyCollection<Posicion>> GetPosicionesAsync() => _repository.GetPosicionesAsync();

    public Task<Posicion?> GetPosicionAsync(Guid id) => _repository.GetPosicionAsync(id);

    public async Task<Posicion> CreatePosicionAsync(CreatePosicionRequest request)
    {
        ValidateRequired(request.Nombre, nameof(request.Nombre));
        await RequireUnidadAsync(request.UnidadId);

        var now = DateTimeOffset.UtcNow;
        var posicion = new Posicion
        {
            Id = Guid.NewGuid(),
            UnidadId = request.UnidadId,
            Nombre = request.Nombre.Trim(),
            Nivel = request.Nivel?.Trim(),
            Perfil = request.Perfil?.Trim(),
            Estado = NormalizeEstado(request.Estado),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repository.SavePosicionAsync(posicion);
        return posicion;
    }

    public async Task<Posicion?> UpdatePosicionAsync(Guid id, UpdatePosicionRequest request)
    {
        ValidateRequired(request.Nombre, nameof(request.Nombre));

        var posicion = await _repository.GetPosicionAsync(id);
        if (posicion is null) return null;

        posicion.Nombre = request.Nombre.Trim();
        posicion.Nivel = request.Nivel?.Trim();
        posicion.Perfil = request.Perfil?.Trim();
        posicion.Estado = NormalizeEstado(request.Estado);
        posicion.UpdatedAt = DateTimeOffset.UtcNow;

        await _repository.SavePosicionAsync(posicion);
        return posicion;
    }

    public async Task<Posicion?> AssignLegajoAsync(Guid id, AssignLegajoRequest request)
    {
        var posicion = await _repository.GetPosicionAsync(id);
        if (posicion is null) return null;

        if (!posicion.LegajoIds.Contains(request.LegajoId))
        {
            posicion.LegajoIds.Add(request.LegajoId);
            posicion.UpdatedAt = DateTimeOffset.UtcNow;
            await _repository.SavePosicionAsync(posicion);
        }

        return posicion;
    }

    public async Task<Posicion?> UnassignLegajoAsync(Guid id, AssignLegajoRequest request)
    {
        var posicion = await _repository.GetPosicionAsync(id);
        if (posicion is null) return null;

        if (posicion.LegajoIds.Remove(request.LegajoId))
        {
            posicion.UpdatedAt = DateTimeOffset.UtcNow;
            await _repository.SavePosicionAsync(posicion);
        }

        return posicion;
    }

    public async Task<Posicion?> DeactivatePosicionAsync(Guid id)
    {
        var posicion = await _repository.GetPosicionAsync(id);
        if (posicion is null) return null;

        posicion.Estado = "Inactiva";
        posicion.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.SavePosicionAsync(posicion);
        return posicion;
    }

    public Task<IReadOnlyCollection<CentroCosto>> GetCentrosCostoAsync() => _repository.GetCentrosCostoAsync();

    public Task<CentroCosto?> GetCentroCostoAsync(Guid id) => _repository.GetCentroCostoAsync(id);

    public async Task<CentroCosto> CreateCentroCostoAsync(CreateCentroCostoRequest request)
    {
        ValidateRequired(request.Codigo, nameof(request.Codigo));
        ValidateRequired(request.Descripcion, nameof(request.Descripcion));
        ValidateRequired(request.Moneda, nameof(request.Moneda));
        await RequireEmpresaAsync(request.EmpresaId);

        var now = DateTimeOffset.UtcNow;
        var centro = new CentroCosto
        {
            Id = Guid.NewGuid(),
            EmpresaId = request.EmpresaId,
            Codigo = request.Codigo.Trim(),
            Descripcion = request.Descripcion.Trim(),
            Moneda = request.Moneda.Trim(),
            Estado = NormalizeEstado(request.Estado),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repository.SaveCentroCostoAsync(centro);
        return centro;
    }

    public async Task<CentroCosto?> UpdateCentroCostoAsync(Guid id, UpdateCentroCostoRequest request)
    {
        ValidateRequired(request.Codigo, nameof(request.Codigo));
        ValidateRequired(request.Descripcion, nameof(request.Descripcion));
        ValidateRequired(request.Moneda, nameof(request.Moneda));

        var centro = await _repository.GetCentroCostoAsync(id);
        if (centro is null) return null;

        centro.Codigo = request.Codigo.Trim();
        centro.Descripcion = request.Descripcion.Trim();
        centro.Moneda = request.Moneda.Trim();
        centro.Estado = NormalizeEstado(request.Estado);
        centro.UpdatedAt = DateTimeOffset.UtcNow;

        await _repository.SaveCentroCostoAsync(centro);
        return centro;
    }

    public async Task<CentroCosto?> DeactivateCentroCostoAsync(Guid id)
    {
        var centro = await _repository.GetCentroCostoAsync(id);
        if (centro is null) return null;

        centro.Estado = "Inactiva";
        centro.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.SaveCentroCostoAsync(centro);
        return centro;
    }

    public Task<IReadOnlyCollection<Sindicato>> GetSindicatosAsync() => _repository.GetSindicatosAsync();

    public Task<Sindicato?> GetSindicatoAsync(Guid id) => _repository.GetSindicatoAsync(id);

    public async Task<Sindicato> CreateSindicatoAsync(CreateSindicatoRequest request)
    {
        ValidateRequired(request.Nombre, nameof(request.Nombre));
        var now = DateTimeOffset.UtcNow;
        var sindicato = new Sindicato
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre.Trim(),
            Codigo = request.Codigo?.Trim(),
            Jurisdiccion = request.Jurisdiccion?.Trim(),
            Estado = "Activo",
            CreatedAt = now,
            UpdatedAt = now
        };
        await _repository.SaveSindicatoAsync(sindicato);
        return sindicato;
    }

    public async Task<Sindicato?> UpdateSindicatoAsync(Guid id, UpdateSindicatoRequest request)
    {
        ValidateRequired(request.Nombre, nameof(request.Nombre));
        var sindicato = await _repository.GetSindicatoAsync(id);
        if (sindicato is null) return null;

        sindicato.Nombre = request.Nombre.Trim();
        sindicato.Codigo = request.Codigo?.Trim();
        sindicato.Jurisdiccion = request.Jurisdiccion?.Trim();
        sindicato.Estado = string.IsNullOrWhiteSpace(request.Estado) ? sindicato.Estado : request.Estado.Trim();
        sindicato.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.SaveSindicatoAsync(sindicato);
        return sindicato;
    }

    public async Task<Sindicato?> DeactivateSindicatoAsync(Guid id)
    {
        var sindicato = await _repository.GetSindicatoAsync(id);
        if (sindicato is null) return null;

        sindicato.Estado = "Inactivo";
        sindicato.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.SaveSindicatoAsync(sindicato);
        return sindicato;
    }

    public Task<IReadOnlyCollection<Convenio>> GetConveniosAsync(Guid? sindicatoId) =>
        _repository.GetConveniosAsync(sindicatoId);

    public Task<Convenio?> GetConvenioAsync(Guid id) => _repository.GetConvenioAsync(id);

    public async Task<Convenio> CreateConvenioAsync(CreateConvenioRequest request)
    {
        ValidateRequired(request.Nombre, nameof(request.Nombre));
        var sindicato = await _repository.GetSindicatoAsync(request.SindicatoId);
        if (sindicato is null) throw new InvalidOperationException("Sindicato inexistente");

        var now = DateTimeOffset.UtcNow;
        var convenio = new Convenio
        {
            Id = Guid.NewGuid(),
            SindicatoId = request.SindicatoId,
            Nombre = request.Nombre.Trim(),
            Numero = request.Numero?.Trim(),
            VigenciaDesde = request.VigenciaDesde,
            VigenciaHasta = request.VigenciaHasta,
            Estado = "Activo",
            CreatedAt = now,
            UpdatedAt = now
        };
        await _repository.SaveConvenioAsync(convenio);
        return convenio;
    }

    public async Task<Convenio?> UpdateConvenioAsync(Guid id, UpdateConvenioRequest request)
    {
        ValidateRequired(request.Nombre, nameof(request.Nombre));
        var convenio = await _repository.GetConvenioAsync(id);
        if (convenio is null) return null;

        convenio.SindicatoId = request.SindicatoId;
        convenio.Nombre = request.Nombre.Trim();
        convenio.Numero = request.Numero?.Trim();
        convenio.VigenciaDesde = request.VigenciaDesde;
        convenio.VigenciaHasta = request.VigenciaHasta;
        convenio.Estado = string.IsNullOrWhiteSpace(request.Estado) ? convenio.Estado : request.Estado.Trim();
        convenio.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.SaveConvenioAsync(convenio);
        return convenio;
    }

    public async Task<Convenio?> DeactivateConvenioAsync(Guid id)
    {
        var convenio = await _repository.GetConvenioAsync(id);
        if (convenio is null) return null;

        convenio.Estado = "Inactivo";
        convenio.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.SaveConvenioAsync(convenio);
        return convenio;
    }

    public Task<IReadOnlyCollection<OrganigramaVersion>> GetOrganigramasAsync(Guid? empresaId) =>
        _repository.GetOrganigramasAsync(empresaId);

    public Task<OrganigramaVersion?> GetOrganigramaAsync(Guid id) => _repository.GetOrganigramaAsync(id);

    public async Task<OrganigramaVersion> CreateOrganigramaAsync(CreateOrganigramaVersionRequest request)
    {
        ValidateRequired(request.Nombre, nameof(request.Nombre));
        await RequireEmpresaAsync(request.EmpresaId);

        var existing = await _repository.GetOrganigramasAsync(request.EmpresaId);
        var nextVersion = existing.Any() ? existing.Max(o => o.Version) + 1 : 1;
        var tree = await GetUnidadesTreeAsync(request.EmpresaId);
        var json = System.Text.Json.JsonSerializer.Serialize(tree);

        var version = new OrganigramaVersion
        {
            Id = Guid.NewGuid(),
            EmpresaId = request.EmpresaId,
            Version = nextVersion,
            Nombre = request.Nombre.Trim(),
            UnidadesJson = json,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _repository.SaveOrganigramaAsync(version);
        return version;
    }

    private async Task RequireEmpresaAsync(Guid empresaId)
    {
        var empresa = await _repository.GetEmpresaAsync(empresaId);
        if (empresa is null)
        {
            _logger.LogWarning("Empresa {EmpresaId} inexistente", empresaId);
            throw new InvalidOperationException("Empresa inexistente");
        }
    }

    private async Task RequireUnidadAsync(Guid unidadId)
    {
        var unidad = await _repository.GetUnidadAsync(unidadId);
        if (unidad is null)
        {
            _logger.LogWarning("Unidad {UnidadId} inexistente", unidadId);
            throw new InvalidOperationException("Unidad inexistente");
        }
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
        return string.IsNullOrEmpty(normalized) ? "Activa" : normalized;
    }
}

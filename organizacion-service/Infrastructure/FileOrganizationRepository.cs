using System.Text.Json;
using OrganizacionService.Domain.Models;

namespace OrganizacionService.Infrastructure;

public class FileOrganizationRepository : IOrganizationRepository
{
    private readonly string _dbPath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };
    private readonly SemaphoreSlim _gate = new(1, 1);

    public FileOrganizationRepository(IHostEnvironment env)
    {
        var storageDir = Path.Combine(env.ContentRootPath, "storage");
        Directory.CreateDirectory(storageDir);
        _dbPath = Path.Combine(storageDir, "organizacion-db.json");
    }

    public async Task<IReadOnlyCollection<Empresa>> GetEmpresasAsync()
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Empresas;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<Empresa?> GetEmpresaAsync(Guid id)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Empresas.FirstOrDefault(e => e.Id == id);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveEmpresaAsync(Empresa empresa)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            var index = store.Empresas.FindIndex(e => e.Id == empresa.Id);
            if (index >= 0)
            {
                store.Empresas[index] = empresa;
            }
            else
            {
                store.Empresas.Add(empresa);
            }

            await PersistAsync(store);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyCollection<UnidadOrganizativa>> GetUnidadesAsync()
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Unidades;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<UnidadOrganizativa?> GetUnidadAsync(Guid id)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Unidades.FirstOrDefault(u => u.Id == id);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveUnidadAsync(UnidadOrganizativa unidad)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            var index = store.Unidades.FindIndex(u => u.Id == unidad.Id);
            if (index >= 0)
            {
                store.Unidades[index] = unidad;
            }
            else
            {
                store.Unidades.Add(unidad);
            }

            await PersistAsync(store);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyCollection<Posicion>> GetPosicionesAsync()
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Posiciones;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<Posicion?> GetPosicionAsync(Guid id)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Posiciones.FirstOrDefault(p => p.Id == id);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SavePosicionAsync(Posicion posicion)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            var index = store.Posiciones.FindIndex(p => p.Id == posicion.Id);
            if (index >= 0)
            {
                store.Posiciones[index] = posicion;
            }
            else
            {
                store.Posiciones.Add(posicion);
            }

            await PersistAsync(store);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyCollection<CentroCosto>> GetCentrosCostoAsync()
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.CentrosCosto;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<CentroCosto?> GetCentroCostoAsync(Guid id)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.CentrosCosto.FirstOrDefault(c => c.Id == id);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveCentroCostoAsync(CentroCosto centroCosto)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            var index = store.CentrosCosto.FindIndex(c => c.Id == centroCosto.Id);
            if (index >= 0)
            {
                store.CentrosCosto[index] = centroCosto;
            }
            else
            {
                store.CentrosCosto.Add(centroCosto);
            }

            await PersistAsync(store);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyCollection<Sindicato>> GetSindicatosAsync()
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Sindicatos;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<Sindicato?> GetSindicatoAsync(Guid id)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Sindicatos.FirstOrDefault(s => s.Id == id);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveSindicatoAsync(Sindicato sindicato)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            var index = store.Sindicatos.FindIndex(s => s.Id == sindicato.Id);
            if (index >= 0)
            {
                store.Sindicatos[index] = sindicato;
            }
            else
            {
                store.Sindicatos.Add(sindicato);
            }
            await PersistAsync(store);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyCollection<Convenio>> GetConveniosAsync(Guid? sindicatoId)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            var query = store.Convenios.AsEnumerable();
            if (sindicatoId.HasValue)
            {
                query = query.Where(c => c.SindicatoId == sindicatoId.Value);
            }
            return query.ToList();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<Convenio?> GetConvenioAsync(Guid id)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Convenios.FirstOrDefault(c => c.Id == id);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveConvenioAsync(Convenio convenio)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            var index = store.Convenios.FindIndex(c => c.Id == convenio.Id);
            if (index >= 0)
            {
                store.Convenios[index] = convenio;
            }
            else
            {
                store.Convenios.Add(convenio);
            }
            await PersistAsync(store);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyCollection<OrganigramaVersion>> GetOrganigramasAsync(Guid? empresaId)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            var query = store.Organigramas.AsEnumerable();
            if (empresaId.HasValue)
            {
                query = query.Where(o => o.EmpresaId == empresaId.Value);
            }
            return query.OrderByDescending(o => o.Version).ToList();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<OrganigramaVersion?> GetOrganigramaAsync(Guid id)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Organigramas.FirstOrDefault(o => o.Id == id);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveOrganigramaAsync(OrganigramaVersion version)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            var index = store.Organigramas.FindIndex(o => o.Id == version.Id);
            if (index >= 0)
            {
                store.Organigramas[index] = version;
            }
            else
            {
                store.Organigramas.Add(version);
            }
            await PersistAsync(store);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<OrganizationStore> LoadAsync()
    {
        if (!File.Exists(_dbPath))
        {
            return new OrganizationStore();
        }

        await using var stream = File.OpenRead(_dbPath);
        var data = await JsonSerializer.DeserializeAsync<OrganizationStore>(stream, _jsonOptions);
        return data ?? new OrganizationStore();
    }

    private async Task PersistAsync(OrganizationStore store)
    {
        await using var stream = File.Create(_dbPath);
        await JsonSerializer.SerializeAsync(stream, store, _jsonOptions);
    }
}

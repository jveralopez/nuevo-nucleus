using System.Text.Json;
using PersonalService.Domain.Models;

namespace PersonalService.Infrastructure;

public class FilePersonalRepository : IPersonalRepository
{
    private readonly string _dbPath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };
    private readonly SemaphoreSlim _gate = new(1, 1);

    public FilePersonalRepository(IHostEnvironment env)
    {
        var storageDir = Path.Combine(env.ContentRootPath, "storage");
        Directory.CreateDirectory(storageDir);
        _dbPath = Path.Combine(storageDir, "personal-db.json");
    }

    public async Task<IReadOnlyCollection<Legajo>> GetLegajosAsync()
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Legajos;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<Legajo?> GetLegajoAsync(Guid id)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Legajos.FirstOrDefault(l => l.Id == id);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<Legajo?> GetLegajoByNumeroAsync(string numero)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Legajos.FirstOrDefault(l => string.Equals(l.Numero, numero, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveLegajoAsync(Legajo legajo)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            var index = store.Legajos.FindIndex(l => l.Id == legajo.Id);
            if (index >= 0)
            {
                store.Legajos[index] = legajo;
            }
            else
            {
                store.Legajos.Add(legajo);
            }

            await PersistAsync(store);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<PersonalStore> LoadAsync()
    {
        if (!File.Exists(_dbPath))
        {
            return new PersonalStore();
        }

        await using var stream = File.OpenRead(_dbPath);
        var data = await JsonSerializer.DeserializeAsync<PersonalStore>(stream, _jsonOptions);
        return data ?? new PersonalStore();
    }

    private async Task PersistAsync(PersonalStore store)
    {
        await using var stream = File.Create(_dbPath);
        await JsonSerializer.SerializeAsync(stream, store, _jsonOptions);
    }
}

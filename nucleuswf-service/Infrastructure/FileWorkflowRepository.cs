using System.Text.Json;
using NucleusWFService.Domain.Models;

namespace NucleusWFService.Infrastructure;

public class FileWorkflowRepository : IWorkflowRepository
{
    private readonly string _dbPath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };
    private readonly SemaphoreSlim _gate = new(1, 1);

    public FileWorkflowRepository(IHostEnvironment env)
    {
        var storageDir = Path.Combine(env.ContentRootPath, "storage");
        Directory.CreateDirectory(storageDir);
        _dbPath = Path.Combine(storageDir, "nucleuswf-db.json");
    }

    public async Task<IReadOnlyCollection<WorkflowDefinition>> GetDefinitionsAsync()
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Definitions;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<WorkflowDefinition?> GetDefinitionAsync(Guid id)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Definitions.FirstOrDefault(d => d.Id == id);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<WorkflowDefinition?> GetDefinitionByKeyAsync(string key, string version)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Definitions.FirstOrDefault(d =>
                string.Equals(d.Key, key, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(d.Version, version, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveDefinitionAsync(WorkflowDefinition definition)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            var index = store.Definitions.FindIndex(d => d.Id == definition.Id);
            if (index >= 0)
            {
                store.Definitions[index] = definition;
            }
            else
            {
                store.Definitions.Add(definition);
            }

            await PersistAsync(store);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyCollection<WorkflowInstance>> GetInstancesAsync()
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Instances;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<WorkflowInstance?> GetInstanceAsync(Guid id)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Instances.FirstOrDefault(i => i.Id == id);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveInstanceAsync(WorkflowInstance instance)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            var index = store.Instances.FindIndex(i => i.Id == instance.Id);
            if (index >= 0)
            {
                store.Instances[index] = instance;
            }
            else
            {
                store.Instances.Add(instance);
            }

            await PersistAsync(store);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<WorkflowStore> LoadAsync()
    {
        if (!File.Exists(_dbPath))
        {
            return new WorkflowStore();
        }

        await using var stream = File.OpenRead(_dbPath);
        var data = await JsonSerializer.DeserializeAsync<WorkflowStore>(stream, _jsonOptions);
        return data ?? new WorkflowStore();
    }

    private async Task PersistAsync(WorkflowStore store)
    {
        await using var stream = File.Create(_dbPath);
        await JsonSerializer.SerializeAsync(stream, store, _jsonOptions);
    }
}

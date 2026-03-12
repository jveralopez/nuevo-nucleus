using System.Text.Json;
using IntegrationHubService.Domain.Models;

namespace IntegrationHubService.Infrastructure;

public class FileIntegrationRepository : IIntegrationRepository
{
    private readonly string _dbPath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };
    private readonly SemaphoreSlim _gate = new(1, 1);

    public FileIntegrationRepository(IHostEnvironment env)
    {
        var storageDir = Path.Combine(env.ContentRootPath, "storage");
        Directory.CreateDirectory(storageDir);
        _dbPath = Path.Combine(storageDir, "integration-hub-db.json");
    }

    public async Task<IReadOnlyCollection<IntegrationTemplate>> GetTemplatesAsync()
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Templates;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IntegrationTemplate?> GetTemplateAsync(Guid id)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Templates.FirstOrDefault(t => t.Id == id);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveTemplateAsync(IntegrationTemplate template)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            var index = store.Templates.FindIndex(t => t.Id == template.Id);
            if (index >= 0)
            {
                store.Templates[index] = template;
            }
            else
            {
                store.Templates.Add(template);
            }

            await PersistAsync(store);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyCollection<IntegrationConnection>> GetConnectionsAsync()
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Connections;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IntegrationConnection?> GetConnectionAsync(Guid id)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Connections.FirstOrDefault(c => c.Id == id);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IntegrationConnection?> GetConnectionByNameAsync(string name)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Connections.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveConnectionAsync(IntegrationConnection connection)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            var index = store.Connections.FindIndex(c => c.Id == connection.Id);
            if (index >= 0)
            {
                store.Connections[index] = connection;
            }
            else
            {
                store.Connections.Add(connection);
            }

            await PersistAsync(store);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyCollection<IntegrationJob>> GetJobsAsync()
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Jobs;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IntegrationJob?> GetJobAsync(Guid id)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Jobs.FirstOrDefault(j => j.Id == id);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveJobAsync(IntegrationJob job)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            var index = store.Jobs.FindIndex(j => j.Id == job.Id);
            if (index >= 0)
            {
                store.Jobs[index] = job;
            }
            else
            {
                store.Jobs.Add(job);
            }

            await PersistAsync(store);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyCollection<IntegrationEvent>> GetEventsAsync(Guid? jobId)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            var query = store.Events.AsEnumerable();
            if (jobId.HasValue)
            {
                query = query.Where(e => e.JobId == jobId.Value);
            }
            return query.OrderByDescending(e => e.CreatedAt).ToList();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveEventAsync(IntegrationEvent integrationEvent)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            store.Events.Add(integrationEvent);
            await PersistAsync(store);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyCollection<IntegrationTrigger>> GetTriggersAsync(string? eventName)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            var query = store.Triggers.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(eventName))
            {
                query = query.Where(t => string.Equals(t.EventName, eventName, StringComparison.OrdinalIgnoreCase));
            }
            return query.OrderByDescending(t => t.CreatedAt).ToList();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IntegrationTrigger?> GetTriggerAsync(Guid id)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Triggers.FirstOrDefault(t => t.Id == id);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveTriggerAsync(IntegrationTrigger trigger)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            var index = store.Triggers.FindIndex(t => t.Id == trigger.Id);
            if (index >= 0)
            {
                store.Triggers[index] = trigger;
            }
            else
            {
                store.Triggers.Add(trigger);
            }
            await PersistAsync(store);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<IntegrationStore> LoadAsync()
    {
        if (!File.Exists(_dbPath))
        {
            return new IntegrationStore();
        }

        await using var stream = File.OpenRead(_dbPath);
        var data = await JsonSerializer.DeserializeAsync<IntegrationStore>(stream, _jsonOptions);
        return data ?? new IntegrationStore();
    }

    private async Task PersistAsync(IntegrationStore store)
    {
        await using var stream = File.Create(_dbPath);
        await JsonSerializer.SerializeAsync(stream, store, _jsonOptions);
    }
}

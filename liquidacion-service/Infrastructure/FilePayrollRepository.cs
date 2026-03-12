using System.Text.Json;
using LiquidacionService.Domain.Models;

namespace LiquidacionService.Infrastructure;

public class FilePayrollRepository : IPayrollRepository
{
    private readonly string _dbPath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };
    private readonly SemaphoreSlim _gate = new(1, 1);

    public FilePayrollRepository(IHostEnvironment env)
    {
        var storageDir = Path.Combine(env.ContentRootPath, "storage");
        Directory.CreateDirectory(storageDir);
        _dbPath = Path.Combine(storageDir, "liquidacion-db.json");
    }

    public async Task<IReadOnlyCollection<PayrollRun>> GetAllAsync()
    {
        await _gate.WaitAsync();
        try
        {
            return await LoadAsync();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<PayrollRun?> GetAsync(Guid id)
    {
        await _gate.WaitAsync();
        try
        {
            var items = await LoadAsync();
            return items.FirstOrDefault(p => p.Id == id);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveAsync(PayrollRun payroll)
    {
        await _gate.WaitAsync();
        try
        {
            var items = await LoadAsync();
            var index = items.FindIndex(p => p.Id == payroll.Id);
            if (index >= 0)
            {
                items[index] = payroll;
            }
            else
            {
                items.Add(payroll);
            }

            await PersistAsync(items);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<List<PayrollRun>> LoadAsync()
    {
        if (!File.Exists(_dbPath))
        {
            return new List<PayrollRun>();
        }

        await using var stream = File.OpenRead(_dbPath);
        var data = await JsonSerializer.DeserializeAsync<List<PayrollRun>>(stream, _jsonOptions);
        return data ?? new List<PayrollRun>();
    }

    private async Task PersistAsync(List<PayrollRun> items)
    {
        await using var stream = File.Create(_dbPath);
        await JsonSerializer.SerializeAsync(stream, items, _jsonOptions);
    }
}

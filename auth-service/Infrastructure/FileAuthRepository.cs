using System.Text.Json;
using AuthService.Domain.Models;

namespace AuthService.Infrastructure;

public class FileAuthRepository : IAuthRepository
{
    private readonly string _dbPath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };
    private readonly SemaphoreSlim _gate = new(1, 1);

    public FileAuthRepository(IHostEnvironment env)
    {
        var storageDir = Path.Combine(env.ContentRootPath, "storage");
        Directory.CreateDirectory(storageDir);
        _dbPath = Path.Combine(storageDir, "auth-db.json");
    }

    public async Task<IReadOnlyCollection<User>> GetUsersAsync()
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Users;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            return store.Users.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveUserAsync(User user)
    {
        await _gate.WaitAsync();
        try
        {
            var store = await LoadAsync();
            var index = store.Users.FindIndex(u => u.Id == user.Id);
            if (index >= 0)
            {
                store.Users[index] = user;
            }
            else
            {
                store.Users.Add(user);
            }

            await PersistAsync(store);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<AuthStore> LoadAsync()
    {
        if (!File.Exists(_dbPath))
        {
            return new AuthStore();
        }

        await using var stream = File.OpenRead(_dbPath);
        var data = await JsonSerializer.DeserializeAsync<AuthStore>(stream, _jsonOptions);
        return data ?? new AuthStore();
    }

    private async Task PersistAsync(AuthStore store)
    {
        await using var stream = File.Create(_dbPath);
        await JsonSerializer.SerializeAsync(stream, store, _jsonOptions);
    }
}

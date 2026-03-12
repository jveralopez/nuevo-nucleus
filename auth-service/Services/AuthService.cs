using AuthService.Domain.Models;
using AuthService.Domain.Requests;
using AuthService.Infrastructure;

namespace AuthService.Services;

public class AuthService
{
    private readonly IAuthRepository _repository;
    private readonly AuthOptions _options;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IAuthRepository repository, AuthOptions options, ILogger<AuthService> logger)
    {
        _repository = repository;
        _options = options;
        _logger = logger;
    }

    public async Task EnsureSeedAdminAsync()
    {
        var existing = await _repository.GetUserByUsernameAsync(_options.SeedAdminUser);
        if (existing is not null) return;

        var user = CreateUser(new CreateUserRequest(_options.SeedAdminUser, _options.SeedAdminPassword, "Admin", "Activo"));
        await _repository.SaveUserAsync(user);
    }

    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        ValidateRequired(request.Username, nameof(request.Username));
        ValidateRequired(request.Password, nameof(request.Password));

        var existing = await _repository.GetUserByUsernameAsync(request.Username.Trim());
        if (existing is not null)
        {
            _logger.LogWarning("Usuario {Username} ya existe", request.Username);
            throw new InvalidOperationException("El usuario ya existe");
        }

        var user = CreateUser(request);
        await _repository.SaveUserAsync(user);
        return user;
    }

    public async Task<User?> AuthenticateAsync(LoginRequest request)
    {
        ValidateRequired(request.Username, nameof(request.Username));
        ValidateRequired(request.Password, nameof(request.Password));

        var user = await _repository.GetUserByUsernameAsync(request.Username.Trim());
        if (user is null || user.Estado != "Activo") return null;

        var hash = PasswordHasher.Hash(request.Password, user.Salt);
        return hash == user.PasswordHash ? user : null;
    }

    private static void ValidateRequired(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{field} es requerido");
        }
    }

    private User CreateUser(CreateUserRequest request)
    {
        var salt = PasswordHasher.GenerateSalt();
        var now = DateTimeOffset.UtcNow;
        return new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username.Trim(),
            Salt = salt,
            PasswordHash = PasswordHasher.Hash(request.Password, salt),
            Role = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role.Trim(),
            Estado = string.IsNullOrWhiteSpace(request.Estado) ? "Activo" : request.Estado.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}

using AuthService.Domain.Models;

namespace AuthService.Infrastructure;

public interface IAuthRepository
{
    Task<IReadOnlyCollection<User>> GetUsersAsync();
    Task<User?> GetUserByUsernameAsync(string username);
    Task SaveUserAsync(User user);
}

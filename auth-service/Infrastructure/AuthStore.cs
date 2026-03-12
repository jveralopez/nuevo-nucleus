using AuthService.Domain.Models;

namespace AuthService.Infrastructure;

public class AuthStore
{
    public List<User> Users { get; set; } = new();
}

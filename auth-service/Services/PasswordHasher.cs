using System.Security.Cryptography;
using System.Text;

namespace AuthService.Services;

public static class PasswordHasher
{
    public static string GenerateSalt()
    {
        var bytes = RandomNumberGenerator.GetBytes(16);
        return Convert.ToBase64String(bytes);
    }

    public static string Hash(string password, string salt)
    {
        var combined = Encoding.UTF8.GetBytes($"{salt}:{password}");
        var hash = SHA256.HashData(combined);
        return Convert.ToBase64String(hash);
    }
}

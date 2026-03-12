namespace AuthService.Services;

public class AuthOptions
{
    public string Issuer { get; set; } = "nucleus-auth";
    public string Audience { get; set; } = "nucleus-api";
    public string SigningKey { get; set; } = "CHANGE_ME_SUPER_SECRET_KEY";
    public int ExpirationMinutes { get; set; } = 60;
    public string SeedAdminUser { get; set; } = "admin";
    public string SeedAdminPassword { get; set; } = "admin123";
}

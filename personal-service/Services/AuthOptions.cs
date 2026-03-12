namespace PersonalService.Services;

public class AuthOptions
{
    public string Issuer { get; set; } = "nucleus-auth";
    public string Audience { get; set; } = "nucleus-api";
    public string SigningKey { get; set; } = "CHANGE_ME_SUPER_SECRET_KEY";
}

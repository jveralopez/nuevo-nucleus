using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;

namespace PersonalService.Tests;

[Collection("Sequential")]
public class PersonalAuthApiTests : IClassFixture<PersonalHealthApiTests.TestFactory>
{
    private readonly PersonalHealthApiTests.TestFactory _factory;

    private const string TestIssuer = "nucleus-auth";
    private const string TestAudience = "nucleus-api";
    private const string TestSigningKey = "CHANGE_ME_SUPER_SECRET_KEY_32_CHARS_LONG";

    public PersonalAuthApiTests(PersonalHealthApiTests.TestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Legajos_RequiresAuthentication()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/legajos");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Legajos_ReturnsOk_WithToken()
    {
        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BuildToken("user-1"));

        var response = await client.GetAsync("/legajos");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
    }

    private static string BuildToken(string userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim> { new("sub", userId) };
        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

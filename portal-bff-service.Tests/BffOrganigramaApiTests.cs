using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace PortalBffService.Tests;

public class BffOrganigramaApiTests : IClassFixture<BffNotificationsApiTests.TestFactory>
{
    private readonly BffNotificationsApiTests.TestFactory _factory;

    private const string TestIssuer = "nucleus-auth";
    private const string TestAudience = "nucleus-api";
    private const string TestSigningKey = "CHANGE_ME_SUPER_SECRET_KEY_32_CHARS_LONG";

    public BffOrganigramaApiTests(BffNotificationsApiTests.TestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Organigramas_Bff_Proxy_Works()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BuildToken("rh-admin", "Admin"));

        var createResponse = await client.PostAsJsonAsync("/api/rh/v1/organizacion/organigramas", new
        {
            empresaId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
            nombre = "Reorden 2026"
        });
        await EnsureSuccess(createResponse, "crear organigrama bff");

        var response = await client.GetAsync("/api/rh/v1/organizacion/organigramas");
        await EnsureSuccess(response, "listar organigramas bff");

        var organigramas = await response.Content.ReadFromJsonAsync<List<OrganigramaItem>>();
        Assert.NotNull(organigramas);
        Assert.Single(organigramas!);
    }

    private static string BuildToken(string userId, string? role = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim> { new Claim("sub", userId) };
        if (!string.IsNullOrWhiteSpace(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static async Task EnsureSuccess(HttpResponseMessage response, string action)
    {
        if (response.IsSuccessStatusCode) return;
        var body = await response.Content.ReadAsStringAsync();
        throw new InvalidOperationException($"{action} status: {(int)response.StatusCode} body: {body}");
    }

    private sealed class OrganigramaItem
    {
        public Guid Id { get; init; }
        public Guid EmpresaId { get; init; }
        public int Version { get; init; }
        public string Nombre { get; init; } = string.Empty;
    }
}

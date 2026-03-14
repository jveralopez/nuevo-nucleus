using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace PortalBffService.Tests;

public class BffTiemposApiTests : IClassFixture<BffNotificationsApiTests.TestFactory>
{
    private readonly BffNotificationsApiTests.TestFactory _factory;

    private const string TestIssuer = "nucleus-auth";
    private const string TestAudience = "nucleus-api";
    private const string TestSigningKey = "CHANGE_ME_SUPER_SECRET_KEY_32_CHARS_LONG";

    public BffTiemposApiTests(BffNotificationsApiTests.TestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Tiempos_Bff_Proxy_Works()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BuildToken("rh-admin", "Admin"));

        var turnosResponse = await client.GetAsync("/api/rh/v1/tiempos/turnos");
        await EnsureSuccess(turnosResponse, "listar turnos bff");

        var createTurno = await client.PostAsJsonAsync("/api/rh/v1/tiempos/turnos", new
        {
            codigo = "TUR-01",
            nombre = "Diurno",
            horaInicio = "08:00",
            horaFin = "17:00",
            toleranciaMinutos = 5,
            activo = true
        });
        await EnsureSuccess(createTurno, "crear turno bff");

        var fichadasResponse = await client.GetAsync("/api/rh/v1/tiempos/fichadas");
        await EnsureSuccess(fichadasResponse, "listar fichadas bff");

        var planillasResponse = await client.GetAsync("/api/rh/v1/tiempos/planillas");
        await EnsureSuccess(planillasResponse, "listar planillas bff");
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
}

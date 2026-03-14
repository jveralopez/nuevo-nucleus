using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using LiquidacionService.Domain.Requests;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace LiquidacionService.Tests;

public class LiquidacionExportsApiTests : IClassFixture<LiquidacionExportsApiTests.TestFactory>
{
    private readonly TestFactory _factory;

    private const string TestIssuer = "nucleus-auth";
    private const string TestAudience = "nucleus-api";
    private const string TestSigningKey = "CHANGE_ME_SUPER_SECRET_KEY_32_CHARS_LONG";

    public LiquidacionExportsApiTests(TestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetExports_RequiresAuthentication()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync($"/payrolls/{Guid.NewGuid()}/exports");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetExports_ReturnsFiles_WhenExported()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BuildAdminToken());

        var createResponse = await client.PostAsJsonAsync("/payrolls", new NewPayrollRequest("2026-06", "Mensual", "Test export"));
        await EnsureSuccess(createResponse, "crear liquidacion");
        var payroll = await createResponse.Content.ReadFromJsonAsync<PayrollResponse>();
        Assert.NotNull(payroll);

        var legajoResponse = await client.PostAsJsonAsync($"/payrolls/{payroll!.Id}/legajos",
            BuildLegajo("200", "Ana Perez", "20-00000000-0"));
        await EnsureSuccess(legajoResponse, "agregar legajo");

        var procesarResponse = await client.PostAsJsonAsync($"/payrolls/{payroll.Id}/procesar", new ProcessPayrollRequest(true));
        await EnsureSuccess(procesarResponse, "procesar liquidacion");

        var exportsResponse = await client.GetAsync($"/payrolls/{payroll.Id}/exports");
        await EnsureSuccess(exportsResponse, "listar exportes");

        var exports = await exportsResponse.Content.ReadFromJsonAsync<List<ExportItem>>();
        Assert.NotNull(exports);
        Assert.Contains(exports!, item => item.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(exports!, item => item.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase));
        Assert.All(exports!, item => Assert.StartsWith("/exports/", item.Url, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetExportsEmpleado_ReturnsFiles_ForAuthenticatedUser()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BuildAdminToken());

        var createResponse = await client.PostAsJsonAsync("/payrolls", new NewPayrollRequest("2026-07", "Mensual", "Test export empleado"));
        await EnsureSuccess(createResponse, "crear liquidacion");
        var payroll = await createResponse.Content.ReadFromJsonAsync<PayrollResponse>();
        Assert.NotNull(payroll);

        var legajoResponse = await client.PostAsJsonAsync($"/payrolls/{payroll!.Id}/legajos",
            BuildLegajo("201", "Juan Gomez", "20-00000000-1"));
        await EnsureSuccess(legajoResponse, "agregar legajo");

        var procesarResponse = await client.PostAsJsonAsync($"/payrolls/{payroll.Id}/procesar", new ProcessPayrollRequest(true));
        await EnsureSuccess(procesarResponse, "procesar liquidacion");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BuildUserToken());
        var exportsResponse = await client.GetAsync($"/payrolls/{payroll.Id}/exports/empleado");
        await EnsureSuccess(exportsResponse, "listar exportes empleado");

        var exports = await exportsResponse.Content.ReadFromJsonAsync<List<ExportItem>>();
        Assert.NotNull(exports);
        Assert.Contains(exports!, item => item.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(exports!, item => item.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase));
    }

    private static string BuildAdminToken() => BuildToken(new[] { new Claim(ClaimTypes.Role, "Admin") });

    private static string BuildUserToken() => BuildToken(Array.Empty<Claim>());

    private static string BuildToken(IEnumerable<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
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

    private static UpsertLegajoRequest BuildLegajo(string numero, string nombre, string cuil)
    {
        return new UpsertLegajoRequest(
            Numero: numero,
            Nombre: nombre,
            Cuil: cuil,
            Convenio: null,
            Categoria: null,
            Basico: 1000,
            Antiguedad: 100,
            Adicionales: 50,
            Presentismo: 0,
            HorasExtra: 0,
            Premios: 0,
            Descuentos: 10,
            NoRemunerativo: 0,
            BonosNoRemunerativos: 0,
            AplicaGanancias: true,
            OmitirGanancias: false,
            ConyugeACargo: false,
            CantHijos: 0,
            CantOtrosFamiliares: 0,
            DeduccionesAdicionales: 0,
            VacacionesDias: 0,
            Licencias: null,
            Embargos: null,
            ContribucionesPatronales: null);
    }

    private sealed class PayrollResponse
    {
        public Guid Id { get; init; }
    }

    private sealed class ExportItem
    {
        public string FileName { get; init; } = string.Empty;
        public string Url { get; init; } = string.Empty;
    }

    public sealed class TestFactory : WebApplicationFactory<Program>
    {
        public TestFactory()
        {
            Environment.SetEnvironmentVariable("Auth__Issuer", TestIssuer);
            Environment.SetEnvironmentVariable("Auth__Audience", TestAudience);
            Environment.SetEnvironmentVariable("Auth__SigningKey", TestSigningKey);
            var dbPath = Path.Combine(Path.GetTempPath(), $"liquidacion-tests-{Guid.NewGuid():N}.db");
            Environment.SetEnvironmentVariable("ConnectionStrings__LiquidacionDb", $"Data Source={dbPath}");
        }
    }
}

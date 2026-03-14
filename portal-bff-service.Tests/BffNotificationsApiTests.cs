using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace PortalBffService.Tests;

public class BffNotificationsApiTests : IClassFixture<BffNotificationsApiTests.TestFactory>
{
    private readonly TestFactory _factory;

    private const string TestIssuer = "nucleus-auth";
    private const string TestAudience = "nucleus-api";
    private const string TestSigningKey = "CHANGE_ME_SUPER_SECRET_KEY_32_CHARS_LONG";
    private const string LiquidacionBase = "https://liquidacion.local";
    private const string WfBase = "https://wf.local";

    public BffNotificationsApiTests(TestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Notificaciones_RequiresAuthentication()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/portal/v1/notificaciones");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Notificaciones_CRUD_Flow()
    {
        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BuildToken("user-1"));

        var createResponse = await client.PostAsJsonAsync("/api/portal/v1/notificaciones", new NotificationRequest(
            "Recibo disponible",
            "Periodo 2026-07",
            "recibo-2026-07"));
        await EnsureSuccess(createResponse, "crear notificacion");

        var listResponse = await client.GetAsync("/api/portal/v1/notificaciones?unreadOnly=true");
        await EnsureSuccess(listResponse, "listar no leidas");
        var list = await listResponse.Content.ReadFromJsonAsync<List<NotificationResponse>>();
        Assert.NotNull(list);
        Assert.Contains(list!, item => item.Title == "Recibo disponible");

        var summaryResponse = await client.GetAsync("/api/portal/v1/notificaciones/resumen");
        await EnsureSuccess(summaryResponse, "resumen");
        var summary = await summaryResponse.Content.ReadFromJsonAsync<NotificationSummaryResponse>();
        Assert.NotNull(summary);
        Assert.True(summary!.Total >= 1);
        Assert.True(summary.Unread >= 1);

        var readAllResponse = await client.PostAsync("/api/portal/v1/notificaciones/read-all", null);
        await EnsureSuccess(readAllResponse, "read-all");

        var unreadResponse = await client.GetAsync("/api/portal/v1/notificaciones?unreadOnly=true");
        await EnsureSuccess(unreadResponse, "listar no leidas luego read-all");
        var unread = await unreadResponse.Content.ReadFromJsonAsync<List<NotificationResponse>>();
        Assert.NotNull(unread);
        Assert.DoesNotContain(unread!, item => item.ReadAt is null);
    }

    [Fact]
    public async Task Notificaciones_Paginacion()
    {
        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BuildToken("user-2"));

        for (var i = 0; i < 3; i++)
        {
            var response = await client.PostAsJsonAsync("/api/portal/v1/notificaciones", new NotificationRequest(
                $"Notif {i}",
                "Detalle",
                $"notif-{i}"));
            await EnsureSuccess(response, "crear notificacion paginacion");
        }

        var listResponse = await client.GetAsync("/api/portal/v1/notificaciones?limit=1&offset=0");
        await EnsureSuccess(listResponse, "listar paginado");
        var list = await listResponse.Content.ReadFromJsonAsync<List<NotificationResponse>>();
        Assert.NotNull(list);
        Assert.Single(list!);
    }

    [Fact]
    public async Task Exportes_Bff_Proxy_Works()
    {
        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BuildToken("user-3"));

        var listResponse = await client.GetAsync($"/api/portal/v1/liquidacion/{Guid.NewGuid()}/exports");
        await EnsureSuccess(listResponse, "listar exportes bff");
        var exports = await listResponse.Content.ReadFromJsonAsync<List<ExportItem>>();
        Assert.NotNull(exports);
        Assert.Single(exports!);

        var downloadResponse = await client.GetAsync("/api/portal/v1/liquidacion/exports/demo.json");
        await EnsureSuccess(downloadResponse, "descargar export bff");
        var body = await downloadResponse.Content.ReadAsStringAsync();
        Assert.Equal("demo", body);
    }

    [Fact]
    public async Task Workflows_Bff_Proxy_Works()
    {
        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BuildToken("user-4"));

        var defsResponse = await client.GetAsync("/api/portal/v1/wf/definitions");
        await EnsureSuccess(defsResponse, "listar definiciones");
        var defs = await defsResponse.Content.ReadFromJsonAsync<List<WorkflowDefinition>>();
        Assert.NotNull(defs);
        Assert.Single(defs!);

        var createInstanceResponse = await client.PostAsJsonAsync("/api/portal/v1/wf/instances", new { key = "vacaciones", version = "1.0.0" });
        await EnsureSuccess(createInstanceResponse, "crear instancia");
        var instance = await createInstanceResponse.Content.ReadFromJsonAsync<WorkflowInstance>();
        Assert.NotNull(instance);

        var transitionResponse = await client.PostAsJsonAsync($"/api/portal/v1/wf/instances/{instance!.Id}/transitions", new { evento = "aprobar" });
        await EnsureSuccess(transitionResponse, "transicionar instancia");
    }

    [Fact]
    public async Task Recibos_Bff_Proxy_Works()
    {
        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BuildToken("user-5"));

        var response = await client.GetAsync($"/api/portal/v1/liquidacion/{Guid.NewGuid()}/recibos");
        await EnsureSuccess(response, "recibos bff");
        var recibos = await response.Content.ReadFromJsonAsync<List<ReciboItem>>();
        Assert.NotNull(recibos);
        Assert.Single(recibos!);
    }

    [Fact]
    public async Task Rh_Bff_Proxy_Works()
    {
        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BuildToken("rh-admin", "Admin"));

        var empresasResponse = await client.GetAsync("/api/rh/v1/organizacion/empresas");
        await EnsureSuccess(empresasResponse, "empresas bff");
        var empresas = await empresasResponse.Content.ReadFromJsonAsync<List<EmpresaItem>>();
        Assert.NotNull(empresas);
        Assert.Single(empresas!);

        var legajosResponse = await client.GetAsync("/api/rh/v1/personal/legajos");
        await EnsureSuccess(legajosResponse, "legajos bff");
        var legajos = await legajosResponse.Content.ReadFromJsonAsync<List<LegajoItem>>();
        Assert.NotNull(legajos);
        Assert.Single(legajos!);

        var jobsResponse = await client.GetAsync("/api/rh/v1/integraciones/jobs");
        await EnsureSuccess(jobsResponse, "jobs bff");
        var jobs = await jobsResponse.Content.ReadFromJsonAsync<List<JobItem>>();
        Assert.NotNull(jobs);
        Assert.Single(jobs!);

        var eventsResponse = await client.GetAsync("/api/rh/v1/integraciones/eventos");
        await EnsureSuccess(eventsResponse, "eventos bff");
        var events = await eventsResponse.Content.ReadFromJsonAsync<List<EventItem>>();
        Assert.NotNull(events);
        Assert.Single(events!);

        var createEmpresa = await client.PostAsJsonAsync("/api/rh/v1/organizacion/empresas", new { nombre = "Demo", pais = "AR", moneda = "ARS" });
        await EnsureSuccess(createEmpresa, "crear empresa bff");

        var createLegajo = await client.PostAsJsonAsync("/api/rh/v1/personal/legajos", new { numero = "200", nombre = "Luis", apellido = "Perez", documento = "123", cuil = "20-00000000-2" });
        await EnsureSuccess(createLegajo, "crear legajo bff");

        var createJob = await client.PostAsJsonAsync("/api/rh/v1/integraciones/jobs", new { templateId = Guid.NewGuid(), periodo = "2026-08", trigger = "manual" });
        await EnsureSuccess(createJob, "crear job bff");

        var retryJob = await client.PostAsJsonAsync($"/api/rh/v1/integraciones/jobs/{Guid.NewGuid()}/retry", new { reason = "Manual" });
        await EnsureSuccess(retryJob, "retry job bff");

        var wfResponse = await client.GetAsync("/api/rh/v1/wf/instances");
        await EnsureSuccess(wfResponse, "wf instances bff");
        var wfJson = await wfResponse.Content.ReadAsStringAsync();
        Assert.Contains("estado", wfJson, StringComparison.OrdinalIgnoreCase);
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
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

    public sealed class TestFactory : WebApplicationFactory<Program>
    {
        public TestFactory()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), "portal-bff-tests", Guid.NewGuid().ToString("N"), "bff.db");
            Environment.SetEnvironmentVariable("ConnectionStrings__PortalBffDb", $"Data Source={dbPath}");
            Environment.SetEnvironmentVariable("Auth__Issuer", TestIssuer);
            Environment.SetEnvironmentVariable("Auth__Audience", TestAudience);
            Environment.SetEnvironmentVariable("Auth__SigningKey", TestSigningKey);
            Environment.SetEnvironmentVariable("Portal__LiquidacionApi", LiquidacionBase);
            Environment.SetEnvironmentVariable("Portal__WfApi", WfBase);
            Environment.SetEnvironmentVariable("Portal__TiemposApi", "https://tiempos.local");
        }

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddHttpClient("test-client").ConfigurePrimaryHttpMessageHandler(() => new FakeHttpHandler());
                services.AddTransient<IHttpClientFactory>(_ => new FixedClientFactory(new HttpClient(new FakeHttpHandler())));
            });
        }
    }

    private sealed class NotificationRequest
    {
        public NotificationRequest(string title, string detail, string sourceId)
        {
            Title = title;
            Detail = detail;
            SourceId = sourceId;
        }

        public string Title { get; }
        public string Detail { get; }
        public string SourceId { get; }
    }

    private sealed class NotificationResponse
    {
        public string Id { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Detail { get; init; } = string.Empty;
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset? ReadAt { get; init; }
    }

    private sealed class NotificationSummaryResponse
    {
        public int Total { get; init; }
        public int Unread { get; init; }
    }

    private sealed class ExportItem
    {
        public string FileName { get; init; } = string.Empty;
        public string Url { get; init; } = string.Empty;
    }

    private sealed class WorkflowDefinition
    {
        public Guid Id { get; init; }
        public string Key { get; init; } = string.Empty;
        public string Version { get; init; } = string.Empty;
        public string Nombre { get; init; } = string.Empty;
    }

    private sealed class WorkflowInstance
    {
        public Guid Id { get; init; }
        public string Estado { get; init; } = string.Empty;
    }

    private sealed class ReciboItem
    {
        public string LegajoNumero { get; init; } = string.Empty;
        public string LegajoNombre { get; init; } = string.Empty;
        public decimal Neto { get; init; }
    }

    private sealed class EmpresaItem
    {
        public Guid Id { get; init; }
        public string Nombre { get; init; } = string.Empty;
    }

    private sealed class LegajoItem
    {
        public Guid Id { get; init; }
        public string Numero { get; init; } = string.Empty;
    }

    private sealed class JobItem
    {
        public Guid Id { get; init; }
        public string Estado { get; init; } = string.Empty;
    }

    private sealed class EventItem
    {
        public Guid Id { get; init; }
        public string Tipo { get; init; } = string.Empty;
    }

    private sealed class FixedClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public FixedClientFactory(HttpClient client)
        {
            _client = client;
        }

        public HttpClient CreateClient(string name) => _client;
    }

    private sealed class FakeHttpHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            if (path.Contains("/payrolls/") && path.Contains("/exports/empleado"))
            {
                var payload = "[{\"fileName\":\"demo.json\",\"url\":\"/exports/demo.json\"}]";
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                });
            }
            if (path.Contains("/payrolls/") && path.EndsWith("/recibos", StringComparison.OrdinalIgnoreCase))
            {
                var payload = "[{\"legajoNumero\":\"100\",\"legajoNombre\":\"Ana\",\"neto\":100}]";
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                });
            }

            if (path.EndsWith("/empresas", StringComparison.OrdinalIgnoreCase))
            {
                var payload = "[{\"id\":\"00000000-0000-0000-0000-000000000010\",\"nombre\":\"Nucleus\"}]";
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                });
            }

            if (path.EndsWith("/organigramas", StringComparison.OrdinalIgnoreCase))
            {
                if (request.Method == HttpMethod.Post)
                {
                    var payload = "{\"id\":\"00000000-0000-0000-0000-000000000041\",\"empresaId\":\"00000000-0000-0000-0000-000000000010\",\"version\":2,\"nombre\":\"Reorden 2026\"}";
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Created)
                    {
                        Content = new StringContent(payload, Encoding.UTF8, "application/json")
                    });
                }

                var listPayload = "[{\"id\":\"00000000-0000-0000-0000-000000000041\",\"empresaId\":\"00000000-0000-0000-0000-000000000010\",\"version\":2,\"nombre\":\"Reorden 2026\"}]";
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(listPayload, Encoding.UTF8, "application/json")
                });
            }

            if (path.EndsWith("/legajos", StringComparison.OrdinalIgnoreCase))
            {
                var payload = "[{\"id\":\"00000000-0000-0000-0000-000000000011\",\"numero\":\"100\"}]";
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                });
            }

            if (path.EndsWith("/turnos", StringComparison.OrdinalIgnoreCase))
            {
                if (request.Method == HttpMethod.Post)
                {
                    var payload = "{\"id\":\"00000000-0000-0000-0000-000000000061\",\"codigo\":\"TUR-01\",\"nombre\":\"Diurno\"}";
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Created)
                    {
                        Content = new StringContent(payload, Encoding.UTF8, "application/json")
                    });
                }

                var listPayload = "[{\"id\":\"00000000-0000-0000-0000-000000000061\",\"codigo\":\"TUR-01\",\"nombre\":\"Diurno\"}]";
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(listPayload, Encoding.UTF8, "application/json")
                });
            }

            if (path.Contains("/horarios", StringComparison.OrdinalIgnoreCase))
            {
                var payload = "[{\"id\":\"00000000-0000-0000-0000-000000000062\",\"nombre\":\"Semana\",\"diasSemana\":\"Lun-Vie\"}]";
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                });
            }

            if (path.Contains("/fichadas", StringComparison.OrdinalIgnoreCase))
            {
                var payload = "[{\"id\":\"00000000-0000-0000-0000-000000000063\",\"tipo\":\"Entrada\",\"origen\":\"terminal\"}]";
                var status = request.Method == HttpMethod.Post ? HttpStatusCode.Created : HttpStatusCode.OK;
                return Task.FromResult(new HttpResponseMessage(status)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                });
            }

            if (path.Contains("/planillas", StringComparison.OrdinalIgnoreCase))
            {
                var payload = "[{\"id\":\"00000000-0000-0000-0000-000000000064\",\"periodo\":\"2026-02\",\"estado\":\"Borrador\"}]";
                var status = request.Method == HttpMethod.Post ? HttpStatusCode.Created : HttpStatusCode.OK;
                return Task.FromResult(new HttpResponseMessage(status)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                });
            }

            if (path.EndsWith("/jobs", StringComparison.OrdinalIgnoreCase))
            {
                var payload = "[{\"id\":\"00000000-0000-0000-0000-000000000012\",\"estado\":\"Pendiente\"}]";
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                });
            }

            if (path.Contains("/integraciones/jobs") && request.Method == HttpMethod.Post)
            {
                var payload = "{\"id\":\"00000000-0000-0000-0000-000000000020\",\"estado\":\"EnProceso\"}";
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                });
            }

            if (path.Contains("/integraciones/jobs/") && path.EndsWith("/retry", StringComparison.OrdinalIgnoreCase))
            {
                var payload = "{\"id\":\"00000000-0000-0000-0000-000000000021\",\"estado\":\"EnProceso\"}";
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                });
            }

            if (path.EndsWith("/empresas", StringComparison.OrdinalIgnoreCase) && request.Method == HttpMethod.Post)
            {
                var payload = "{\"id\":\"00000000-0000-0000-0000-000000000030\",\"nombre\":\"Demo\"}";
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                });
            }

            if (path.EndsWith("/legajos", StringComparison.OrdinalIgnoreCase) && request.Method == HttpMethod.Post)
            {
                var payload = "{\"id\":\"00000000-0000-0000-0000-000000000031\",\"numero\":\"200\"}";
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                });
            }

            if (path.EndsWith("/eventos", StringComparison.OrdinalIgnoreCase))
            {
                var payload = "[{\"id\":\"00000000-0000-0000-0000-000000000013\",\"tipo\":\"JobCompletado\"}]";
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                });
            }
            if (path.Contains("/exports/"))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(Encoding.UTF8.GetBytes("demo"))
                });
            }

            if (path.EndsWith("/definitions", StringComparison.OrdinalIgnoreCase))
            {
                var payload = "[{\"id\":\"00000000-0000-0000-0000-000000000001\",\"key\":\"vacaciones\",\"version\":\"1.0.0\",\"nombre\":\"Vacaciones\"}]";
                var status = request.Method == HttpMethod.Post ? HttpStatusCode.Created : HttpStatusCode.OK;
                return Task.FromResult(new HttpResponseMessage(status)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                });
            }

            if (path.EndsWith("/instances", StringComparison.OrdinalIgnoreCase))
            {
                var payload = "{\"id\":\"00000000-0000-0000-0000-000000000002\",\"estado\":\"Inicio\"}";
                var status = request.Method == HttpMethod.Post ? HttpStatusCode.Created : HttpStatusCode.OK;
                return Task.FromResult(new HttpResponseMessage(status)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                });
            }

            if (path.Contains("/instances/") && path.EndsWith("/transitions", StringComparison.OrdinalIgnoreCase))
            {
                var payload = "{\"estado\":\"Aprobacion\"}";
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }
}

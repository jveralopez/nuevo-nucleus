using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PersonalService.Tests;

[Collection("Sequential")]
public class PersonalHealthApiTests : IClassFixture<PersonalHealthApiTests.TestFactory>
{
    private readonly TestFactory _factory;

    public PersonalHealthApiTests(TestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    public sealed class TestFactory : WebApplicationFactory<Program>
    {
        public TestFactory()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), $"personal-tests-{Guid.NewGuid():N}.db");
            Environment.SetEnvironmentVariable("ConnectionStrings__PersonalDb", $"Data Source={dbPath}");
            Environment.SetEnvironmentVariable("Database__ApplyMigrations", "true");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Production");
            Environment.SetEnvironmentVariable("Auth__Issuer", "nucleus-auth");
            Environment.SetEnvironmentVariable("Auth__Audience", "nucleus-api");
            Environment.SetEnvironmentVariable("Auth__SigningKey", "CHANGE_ME_SUPER_SECRET_KEY_32_CHARS_LONG");
        }
    }
}

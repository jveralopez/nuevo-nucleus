using IntegrationHubService.Domain.Models;
using IntegrationHubService.Domain.Requests;
using IntegrationHubService.Services;
using IntegrationHubService.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationHubService.Tests;

public class IntegrationHubServiceTests
{
    [Fact]
    public async Task CreateTemplate_CreatesDraft()
    {
        var repo = new InMemoryIntegrationRepository();
        var service = new IntegrationHubService.Services.IntegrationHubService(repo, new FakeSecretProvider(), NullLogger<IntegrationHubService.Services.IntegrationHubService>.Instance);

        var request = new CreateTemplateRequest(
            "banco-galicia",
            "1.0.0",
            "0 4 * * *",
            new TemplateSource { Type = "sql", Connection = "rrhh-sql", Query = "./queries/galicia.sql" },
            new TemplateTransform { Type = "liquid", Template = "./templates/galicia.liquid" },
            new TemplateDestination { Type = "local", Connection = "", Path = "/out/{{fecha}}.txt" }
        );

        var template = await service.CreateTemplateAsync(request);

        Assert.Equal("Draft", template.Estado);
        Assert.Single(repo.Templates);
    }

    [Fact]
    public async Task PublishTemplate_UpdatesState()
    {
        var repo = new InMemoryIntegrationRepository();
        var service = new IntegrationHubService.Services.IntegrationHubService(repo, new FakeSecretProvider(), NullLogger<IntegrationHubService.Services.IntegrationHubService>.Instance);
        var template = await service.CreateTemplateAsync(new CreateTemplateRequest(
            "banco-galicia",
            "1.0.0",
            "0 4 * * *",
            new TemplateSource { Type = "sql", Connection = "rrhh-sql", Query = "./queries/galicia.sql" },
            new TemplateTransform { Type = "liquid", Template = "./templates/galicia.liquid" },
            new TemplateDestination { Type = "local", Connection = "", Path = "/out/{{fecha}}.txt" }
        ));

        var published = await service.PublishTemplateAsync(template.Id);

        Assert.NotNull(published);
        Assert.Equal("Publicado", published!.Estado);
    }

    [Fact]
    public async Task StartJob_GeneratesOutput()
    {
        var repo = new InMemoryIntegrationRepository();
        var service = new IntegrationHubService.Services.IntegrationHubService(repo, new FakeSecretProvider(), NullLogger<IntegrationHubService.Services.IntegrationHubService>.Instance);
        var template = await service.CreateTemplateAsync(new CreateTemplateRequest(
            "banco-galicia",
            "1.0.0",
            "0 4 * * *",
            new TemplateSource { Type = "sql", Connection = "rrhh-sql", Query = "./queries/galicia.sql" },
            new TemplateTransform { Type = "liquid", Template = "./templates/galicia.liquid" },
            new TemplateDestination { Type = "local", Connection = "", Path = "/out/{{fecha}}.txt" }
        ));

        var job = await service.StartJobAsync(new StartJobRequest(template.Id, "2026-02", "manual"));

        Assert.True(job.Estado == "Completado", job.Error ?? "Job falló");
        Assert.False(string.IsNullOrWhiteSpace(job.ArchivoGenerado));
    }
}

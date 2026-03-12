using IntegrationHubService.Domain.Models;
using IntegrationHubService.Domain.Requests;
using IntegrationHubService.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationHubService.Tests;

public class IntegrationTriggerTests
{
    [Fact]
    public async Task CreateTrigger_RequiresTemplate()
    {
        var repo = new InMemoryIntegrationRepository();
        var service = new Services.IntegrationHubService(repo, new FakeSecretProvider(), NullLogger<Services.IntegrationHubService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateTriggerAsync(new CreateTriggerRequest("LiquidacionFinalizada", Guid.NewGuid(), true)));
    }

    [Fact]
    public async Task ExecuteTrigger_CreatesJob()
    {
        var repo = new InMemoryIntegrationRepository();
        var service = new Services.IntegrationHubService(repo, new FakeSecretProvider(), NullLogger<Services.IntegrationHubService>.Instance);
        var template = await service.CreateTemplateAsync(new CreateTemplateRequest(
            "banco-demo",
            "1.0.0",
            "0 4 * * *",
            new TemplateSource { Type = "sql", Connection = "rrhh", Query = "query" },
            new TemplateTransform { Type = "liquid", Template = "tpl" },
            new TemplateDestination { Type = "sftp", Connection = "dest", Path = "/out/file.txt" }));

        var trigger = await service.CreateTriggerAsync(new CreateTriggerRequest("LiquidacionFinalizada", template.Id, true));
        var job = await service.ExecuteTriggerAsync(trigger.Id, new ExecuteTriggerRequest("2026-08", "manual"));

        Assert.NotNull(job);
        Assert.Equal(template.Id, job!.TemplateId);
    }
}

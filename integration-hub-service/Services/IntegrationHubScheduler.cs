using IntegrationHubService.Domain.Requests;
using NCrontab;

namespace IntegrationHubService.Services;

public class IntegrationHubScheduler : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SchedulerOptions _options;
    private readonly ILogger<IntegrationHubScheduler> _logger;

    public IntegrationHubScheduler(IServiceProvider serviceProvider, SchedulerOptions options, ILogger<IntegrationHubScheduler> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Scheduler deshabilitado");
            return;
        }

        var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.IntervalSeconds));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await RunScheduledJobsAsync();
                await RetryFailedJobsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Scheduler error");
            }
        }
    }

    private async Task RunScheduledJobsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IntegrationHubService>();
        var repo = scope.ServiceProvider.GetRequiredService<Infrastructure.IIntegrationRepository>();
        var templates = await repo.GetTemplatesAsync();

        var now = DateTimeOffset.UtcNow;
        foreach (var template in templates.Where(t => t.Estado == "Publicado"))
        {
            if (string.IsNullOrWhiteSpace(template.Schedule)) continue;
            var schedule = CrontabSchedule.Parse(template.Schedule);
            var last = template.LastRunAt?.UtcDateTime ?? now.UtcDateTime.AddMinutes(-1);
            var next = schedule.GetNextOccurrence(last);
            if (next <= now.UtcDateTime)
            {
                await service.StartJobAsync(new StartJobRequest(template.Id, now.ToString("yyyy-MM"), "scheduler"));
                template.LastRunAt = now;
                template.UpdatedAt = now;
                await repo.SaveTemplateAsync(template);
            }
        }
    }

    private async Task RetryFailedJobsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IntegrationHubService>();
        var repo = scope.ServiceProvider.GetRequiredService<Infrastructure.IIntegrationRepository>();
        var jobs = await repo.GetJobsAsync();
        var now = DateTimeOffset.UtcNow;

        foreach (var job in jobs.Where(j => j.Estado == "Fallido" && j.RetryCount < _options.MaxRetries))
        {
            job.RetryCount += 1;
            job.LastRetryAt = now;
            await repo.SaveJobAsync(job);
            await service.RetryJobAsync(job.Id, new RetryJobRequest("auto"));
        }
    }
}

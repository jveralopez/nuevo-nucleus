namespace IntegrationHubService.Services;

public class SchedulerOptions
{
    public bool Enabled { get; set; } = true;
    public int IntervalSeconds { get; set; } = 60;
    public int MaxRetries { get; set; } = 3;
}

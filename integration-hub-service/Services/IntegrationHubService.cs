using IntegrationHubService.Domain.Models;
using IntegrationHubService.Domain.Requests;
using IntegrationHubService.Infrastructure;

namespace IntegrationHubService.Services;

public class IntegrationHubService
{
    private readonly IIntegrationRepository _repository;
    private readonly ISecretProvider _secrets;
    private readonly ILogger<IntegrationHubService> _logger;

    public IntegrationHubService(IIntegrationRepository repository, ISecretProvider secrets, ILogger<IntegrationHubService> logger)
    {
        _repository = repository;
        _secrets = secrets;
        _logger = logger;
    }

    public Task<IReadOnlyCollection<IntegrationTemplate>> GetTemplatesAsync() => _repository.GetTemplatesAsync();

    public Task<IntegrationTemplate?> GetTemplateAsync(Guid id) => _repository.GetTemplateAsync(id);

    public async Task<IntegrationTemplate> CreateTemplateAsync(CreateTemplateRequest request)
    {
        ValidateRequired(request.Name, nameof(request.Name));
        ValidateRequired(request.Version, nameof(request.Version));
        ValidateRequired(request.Schedule, nameof(request.Schedule));
        ValidateComponent(request.Source.Type, nameof(request.Source.Type));
        ValidateComponent(request.Transform.Type, nameof(request.Transform.Type));
        ValidateComponent(request.Destination.Type, nameof(request.Destination.Type));

        var now = DateTimeOffset.UtcNow;
        var template = new IntegrationTemplate
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Version = request.Version.Trim(),
            Schedule = request.Schedule.Trim(),
            Source = request.Source,
            Transform = request.Transform,
            Destination = request.Destination,
            Estado = "Draft",
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repository.SaveTemplateAsync(template);
        return template;
    }

    public async Task<IntegrationTemplate?> UpdateTemplateAsync(Guid id, UpdateTemplateRequest request)
    {
        ValidateRequired(request.Schedule, nameof(request.Schedule));
        ValidateComponent(request.Source.Type, nameof(request.Source.Type));
        ValidateComponent(request.Transform.Type, nameof(request.Transform.Type));
        ValidateComponent(request.Destination.Type, nameof(request.Destination.Type));

        var template = await _repository.GetTemplateAsync(id);
        if (template is null) return null;

        template.Schedule = request.Schedule.Trim();
        template.Source = request.Source;
        template.Transform = request.Transform;
        template.Destination = request.Destination;
        template.Estado = string.IsNullOrWhiteSpace(request.Estado) ? template.Estado : request.Estado.Trim();
        template.UpdatedAt = DateTimeOffset.UtcNow;

        await _repository.SaveTemplateAsync(template);
        return template;
    }

    public async Task<IntegrationTemplate?> PublishTemplateAsync(Guid id)
    {
        var template = await _repository.GetTemplateAsync(id);
        if (template is null) return null;

        template.Estado = "Publicado";
        template.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.SaveTemplateAsync(template);
        return template;
    }

    public Task<IReadOnlyCollection<IntegrationConnection>> GetConnectionsAsync() => _repository.GetConnectionsAsync();

    public Task<IntegrationConnection?> GetConnectionAsync(Guid id) => _repository.GetConnectionAsync(id);

    public async Task<IntegrationConnection> CreateConnectionAsync(CreateConnectionRequest request)
    {
        ValidateRequired(request.Name, nameof(request.Name));
        ValidateRequired(request.Type, nameof(request.Type));
        ValidateRequired(request.Host, nameof(request.Host));
        ValidateRequired(request.Username, nameof(request.Username));
        ValidateRequired(request.SecretId, nameof(request.SecretId));

        var now = DateTimeOffset.UtcNow;
        var connection = new IntegrationConnection
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Type = request.Type.Trim(),
            Host = request.Host.Trim(),
            Username = request.Username.Trim(),
            SecretId = request.SecretId.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repository.SaveConnectionAsync(connection);
        return connection;
    }

    public Task<IReadOnlyCollection<IntegrationJob>> GetJobsAsync() => _repository.GetJobsAsync();

    public Task<IntegrationJob?> GetJobAsync(Guid id) => _repository.GetJobAsync(id);

    public Task<IReadOnlyCollection<IntegrationTrigger>> GetTriggersAsync(string? eventName) =>
        _repository.GetTriggersAsync(eventName);

    public Task<IntegrationTrigger?> GetTriggerAsync(Guid id) => _repository.GetTriggerAsync(id);

    public async Task<IntegrationTrigger> CreateTriggerAsync(CreateTriggerRequest request)
    {
        ValidateRequired(request.EventName, nameof(request.EventName));

        var template = await _repository.GetTemplateAsync(request.TemplateId);
        if (template is null)
        {
            throw new InvalidOperationException("Template inexistente");
        }

        var now = DateTimeOffset.UtcNow;
        var trigger = new IntegrationTrigger
        {
            Id = Guid.NewGuid(),
            EventName = request.EventName.Trim(),
            TemplateId = template.Id,
            Enabled = request.Enabled,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repository.SaveTriggerAsync(trigger);
        return trigger;
    }

    public async Task<IntegrationTrigger?> UpdateTriggerAsync(Guid id, CreateTriggerRequest request)
    {
        ValidateRequired(request.EventName, nameof(request.EventName));

        var trigger = await _repository.GetTriggerAsync(id);
        if (trigger is null) return null;

        trigger.EventName = request.EventName.Trim();
        trigger.TemplateId = request.TemplateId;
        trigger.Enabled = request.Enabled;
        trigger.UpdatedAt = DateTimeOffset.UtcNow;

        await _repository.SaveTriggerAsync(trigger);
        return trigger;
    }

    public async Task<IntegrationJob?> ExecuteTriggerAsync(Guid id, ExecuteTriggerRequest request)
    {
        ValidateRequired(request.Trigger, nameof(request.Trigger));

        var trigger = await _repository.GetTriggerAsync(id);
        if (trigger is null || !trigger.Enabled) return null;

        var job = await StartJobAsync(new StartJobRequest(trigger.TemplateId, request.Periodo, request.Trigger));
        await LogEventAsync(job, "TriggerEjecutado", trigger.EventName);
        return job;
    }

    public async Task<IntegrationJob> StartJobAsync(StartJobRequest request)
    {
        ValidateRequired(request.Trigger, nameof(request.Trigger));

        var template = await _repository.GetTemplateAsync(request.TemplateId);
        if (template is null)
        {
            throw new InvalidOperationException("Template inexistente");
        }

        var now = DateTimeOffset.UtcNow;
        var job = new IntegrationJob
        {
            Id = Guid.NewGuid(),
            TemplateId = template.Id,
            Trigger = request.Trigger.Trim(),
            Periodo = request.Periodo,
            Estado = "EnProceso",
            RetryCount = 0,
            CreatedAt = now,
            UpdatedAt = now
        };

        await LogEventAsync(job, "JobIniciado", $"Trigger={job.Trigger}");
        job = await ExecuteJobAsync(job, template);
        await _repository.SaveJobAsync(job);
        return job;
    }

    public async Task<IntegrationJob?> RetryJobAsync(Guid id, RetryJobRequest request)
    {
        var job = await _repository.GetJobAsync(id);
        if (job is null) return null;

        var template = await _repository.GetTemplateAsync(job.TemplateId);
        if (template is null)
        {
            throw new InvalidOperationException("Template inexistente");
        }

        job.Estado = "EnProceso";
        job.Error = null;
        job.RetryCount += 1;
        job.LastRetryAt = DateTimeOffset.UtcNow;
        job.UpdatedAt = DateTimeOffset.UtcNow;
        await LogEventAsync(job, "JobReintentado", request.Reason ?? "Manual");
        job = await ExecuteJobAsync(job, template);
        await _repository.SaveJobAsync(job);
        return job;
    }

    private async Task<IntegrationJob> ExecuteJobAsync(IntegrationJob job, IntegrationTemplate template)
    {
        try
        {
            var exportDir = Path.Combine(Directory.GetCurrentDirectory(), "storage", "exports");
            Directory.CreateDirectory(exportDir);
            var fileName = $"{template.Name}-{job.Id}.txt";
            var filePath = Path.Combine(exportDir, fileName);
            await File.WriteAllTextAsync(filePath, $"Template={template.Name}\nPeriodo={job.Periodo}\nEstado=OK");
            job.ArchivoGenerado = fileName;

            if (string.Equals(template.Destination.Type, "sftp", StringComparison.OrdinalIgnoreCase))
            {
                await UploadSftpAsync(template, filePath);
            }

            job.Estado = "Completado";
            await LogEventAsync(job, "JobCompletado", job.ArchivoGenerado ?? "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ejecutando job {JobId}", job.Id);
            job.Estado = "Fallido";
            job.Error = ex.Message;
            await LogEventAsync(job, "JobFallido", ex.Message);
        }

        job.UpdatedAt = DateTimeOffset.UtcNow;
        return job;
    }

    private Task LogEventAsync(IntegrationJob job, string tipo, string detalle)
    {
        var integrationEvent = new IntegrationEvent
        {
            Id = Guid.NewGuid(),
            JobId = job.Id,
            Tipo = tipo,
            Detalle = detalle,
            CreatedAt = DateTimeOffset.UtcNow
        };
        return _repository.SaveEventAsync(integrationEvent);
    }

    private async Task UploadSftpAsync(IntegrationTemplate template, string filePath)
    {
        var connection = await _repository.GetConnectionByNameAsync(template.Destination.Connection);
        if (connection is null)
        {
            throw new InvalidOperationException("Conexion SFTP inexistente");
        }

        if (string.IsNullOrWhiteSpace(connection.SecretId))
        {
            throw new InvalidOperationException("SecretId requerido para SFTP");
        }

        var password = await _secrets.GetSecretAsync(connection.SecretId);
        using var sftp = new Renci.SshNet.SftpClient(connection.Host, connection.Username, password);
        sftp.Connect();
        await using var fileStream = File.OpenRead(filePath);
        var remotePath = template.Destination.Path.Replace("{{fecha}}", DateTime.UtcNow.ToString("yyyyMMdd"));
        sftp.UploadFile(fileStream, remotePath, true);
        sftp.Disconnect();
    }

    private static void ValidateRequired(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{field} es requerido");
        }
    }

    private static void ValidateComponent(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{field} es requerido");
        }
    }
}

namespace NucleusWFService.Domain.Models;

public class WorkflowOperation
{
    public Guid Id { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public Guid InstanceId { get; set; }
    public string DefinitionKey { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? Evento { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

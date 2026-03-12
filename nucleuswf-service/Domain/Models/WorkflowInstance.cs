namespace NucleusWFService.Domain.Models;

public class WorkflowInstance
{
    public Guid Id { get; set; }
    public Guid DefinitionId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public Dictionary<string, string> Datos { get; set; } = new();
    public List<WorkflowHistoryEntry> Historial { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

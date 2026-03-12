namespace NucleusWFService.Domain.Models;

public class WorkflowDefinition
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string EstadoInicial { get; set; } = string.Empty;
    public List<WorkflowTransition> Transiciones { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

namespace NucleusWFService.Domain.Models;

public class WorkflowTransition
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string Evento { get; set; } = string.Empty;
}

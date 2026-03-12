namespace NucleusWFService.Domain.Models;

public class WorkflowHistoryEntry
{
    public DateTimeOffset Timestamp { get; set; }
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string Evento { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
}

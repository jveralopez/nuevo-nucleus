using NucleusWFService.Domain.Models;

namespace NucleusWFService.Infrastructure;

public class WorkflowStore
{
    public List<WorkflowDefinition> Definitions { get; set; } = new();
    public List<WorkflowInstance> Instances { get; set; } = new();
    public List<WorkflowOperation> Operations { get; set; } = new();
}

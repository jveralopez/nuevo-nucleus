using NucleusWFService.Domain.Models;

namespace NucleusWFService.Infrastructure;

public interface IWorkflowRepository
{
    Task<IReadOnlyCollection<WorkflowDefinition>> GetDefinitionsAsync();
    Task<WorkflowDefinition?> GetDefinitionAsync(Guid id);
    Task<WorkflowDefinition?> GetDefinitionByKeyAsync(string key, string version);
    Task SaveDefinitionAsync(WorkflowDefinition definition);

    Task<IReadOnlyCollection<WorkflowInstance>> GetInstancesAsync();
    Task<WorkflowInstance?> GetInstanceAsync(Guid id);
    Task SaveInstanceAsync(WorkflowInstance instance);

    Task<WorkflowOperation?> GetOperationAsync(string idempotencyKey, string operation);
    Task SaveOperationAsync(WorkflowOperation operation);
}

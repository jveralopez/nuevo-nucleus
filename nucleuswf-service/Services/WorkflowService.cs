using NucleusWFService.Domain.Models;
using NucleusWFService.Domain.Requests;
using NucleusWFService.Infrastructure;

namespace NucleusWFService.Services;

public class WorkflowService
{
    private readonly IWorkflowRepository _repository;
    private readonly ILogger<WorkflowService> _logger;

    public WorkflowService(IWorkflowRepository repository, ILogger<WorkflowService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public Task<IReadOnlyCollection<WorkflowDefinition>> GetDefinitionsAsync() => _repository.GetDefinitionsAsync();

    public Task<WorkflowDefinition?> GetDefinitionAsync(Guid id) => _repository.GetDefinitionAsync(id);

    public async Task<WorkflowDefinition> CreateDefinitionAsync(CreateDefinitionRequest request)
    {
        ValidateRequired(request.Key, nameof(request.Key));
        ValidateRequired(request.Version, nameof(request.Version));
        ValidateRequired(request.Nombre, nameof(request.Nombre));
        ValidateRequired(request.EstadoInicial, nameof(request.EstadoInicial));
        ValidateTransitions(request.Transiciones);

        var existing = await _repository.GetDefinitionByKeyAsync(request.Key, request.Version);
        if (existing is not null)
        {
            throw new InvalidOperationException("La definición ya existe");
        }

        var now = DateTimeOffset.UtcNow;
        var definition = new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            Key = request.Key.Trim(),
            Version = request.Version.Trim(),
            Nombre = request.Nombre.Trim(),
            EstadoInicial = request.EstadoInicial.Trim(),
            Transiciones = request.Transiciones,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repository.SaveDefinitionAsync(definition);
        return definition;
    }

    public async Task<WorkflowDefinition?> UpdateDefinitionAsync(Guid id, UpdateDefinitionRequest request)
    {
        ValidateRequired(request.Nombre, nameof(request.Nombre));
        ValidateRequired(request.EstadoInicial, nameof(request.EstadoInicial));
        ValidateTransitions(request.Transiciones);

        var definition = await _repository.GetDefinitionAsync(id);
        if (definition is null) return null;

        definition.Nombre = request.Nombre.Trim();
        definition.EstadoInicial = request.EstadoInicial.Trim();
        definition.Transiciones = request.Transiciones;
        definition.UpdatedAt = DateTimeOffset.UtcNow;

        await _repository.SaveDefinitionAsync(definition);
        return definition;
    }

    public Task<IReadOnlyCollection<WorkflowInstance>> GetInstancesAsync() => _repository.GetInstancesAsync();

    public Task<WorkflowInstance?> GetInstanceAsync(Guid id) => _repository.GetInstanceAsync(id);

    public async Task<WorkflowInstance> StartInstanceAsync(StartInstanceRequest request)
    {
        ValidateRequired(request.Key, nameof(request.Key));
        ValidateRequired(request.Version, nameof(request.Version));

        var definition = await _repository.GetDefinitionByKeyAsync(request.Key, request.Version);
        if (definition is null)
        {
            throw new InvalidOperationException("Definición no encontrada");
        }

        var now = DateTimeOffset.UtcNow;
        var instance = new WorkflowInstance
        {
            Id = Guid.NewGuid(),
            DefinitionId = definition.Id,
            Key = definition.Key,
            Version = definition.Version,
            Estado = definition.EstadoInicial,
            Datos = request.Datos ?? new Dictionary<string, string>(),
            CreatedAt = now,
            UpdatedAt = now
        };

        instance.Historial.Add(new WorkflowHistoryEntry
        {
            Timestamp = now,
            From = string.Empty,
            To = instance.Estado,
            Evento = "START",
            Actor = "system"
        });

        await _repository.SaveInstanceAsync(instance);
        return instance;
    }

    public async Task<WorkflowInstance?> ApplyTransitionAsync(Guid instanceId, TransitionRequest request)
    {
        ValidateRequired(request.Evento, nameof(request.Evento));

        var instance = await _repository.GetInstanceAsync(instanceId);
        if (instance is null) return null;

        var definition = await _repository.GetDefinitionAsync(instance.DefinitionId);
        if (definition is null)
        {
            _logger.LogWarning("Definición {DefinitionId} inexistente para instancia {InstanceId}", instance.DefinitionId, instanceId);
            throw new InvalidOperationException("Definición no encontrada");
        }

        var transition = definition.Transiciones.FirstOrDefault(t =>
            string.Equals(t.From, instance.Estado, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(t.Evento, request.Evento, StringComparison.OrdinalIgnoreCase));

        if (transition is null)
        {
            throw new InvalidOperationException("Transición inválida");
        }

        var from = instance.Estado;
        instance.Estado = transition.To;
        instance.UpdatedAt = DateTimeOffset.UtcNow;
        if (request.Datos is not null)
        {
            foreach (var kvp in request.Datos)
            {
                instance.Datos[kvp.Key] = kvp.Value;
            }
        }

        instance.Historial.Add(new WorkflowHistoryEntry
        {
            Timestamp = instance.UpdatedAt,
            From = from,
            To = instance.Estado,
            Evento = transition.Evento,
            Actor = string.IsNullOrWhiteSpace(request.Actor) ? "system" : request.Actor.Trim()
        });

        await _repository.SaveInstanceAsync(instance);
        return instance;
    }

    private static void ValidateRequired(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{field} es requerido");
        }
    }

    private static void ValidateTransitions(List<WorkflowTransition> transitions)
    {
        if (transitions.Count == 0)
        {
            throw new ArgumentException("Se requieren transiciones");
        }

        foreach (var transition in transitions)
        {
            if (string.IsNullOrWhiteSpace(transition.From) ||
                string.IsNullOrWhiteSpace(transition.To) ||
                string.IsNullOrWhiteSpace(transition.Evento))
            {
                throw new ArgumentException("Transiciones inválidas");
            }
        }
    }
}

using NucleusWFService.Domain.Models;

namespace NucleusWFService.Domain.Requests;

public record CreateDefinitionRequest(
    string Key,
    string Version,
    string Nombre,
    string EstadoInicial,
    List<WorkflowTransition> Transiciones);

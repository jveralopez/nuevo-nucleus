using NucleusWFService.Domain.Models;

namespace NucleusWFService.Domain.Requests;

public record UpdateDefinitionRequest(
    string Nombre,
    string EstadoInicial,
    List<WorkflowTransition> Transiciones);

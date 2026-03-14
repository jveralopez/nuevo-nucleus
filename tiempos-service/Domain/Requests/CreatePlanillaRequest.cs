namespace TiemposService.Domain.Requests;

public class CreatePlanillaRequest
{
    public string Periodo { get; set; } = string.Empty;
    public Guid EmpresaId { get; set; }
    public List<PlanillaDetalleRequest> Detalles { get; set; } = new();
}

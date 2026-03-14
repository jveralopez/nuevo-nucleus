namespace TiemposService.Domain.Models;

public class PlanillaHoras
{
    public Guid Id { get; set; }
    public string Periodo { get; set; } = string.Empty;
    public Guid EmpresaId { get; set; }
    public string Estado { get; set; } = "Borrador";
    public decimal TotalHoras { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<PlanillaDetalle> Detalles { get; set; } = new();
}

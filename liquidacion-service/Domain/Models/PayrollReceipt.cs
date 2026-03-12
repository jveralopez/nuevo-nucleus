namespace LiquidacionService.Domain.Models;

public class PayrollReceipt
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid PayrollRunId { get; init; }
    public Guid LegajoId { get; init; }
    public string LegajoNumero { get; init; } = string.Empty;
    public string LegajoNombre { get; init; } = string.Empty;
    public decimal Remunerativo { get; init; }
    public decimal Deducciones { get; init; }
    public decimal Neto { get; init; }
    public DateTime GeneradoEn { get; init; } = DateTime.UtcNow;
    public List<ReceiptDetail> Detalle { get; init; } = new();
}

public record ReceiptDetail(string Concepto, decimal Importe)
{
    public Guid Id { get; init; } = Guid.NewGuid();
}

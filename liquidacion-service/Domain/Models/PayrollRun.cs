using LiquidacionService.Domain.Requests;

namespace LiquidacionService.Domain.Models;

public class PayrollRun
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Periodo { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public PayrollStatus Estado { get; private set; } = PayrollStatus.Draft;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
    public List<LegajoEnLote> Legajos { get; } = new();
    public List<PayrollReceipt> Recibos { get; } = new();

    public static PayrollRun Create(NewPayrollRequest request) => new()
    {
        Periodo = request.Periodo,
        Tipo = request.Tipo,
        Descripcion = request.Descripcion,
        Estado = PayrollStatus.Draft
    };

    public void Touch() => UpdatedAt = DateTime.UtcNow;

    public void AddLegajo(LegajoEnLote legajo)
    {
        if (Estado != PayrollStatus.Draft)
        {
            throw new InvalidOperationException("Solo se pueden agregar legajos en estado Draft");
        }

        if (Legajos.Any(l => l.Numero.Equals(legajo.Numero, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("El legajo ya existe en la liquidación");
        }

        Legajos.Add(legajo);
        Touch();
    }

    public bool RemoveLegajo(Guid legajoId)
    {
        if (Estado != PayrollStatus.Draft)
        {
            throw new InvalidOperationException("Solo se pueden quitar legajos en estado Draft");
        }

        var removed = Legajos.RemoveAll(l => l.Id == legajoId) > 0;
        if (removed) Touch();
        return removed;
    }

    public void SetEstado(PayrollStatus nuevoEstado)
    {
        Estado = nuevoEstado;
        Touch();
    }

    public void SetRecibos(IEnumerable<PayrollReceipt> recibos)
    {
        Recibos.Clear();
        Recibos.AddRange(recibos);
        Touch();
    }
}

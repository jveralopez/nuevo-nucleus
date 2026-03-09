namespace LiquidacionService.Domain.Models;

public class LegajoEnLote
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Numero { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Cuil { get; init; } = string.Empty;
    public decimal Basico { get; init; }
    public decimal Antiguedad { get; init; }
    public decimal Adicionales { get; init; }
    public decimal Descuentos { get; init; }

    public static LegajoEnLote FromRequest(UpsertLegajoRequest request) => new()
    {
        Numero = request.Numero,
        Nombre = request.Nombre,
        Cuil = request.Cuil,
        Basico = request.Basico,
        Antiguedad = request.Antiguedad,
        Adicionales = request.Adicionales,
        Descuentos = request.Descuentos
    };
}

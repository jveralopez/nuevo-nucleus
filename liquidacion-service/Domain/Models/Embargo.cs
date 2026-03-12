namespace LiquidacionService.Domain.Models;

public class Embargo
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Tipo { get; init; } = "OC";
    public decimal? Porcentaje { get; init; }
    public decimal? MontoFijo { get; init; }
    public decimal? MontoTotal { get; init; }
    public decimal? MontoPendiente { get; init; }
    public string BaseCalculo { get; init; } = "Neto";
    public bool Activo { get; init; } = true;
}

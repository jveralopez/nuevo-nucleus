namespace LiquidacionService.Domain.Models;

public class LicenciaEnLote
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Tipo { get; init; } = string.Empty;
    public int Dias { get; init; }
    public bool ConGoce { get; init; } = true;
}

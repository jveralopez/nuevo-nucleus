namespace PersonalService.Domain.Models;

public class Licencia
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Tipo { get; set; } = string.Empty;
    public string? CodigoSIJP { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public bool ConGoce { get; set; } = true;
    public bool CuentaVacaciones { get; set; }
}

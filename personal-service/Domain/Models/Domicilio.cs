namespace PersonalService.Domain.Models;

public class Domicilio
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Tipo { get; set; } = string.Empty;
    public string Calle { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string? Piso { get; set; }
    public string? Depto { get; set; }
    public string Localidad { get; set; } = string.Empty;
    public string Provincia { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
    public string? CodigoPostal { get; set; }
    public string? Observaciones { get; set; }
    public Guid LegajoId { get; set; }
}

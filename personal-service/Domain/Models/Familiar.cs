namespace PersonalService.Domain.Models;

public class Familiar
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public DateTime? FechaNacimiento { get; set; }
    public bool Vive { get; set; } = true;
    public bool Discapacidad { get; set; }
    public bool ACargo { get; set; }
    public bool ACargoObraSocial { get; set; }
    public bool AplicaGanancias { get; set; }
}

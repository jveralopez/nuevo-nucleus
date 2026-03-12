namespace ConfiguracionService.Domain.Requests;

public class UpsertParametroRequest
{
    public string Clave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;
}

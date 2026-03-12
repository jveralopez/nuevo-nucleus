namespace ConfiguracionService.Domain.Requests;

public class CreateCatalogoItemRequest
{
    public string Tipo { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public string? MetadataJson { get; set; }
}

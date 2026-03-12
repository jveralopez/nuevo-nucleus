namespace OrganizacionService.Domain.Requests;

public class CreateOrganigramaVersionRequest
{
    public Guid EmpresaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

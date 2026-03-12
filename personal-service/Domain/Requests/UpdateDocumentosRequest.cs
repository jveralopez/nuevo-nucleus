namespace PersonalService.Domain.Requests;

public class UpdateDocumentosRequest
{
    public List<DocumentoPersonalRequest> Documentos { get; set; } = new();
}

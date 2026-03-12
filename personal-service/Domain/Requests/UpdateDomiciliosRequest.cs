namespace PersonalService.Domain.Requests;

public class UpdateDomiciliosRequest
{
    public List<DomicilioRequest> Domicilios { get; set; } = new();
}

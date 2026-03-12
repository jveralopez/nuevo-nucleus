namespace PersonalService.Domain.Requests;

public record UpdateLicenciasRequest(IReadOnlyCollection<LicenciaRequest> Licencias);

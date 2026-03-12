namespace PersonalService.Domain.Requests;

public record UpdateFamiliaresRequest(IReadOnlyCollection<FamiliarRequest> Familiares);

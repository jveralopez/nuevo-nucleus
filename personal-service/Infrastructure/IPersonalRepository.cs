using PersonalService.Domain.Models;

namespace PersonalService.Infrastructure;

public interface IPersonalRepository
{
    Task<IReadOnlyCollection<Legajo>> GetLegajosAsync();
    Task<Legajo?> GetLegajoAsync(Guid id);
    Task<Legajo?> GetLegajoByNumeroAsync(string numero);
    Task SaveLegajoAsync(Legajo legajo);
}

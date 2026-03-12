using PersonalService.Domain.Models;

namespace PersonalService.Infrastructure;

public class PersonalStore
{
    public List<Legajo> Legajos { get; set; } = new();
}

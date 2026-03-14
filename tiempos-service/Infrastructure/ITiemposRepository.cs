using TiemposService.Domain.Models;

namespace TiemposService.Infrastructure;

public interface ITiemposRepository
{
    Task<IReadOnlyCollection<Turno>> GetTurnosAsync();
    Task<Turno?> GetTurnoAsync(Guid id);
    Task<Turno?> GetTurnoByCodigoAsync(string codigo);
    Task SaveTurnoAsync(Turno turno);

    Task<IReadOnlyCollection<Horario>> GetHorariosAsync();
    Task<Horario?> GetHorarioAsync(Guid id);
    Task SaveHorarioAsync(Horario horario);

    Task<IReadOnlyCollection<Fichada>> GetFichadasAsync(Guid? legajoId, DateTimeOffset? desde, DateTimeOffset? hasta);
    Task<Fichada?> GetFichadaAsync(Guid id);
    Task SaveFichadaAsync(Fichada fichada);

    Task<IReadOnlyCollection<PlanillaHoras>> GetPlanillasAsync(Guid? empresaId, string? periodo);
    Task<PlanillaHoras?> GetPlanillaAsync(Guid id);
    Task SavePlanillaAsync(PlanillaHoras planilla);

    Task<IReadOnlyCollection<Ausencia>> GetAusenciasAsync(Guid? legajoId, string? legajoNumero, DateTimeOffset? desde, DateTimeOffset? hasta, string? tipo);
    Task SaveAusenciaAsync(Ausencia ausencia);
}

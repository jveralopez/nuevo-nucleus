namespace TiemposService.Domain.Models;

public record AusenciaResumenItem(string Key, int TotalRegistros, int TotalDias);

public record AusenciaResumen(int TotalRegistros, int TotalDias, IReadOnlyList<AusenciaResumenItem> PorTipo, IReadOnlyList<AusenciaResumenItem> PorLegajo);

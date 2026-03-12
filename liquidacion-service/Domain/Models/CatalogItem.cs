namespace LiquidacionService.Domain.Models;

public record CatalogItem(
    string? Codigo,
    string? Descripcion,
    string? GrupoTliq,
    string? TipoConcepto,
    bool? AcumGanancia,
    bool? FiguraRecibo,
    string? TipoAsiento,
    string? Secuencia,
    bool? Activo,
    bool? EjecutaAjuste,
    bool? Retroactivo,
    string? Source
);

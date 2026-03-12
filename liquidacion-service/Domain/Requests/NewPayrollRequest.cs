namespace LiquidacionService.Domain.Requests;

public record NewPayrollRequest
(
    string Periodo,
    string Tipo,
    string? Descripcion
);

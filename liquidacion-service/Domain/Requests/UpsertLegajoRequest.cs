namespace LiquidacionService.Domain.Requests;

public record UpsertLegajoRequest
(
    string Numero,
    string Nombre,
    string Cuil,
    decimal Basico,
    decimal Antiguedad,
    decimal Adicionales,
    decimal Descuentos
);

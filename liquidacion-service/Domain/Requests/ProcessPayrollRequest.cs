namespace LiquidacionService.Domain.Requests;

public record ProcessPayrollRequest(bool Exportar = false, bool AplicarVacacionesWorkflow = false);

using LiquidacionService.Domain.Models;

namespace LiquidacionService.Infrastructure;

public interface IPayrollRepository
{
    Task<IReadOnlyCollection<PayrollRun>> GetAllAsync();
    Task<PayrollRun?> GetAsync(Guid id);
    Task SaveAsync(PayrollRun payroll);
}

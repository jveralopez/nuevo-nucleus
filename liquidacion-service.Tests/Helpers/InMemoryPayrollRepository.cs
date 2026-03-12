using LiquidacionService.Domain.Models;
using LiquidacionService.Infrastructure;

namespace LiquidacionService.Tests.Helpers;

public class InMemoryPayrollRepository : IPayrollRepository
{
    public List<PayrollRun> Payrolls { get; } = new();

    public Task<IReadOnlyCollection<PayrollRun>> GetAllAsync() => Task.FromResult<IReadOnlyCollection<PayrollRun>>(Payrolls);

    public Task<PayrollRun?> GetAsync(Guid id) => Task.FromResult(Payrolls.FirstOrDefault(p => p.Id == id));

    public Task SaveAsync(PayrollRun payroll)
    {
        var index = Payrolls.FindIndex(p => p.Id == payroll.Id);
        if (index >= 0) Payrolls[index] = payroll; else Payrolls.Add(payroll);
        return Task.CompletedTask;
    }
}

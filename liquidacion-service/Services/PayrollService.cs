using LiquidacionService.Domain.Models;
using LiquidacionService.Domain.Requests;
using LiquidacionService.Infrastructure;

namespace LiquidacionService.Services;

public class PayrollService
{
    private readonly IPayrollRepository _repository;
    private readonly ReceiptCalculator _calculator;
    private readonly ReceiptExporter _exporter;
    private readonly ILogger<PayrollService> _logger;

    public PayrollService(IPayrollRepository repository, ReceiptCalculator calculator, ReceiptExporter exporter, ILogger<PayrollService> logger)
    {
        _repository = repository;
        _calculator = calculator;
        _exporter = exporter;
        _logger = logger;
    }

    public Task<IReadOnlyCollection<PayrollRun>> GetAllAsync() => _repository.GetAllAsync();

    public Task<PayrollRun?> GetAsync(Guid id) => _repository.GetAsync(id);

    public async Task<PayrollRun> CreateAsync(NewPayrollRequest request)
    {
        var payroll = PayrollRun.Create(request);
        await _repository.SaveAsync(payroll);
        return payroll;
    }

    public async Task<PayrollRun?> AddLegajoAsync(Guid payrollId, UpsertLegajoRequest request)
    {
        var payroll = await _repository.GetAsync(payrollId);
        if (payroll is null) return null;

        try
        {
            payroll.AddLegajo(LegajoEnLote.FromRequest(request));
            await _repository.SaveAsync(payroll);
            return payroll;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo agregar el legajo {Legajo} a la liquidación {PayrollId}", request.Numero, payrollId);
            throw;
        }
    }

    public async Task<bool> RemoveLegajoAsync(Guid payrollId, Guid legajoId)
    {
        var payroll = await _repository.GetAsync(payrollId);
        if (payroll is null) return false;

        try
        {
            var removed = payroll.RemoveLegajo(legajoId);
            if (removed)
            {
                await _repository.SaveAsync(payroll);
            }
            return removed;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo eliminar el legajo {LegajoId} de la liquidación {PayrollId}", legajoId, payrollId);
            throw;
        }
    }

    public async Task<PayrollRun?> ProcessAsync(Guid payrollId, bool exportar)
    {
        var payroll = await _repository.GetAsync(payrollId);
        if (payroll is null) return null;
        if (!payroll.Legajos.Any())
        {
            throw new InvalidOperationException("La liquidación no tiene legajos");
        }

        payroll.SetEstado(PayrollStatus.Calculando);
        await _repository.SaveAsync(payroll);

        var recibos = payroll.Legajos.Select(l => _calculator.BuildReceipt(payroll, l)).ToList();
        payroll.SetRecibos(recibos);
        payroll.SetEstado(PayrollStatus.Procesado);

        if (exportar)
        {
            await _exporter.ExportAsync(payroll);
            payroll.SetEstado(PayrollStatus.Exportado);
        }

        await _repository.SaveAsync(payroll);
        return payroll;
    }

    public async Task<IReadOnlyCollection<PayrollReceipt>?> GetRecibosAsync(Guid payrollId)
    {
        var payroll = await _repository.GetAsync(payrollId);
        return payroll?.Recibos;
    }
}

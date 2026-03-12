using LiquidacionService.Domain.Models;
using LiquidacionService.Domain.Requests;
using LiquidacionService.Infrastructure;

namespace LiquidacionService.Services;

public class PayrollService
{
    private readonly IPayrollRepository _repository;
    private readonly ReceiptCalculator _calculator;
    private readonly ReceiptExporter _exporter;
    private readonly IntegrationHubClient _integrationHub;
    private readonly WorkflowVacacionesClient _vacacionesClient;
    private readonly ILogger<PayrollService> _logger;

    public PayrollService(IPayrollRepository repository, ReceiptCalculator calculator, ReceiptExporter exporter, IntegrationHubClient integrationHub, WorkflowVacacionesClient vacacionesClient, ILogger<PayrollService> logger)
    {
        _repository = repository;
        _calculator = calculator;
        _exporter = exporter;
        _integrationHub = integrationHub;
        _vacacionesClient = vacacionesClient;
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

    public async Task UpdateAsync(PayrollRun payroll)
    {
        await _repository.SaveAsync(payroll);
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

    public async Task<PayrollRun?> ProcessAsync(Guid payrollId, ProcessPayrollRequest request)
    {
        var payroll = await _repository.GetAsync(payrollId);
        if (payroll is null) return null;
        if (!payroll.Legajos.Any())
        {
            throw new InvalidOperationException("La liquidación no tiene legajos");
        }

        if (request.AplicarVacacionesWorkflow)
        {
            await AplicarVacacionesDesdeWorkflowAsync(payroll);
        }

        payroll.SetEstado(PayrollStatus.Calculando);
        await _repository.SaveAsync(payroll);

        var recibos = payroll.Legajos.Select(l => _calculator.BuildReceipt(payroll, l)).ToList();
        payroll.SetRecibos(recibos);
        payroll.SetEstado(PayrollStatus.Procesado);

        if (request.Exportar)
        {
            await _exporter.ExportAsync(payroll);
            payroll.SetEstado(PayrollStatus.Exportado);

            try
            {
                var periodo = payroll.Periodo ?? string.Empty;
                await _integrationHub.TriggerJobAsync(periodo);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo disparar Integration Hub para la liquidación {PayrollId}", payrollId);
            }
        }

        await _repository.SaveAsync(payroll);
        return payroll;
    }

    private async Task AplicarVacacionesDesdeWorkflowAsync(PayrollRun payroll)
    {
        var instances = await _vacacionesClient.GetVacacionesAprobadasAsync();
        if (!instances.Any())
        {
            return;
        }

        foreach (var instance in instances)
        {
            if (!instance.Datos.TryGetValue("legajoNumero", out var legajoNumero))
            {
                continue;
            }

            if (!instance.Datos.TryGetValue("dias", out var diasRaw))
            {
                continue;
            }

            if (!int.TryParse(diasRaw, out var dias) || dias <= 0)
            {
                continue;
            }

            var legajo = payroll.Legajos.FirstOrDefault(l => string.Equals(l.Numero, legajoNumero, StringComparison.OrdinalIgnoreCase));
            if (legajo is null)
            {
                continue;
            }

            var actualizado = new LegajoEnLote
            {
                Id = legajo.Id,
                Numero = legajo.Numero,
                Nombre = legajo.Nombre,
                Cuil = legajo.Cuil,
                Convenio = legajo.Convenio,
                Categoria = legajo.Categoria,
                Basico = legajo.Basico,
                Antiguedad = legajo.Antiguedad,
                Adicionales = legajo.Adicionales,
                Descuentos = legajo.Descuentos,
                NoRemunerativo = legajo.NoRemunerativo,
                AplicaGanancias = legajo.AplicaGanancias,
                OmitirGanancias = legajo.OmitirGanancias,
                ConyugeACargo = legajo.ConyugeACargo,
                CantHijos = legajo.CantHijos,
                CantOtrosFamiliares = legajo.CantOtrosFamiliares,
                DeduccionesAdicionales = legajo.DeduccionesAdicionales,
                VacacionesDias = dias,
                Licencias = legajo.Licencias,
                Embargos = legajo.Embargos
            };

            payroll.RemoveLegajo(legajo.Id);
            payroll.AddLegajo(actualizado);
        }
    }

    public async Task<IReadOnlyCollection<PayrollReceipt>?> GetRecibosAsync(Guid payrollId)
    {
        var payroll = await _repository.GetAsync(payrollId);
        return payroll?.Recibos;
    }
}

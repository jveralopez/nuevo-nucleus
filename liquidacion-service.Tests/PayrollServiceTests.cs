using LiquidacionService.Domain.Models;
using LiquidacionService.Domain.Requests;
using LiquidacionService.Services;
using LiquidacionService.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LiquidacionService.Tests;

public class PayrollServiceTests
{
    [Fact]
    public async Task ProcessAsync_TriggersIntegrationHub_WhenExporting()
    {
        var repo = new InMemoryPayrollRepository();
        var env = new TestHostEnvironment();
        var rules = new ConceptRuleEngine(env);
        var ganancias = new GananciasCalculator(env);
        var calculator = new ReceiptCalculator(rules, ganancias);
        var exporter = new ReceiptExporter(env);
        var workflowClient = new WorkflowVacacionesClient(new HttpClient(), new WorkflowOptions());
        var handler = new TrackingHandler();
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5050") };
        var options = new IntegrationHubOptions
        {
            BaseUrl = "http://localhost:5050",
            TemplateId = Guid.NewGuid().ToString(),
            AccessToken = "token"
        };
        var integrationClient = new IntegrationHubClient(httpClient, options, NullLogger<IntegrationHubClient>.Instance);
        var service = new PayrollService(repo, calculator, exporter, integrationClient, workflowClient, NullLogger<PayrollService>.Instance);

        var payroll = await service.CreateAsync(new NewPayrollRequest("2026-02", "Mensual", "Test"));
        await service.AddLegajoAsync(payroll.Id, BuildLegajo("100", "Ana Perez", "20-00000000-0"));

        var processed = await service.ProcessAsync(payroll.Id, new ProcessPayrollRequest(true));

        Assert.NotNull(processed);
        Assert.Equal(1, handler.CallCount);
        Assert.Equal(PayrollStatus.Exportado, processed!.Estado);
    }

    [Fact]
    public async Task ProcessAsync_DoesNotTrigger_WhenTemplateMissing()
    {
        var repo = new InMemoryPayrollRepository();
        var env = new TestHostEnvironment();
        var rules = new ConceptRuleEngine(env);
        var ganancias = new GananciasCalculator(env);
        var calculator = new ReceiptCalculator(rules, ganancias);
        var exporter = new ReceiptExporter(env);
        var workflowClient = new WorkflowVacacionesClient(new HttpClient(), new WorkflowOptions());
        var handler = new TrackingHandler();
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5050") };
        var options = new IntegrationHubOptions
        {
            BaseUrl = "http://localhost:5050",
            TemplateId = "",
            AccessToken = ""
        };
        var integrationClient = new IntegrationHubClient(httpClient, options, NullLogger<IntegrationHubClient>.Instance);
        var service = new PayrollService(repo, calculator, exporter, integrationClient, workflowClient, NullLogger<PayrollService>.Instance);

        var payroll = await service.CreateAsync(new NewPayrollRequest("2026-03", "Mensual", "Test"));
        await service.AddLegajoAsync(payroll.Id, BuildLegajo("101", "Juan Gomez", "20-00000000-1"));

        var processed = await service.ProcessAsync(payroll.Id, new ProcessPayrollRequest(true));

        Assert.NotNull(processed);
        Assert.Equal(0, handler.CallCount);
        Assert.Equal(PayrollStatus.Exportado, processed!.Estado);
    }

    private static UpsertLegajoRequest BuildLegajo(string numero, string nombre, string cuil)
    {
        return new UpsertLegajoRequest(
            Numero: numero,
            Nombre: nombre,
            Cuil: cuil,
            Convenio: null,
            Categoria: null,
            Basico: 1000,
            Antiguedad: 100,
            Adicionales: 50,
            Presentismo: 0,
            HorasExtra: 0,
            Premios: 0,
            Descuentos: 10,
            NoRemunerativo: 0,
            BonosNoRemunerativos: 0,
            AplicaGanancias: true,
            OmitirGanancias: false,
            ConyugeACargo: false,
            CantHijos: 0,
            CantOtrosFamiliares: 0,
            DeduccionesAdicionales: 0,
            VacacionesDias: 0,
            Licencias: null,
            Embargos: null);
    }
}

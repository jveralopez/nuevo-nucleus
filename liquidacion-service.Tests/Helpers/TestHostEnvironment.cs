using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace LiquidacionService.Tests.Helpers;

public class TestHostEnvironment : IHostEnvironment
{
    public TestHostEnvironment()
    {
        ContentRootPath = Path.Combine(Path.GetTempPath(), "liquidacion-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(ContentRootPath);
    }

    public string EnvironmentName { get; set; } = Environments.Development;
    public string ApplicationName { get; set; } = "liquidacion-service-tests";
    public string ContentRootPath { get; set; }
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}

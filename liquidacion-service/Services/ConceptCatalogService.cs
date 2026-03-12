using System.Text.Json;
using LiquidacionService.Domain.Models;

namespace LiquidacionService.Services;

public class ConceptCatalogService
{
    private readonly string _catalogRoot;
    private readonly JsonSerializerOptions _jsonOptions;

    public ConceptCatalogService(IHostEnvironment env)
    {
        _catalogRoot = Path.Combine(env.ContentRootPath, "..", "data", "catalogos");
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public IReadOnlyCollection<CatalogItem> GetCatalog(string tipo)
    {
        var fileName = tipo.Equals("sicoss", StringComparison.OrdinalIgnoreCase)
            ? "conceptos_sicoss_23.01.json"
            : "conceptos_23.01.json";
        var path = Path.Combine(_catalogRoot, fileName);
        if (!File.Exists(path))
        {
            return Array.Empty<CatalogItem>();
        }

        using var stream = File.OpenRead(path);
        var payload = JsonSerializer.Deserialize<CatalogPayload>(stream, _jsonOptions);
        return payload?.Items ?? new List<CatalogItem>();
    }

    private sealed class CatalogPayload
    {
        public List<CatalogItem> Items { get; set; } = new();
    }
}

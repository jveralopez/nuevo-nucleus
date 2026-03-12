using System.Text.Json;
using System.Text.Json.Serialization;
using LiquidacionService.Domain.Models;

namespace LiquidacionService.Services;

public class ConceptRuleEngine
{
    private readonly string _rulesPath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ConceptRuleEngine(IHostEnvironment env)
    {
        _rulesPath = Path.Combine(env.ContentRootPath, "..", "data", "reglas", "conceptos", "reglas.json");
    }

    public IReadOnlyCollection<ConceptRule> GetRules(string? convenio)
    {
        var baseRules = LoadRules(_rulesPath);
        if (string.IsNullOrWhiteSpace(convenio))
        {
            return baseRules;
        }

        var convenioFile = Path.Combine(Path.GetDirectoryName(_rulesPath) ?? string.Empty, "convenios", NormalizeConvenio(convenio) + ".json");
        var convenioRules = LoadRules(convenioFile);
        if (convenioRules.Count == 0)
        {
            return baseRules;
        }

        var convenioFields = convenioRules
            .Where(r => r.Formula == ConceptFormulaType.LegajoField && !string.IsNullOrWhiteSpace(r.LegajoField))
            .Select(r => r.LegajoField!.ToLowerInvariant())
            .ToHashSet();

        var merged = baseRules
            .Where(r => r.Formula != ConceptFormulaType.LegajoField || !convenioFields.Contains((r.LegajoField ?? string.Empty).ToLowerInvariant()))
            .ToList();
        merged.AddRange(convenioRules);
        return merged;
    }

    private IReadOnlyCollection<ConceptRule> LoadRules(string path)
    {
        if (!File.Exists(path))
        {
            return Array.Empty<ConceptRule>();
        }

        using var stream = File.OpenRead(path);
        var payload = JsonSerializer.Deserialize<ConceptRulePayload>(stream, _jsonOptions);
        return payload?.Rules ?? new List<ConceptRule>();
    }

    private static string NormalizeConvenio(string convenio)
    {
        var trimmed = convenio.Trim().ToLowerInvariant();
        return trimmed.Replace(' ', '-').Replace('/', '-');
    }

    private sealed class ConceptRulePayload
    {
        public List<ConceptRule> Rules { get; set; } = new();
    }
}

using LiquidacionService.Domain.Requests;

namespace LiquidacionService.Domain.Models;

public class LegajoEnLote
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Numero { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Cuil { get; init; } = string.Empty;
    public string? Convenio { get; init; }
    public string? Categoria { get; init; }
    public decimal Basico { get; init; }
    public decimal Antiguedad { get; init; }
    public decimal Adicionales { get; init; }
    public decimal Presentismo { get; init; }
    public decimal HorasExtra { get; init; }
    public decimal Premios { get; init; }
    public decimal Descuentos { get; init; }
    public decimal NoRemunerativo { get; init; }
    public decimal BonosNoRemunerativos { get; init; }
    public bool AplicaGanancias { get; init; } = true;
    public bool OmitirGanancias { get; init; }
    public bool ConyugeACargo { get; init; }
    public int CantHijos { get; init; }
    public int CantOtrosFamiliares { get; init; }
    public decimal DeduccionesAdicionales { get; init; }
    public int VacacionesDias { get; init; }
    public List<LicenciaEnLote> Licencias { get; init; } = new();
    public List<Embargo> Embargos { get; init; } = new();

    public static LegajoEnLote FromRequest(UpsertLegajoRequest request) => new()
    {
        Numero = request.Numero,
        Nombre = request.Nombre,
        Cuil = request.Cuil,
        Convenio = request.Convenio,
        Categoria = request.Categoria,
        Basico = request.Basico,
        Antiguedad = request.Antiguedad,
        Adicionales = request.Adicionales,
        Presentismo = request.Presentismo,
        HorasExtra = request.HorasExtra,
        Premios = request.Premios,
        Descuentos = request.Descuentos,
        NoRemunerativo = request.NoRemunerativo,
        BonosNoRemunerativos = request.BonosNoRemunerativos,
        AplicaGanancias = request.AplicaGanancias,
        OmitirGanancias = request.OmitirGanancias,
        ConyugeACargo = request.ConyugeACargo,
        CantHijos = request.CantHijos,
        CantOtrosFamiliares = request.CantOtrosFamiliares,
        DeduccionesAdicionales = request.DeduccionesAdicionales,
        VacacionesDias = request.VacacionesDias,
        Licencias = request.Licencias?.Select(l => new LicenciaEnLote
        {
            Tipo = l.Tipo,
            Dias = l.Dias,
            ConGoce = l.ConGoce
        }).ToList() ?? new List<LicenciaEnLote>(),
        Embargos = request.Embargos?.Select(e => new Embargo
        {
            Tipo = e.Tipo,
            Porcentaje = e.Porcentaje,
            MontoFijo = e.MontoFijo,
            MontoTotal = e.MontoTotal,
            MontoPendiente = e.MontoPendiente,
            BaseCalculo = e.BaseCalculo,
            Activo = e.Activo
        }).ToList() ?? new List<Embargo>()
    };
}

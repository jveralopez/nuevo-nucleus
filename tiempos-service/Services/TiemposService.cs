using TiemposService.Domain.Models;
using TiemposService.Domain.Requests;
using TiemposService.Infrastructure;

namespace TiemposService.Services;

public class TiemposService
{
    private readonly ITiemposRepository _repository;
    private readonly ILogger<TiemposService> _logger;

    public TiemposService(ITiemposRepository repository, ILogger<TiemposService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public Task<IReadOnlyCollection<Turno>> GetTurnosAsync() => _repository.GetTurnosAsync();

    public Task<Turno?> GetTurnoAsync(Guid id) => _repository.GetTurnoAsync(id);

    public async Task<Turno> CreateTurnoAsync(CreateTurnoRequest request)
    {
        ValidateRequired(request.Codigo, nameof(request.Codigo));
        ValidateRequired(request.Nombre, nameof(request.Nombre));

        var existing = await _repository.GetTurnoByCodigoAsync(request.Codigo.Trim());
        if (existing is not null)
        {
            _logger.LogWarning("Turno {Codigo} ya existe", request.Codigo);
            throw new InvalidOperationException("El código de turno ya existe");
        }

        var now = DateTimeOffset.UtcNow;
        var turno = new Turno
        {
            Id = Guid.NewGuid(),
            Codigo = request.Codigo.Trim(),
            Nombre = request.Nombre.Trim(),
            HoraInicio = request.HoraInicio,
            HoraFin = request.HoraFin,
            ToleranciaMinutos = request.ToleranciaMinutos,
            Activo = request.Activo,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repository.SaveTurnoAsync(turno);
        return turno;
    }

    public async Task<Turno?> UpdateTurnoAsync(Guid id, UpdateTurnoRequest request)
    {
        ValidateRequired(request.Codigo, nameof(request.Codigo));
        ValidateRequired(request.Nombre, nameof(request.Nombre));

        var turno = await _repository.GetTurnoAsync(id);
        if (turno is null) return null;

        var existing = await _repository.GetTurnoByCodigoAsync(request.Codigo.Trim());
        if (existing is not null && existing.Id != id)
        {
            throw new InvalidOperationException("El código de turno ya existe");
        }

        turno.Codigo = request.Codigo.Trim();
        turno.Nombre = request.Nombre.Trim();
        turno.HoraInicio = request.HoraInicio;
        turno.HoraFin = request.HoraFin;
        turno.ToleranciaMinutos = request.ToleranciaMinutos;
        turno.Activo = request.Activo;
        turno.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.SaveTurnoAsync(turno);
        return turno;
    }

    public async Task<Turno?> DeactivateTurnoAsync(Guid id)
    {
        var turno = await _repository.GetTurnoAsync(id);
        if (turno is null) return null;
        turno.Activo = false;
        turno.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.SaveTurnoAsync(turno);
        return turno;
    }

    public Task<IReadOnlyCollection<Horario>> GetHorariosAsync() => _repository.GetHorariosAsync();

    public Task<Horario?> GetHorarioAsync(Guid id) => _repository.GetHorarioAsync(id);

    public async Task<Horario> CreateHorarioAsync(CreateHorarioRequest request)
    {
        ValidateRequired(request.Nombre, nameof(request.Nombre));
        ValidateRequired(request.DiasSemana, nameof(request.DiasSemana));
        await RequireTurnoAsync(request.TurnoId);

        var now = DateTimeOffset.UtcNow;
        var horario = new Horario
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre.Trim(),
            DiasSemana = request.DiasSemana.Trim(),
            TurnoId = request.TurnoId,
            Activo = request.Activo,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repository.SaveHorarioAsync(horario);
        return horario;
    }

    public async Task<Horario?> UpdateHorarioAsync(Guid id, UpdateHorarioRequest request)
    {
        ValidateRequired(request.Nombre, nameof(request.Nombre));
        ValidateRequired(request.DiasSemana, nameof(request.DiasSemana));
        await RequireTurnoAsync(request.TurnoId);

        var horario = await _repository.GetHorarioAsync(id);
        if (horario is null) return null;

        horario.Nombre = request.Nombre.Trim();
        horario.DiasSemana = request.DiasSemana.Trim();
        horario.TurnoId = request.TurnoId;
        horario.Activo = request.Activo;
        horario.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.SaveHorarioAsync(horario);
        return horario;
    }

    public async Task<Horario?> DeactivateHorarioAsync(Guid id)
    {
        var horario = await _repository.GetHorarioAsync(id);
        if (horario is null) return null;
        horario.Activo = false;
        horario.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.SaveHorarioAsync(horario);
        return horario;
    }

    public Task<IReadOnlyCollection<Fichada>> GetFichadasAsync(Guid? legajoId, DateTimeOffset? desde, DateTimeOffset? hasta) =>
        _repository.GetFichadasAsync(legajoId, desde, hasta);

    public Task<Fichada?> GetFichadaAsync(Guid id) => _repository.GetFichadaAsync(id);

    public async Task<Fichada> CreateFichadaAsync(CreateFichadaRequest request)
    {
        ValidateRequired(request.Tipo, nameof(request.Tipo));
        ValidateRequired(request.Origen, nameof(request.Origen));

        var now = DateTimeOffset.UtcNow;
        var fichada = new Fichada
        {
            Id = Guid.NewGuid(),
            LegajoId = request.LegajoId,
            FechaHora = request.FechaHora,
            Tipo = request.Tipo.Trim(),
            Origen = request.Origen.Trim(),
            Observaciones = request.Observaciones?.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repository.SaveFichadaAsync(fichada);
        return fichada;
    }

    public async Task<Fichada?> UpdateFichadaAsync(Guid id, UpdateFichadaRequest request)
    {
        var fichada = await _repository.GetFichadaAsync(id);
        if (fichada is null) return null;

        if (request.FechaHora.HasValue) fichada.FechaHora = request.FechaHora.Value;
        if (!string.IsNullOrWhiteSpace(request.Tipo)) fichada.Tipo = request.Tipo.Trim();
        if (!string.IsNullOrWhiteSpace(request.Origen)) fichada.Origen = request.Origen.Trim();
        if (request.Observaciones is not null) fichada.Observaciones = request.Observaciones.Trim();
        fichada.UpdatedAt = DateTimeOffset.UtcNow;

        await _repository.SaveFichadaAsync(fichada);
        return fichada;
    }

    public Task<IReadOnlyCollection<PlanillaHoras>> GetPlanillasAsync(Guid? empresaId, string? periodo) =>
        _repository.GetPlanillasAsync(empresaId, periodo);

    public Task<PlanillaHoras?> GetPlanillaAsync(Guid id) => _repository.GetPlanillaAsync(id);

    public async Task<PlanillaHoras> CreatePlanillaAsync(CreatePlanillaRequest request)
    {
        ValidateRequired(request.Periodo, nameof(request.Periodo));
        if (request.Detalles is null || request.Detalles.Count == 0)
        {
            throw new ArgumentException("Debe incluir al menos un detalle", nameof(request.Detalles));
        }

        var now = DateTimeOffset.UtcNow;
        var detalles = request.Detalles.Select(MapDetalle).ToList();
        var totalHoras = detalles.Sum(d => d.HorasNormales + d.HorasExtra - d.HorasAusencia);

        var planilla = new PlanillaHoras
        {
            Id = Guid.NewGuid(),
            Periodo = request.Periodo.Trim(),
            EmpresaId = request.EmpresaId,
            Estado = "Borrador",
            TotalHoras = totalHoras,
            CreatedAt = now,
            UpdatedAt = now,
            Detalles = detalles
        };

        foreach (var detalle in planilla.Detalles)
        {
            detalle.PlanillaId = planilla.Id;
        }

        await _repository.SavePlanillaAsync(planilla);
        return planilla;
    }

    public async Task<PlanillaHoras?> ClosePlanillaAsync(Guid id)
    {
        var planilla = await _repository.GetPlanillaAsync(id);
        if (planilla is null) return null;

        planilla.Estado = "Cerrada";
        planilla.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.SavePlanillaAsync(planilla);
        return planilla;
    }

    public Task<IReadOnlyCollection<Ausencia>> GetAusenciasAsync(Guid? legajoId, string? legajoNumero, DateTimeOffset? desde, DateTimeOffset? hasta, string? tipo) =>
        _repository.GetAusenciasAsync(legajoId, legajoNumero, desde, hasta, tipo);

    public async Task<Ausencia> CreateAusenciaAsync(CreateAusenciaRequest request)
    {
        ValidateRequired(request.LegajoNumero, nameof(request.LegajoNumero));
        ValidateRequired(request.Tipo, nameof(request.Tipo));

        if (request.FechaHasta < request.FechaDesde)
        {
            throw new ArgumentException("FechaHasta debe ser mayor o igual a FechaDesde", nameof(request.FechaHasta));
        }

        var now = DateTimeOffset.UtcNow;
        var ausencia = new Ausencia
        {
            Id = Guid.NewGuid(),
            LegajoId = request.LegajoId,
            LegajoNumero = request.LegajoNumero.Trim(),
            Tipo = request.Tipo.Trim(),
            FechaDesde = request.FechaDesde,
            FechaHasta = request.FechaHasta,
            Origen = string.IsNullOrWhiteSpace(request.Origen) ? "Medicina" : request.Origen.Trim(),
            Estado = "Aprobada",
            Observaciones = request.Observaciones?.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repository.SaveAusenciaAsync(ausencia);
        return ausencia;
    }

    public async Task<AusenciaResumen> GetAusenciasResumenAsync(Guid? legajoId, string? legajoNumero, DateTimeOffset? desde, DateTimeOffset? hasta, string? tipo)
    {
        var items = await _repository.GetAusenciasAsync(legajoId, legajoNumero, desde, hasta, tipo);
        var totalRegistros = items.Count;
        var totalDias = items.Sum(CalculateDias);

        var porTipo = items
            .GroupBy(i => i.Tipo)
            .Select(g => new AusenciaResumenItem(g.Key, g.Count(), g.Sum(CalculateDias)))
            .OrderByDescending(i => i.TotalDias)
            .ToList();

        var porLegajo = items
            .GroupBy(i => i.LegajoNumero)
            .Select(g => new AusenciaResumenItem(g.Key, g.Count(), g.Sum(CalculateDias)))
            .OrderByDescending(i => i.TotalDias)
            .Take(10)
            .ToList();

        return new AusenciaResumen(totalRegistros, totalDias, porTipo, porLegajo);
    }

    private async Task RequireTurnoAsync(Guid turnoId)
    {
        var turno = await _repository.GetTurnoAsync(turnoId);
        if (turno is null)
        {
            _logger.LogWarning("Turno {TurnoId} inexistente", turnoId);
            throw new InvalidOperationException("Turno inexistente");
        }
    }

    private static void ValidateRequired(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{field} es requerido");
        }
    }

    private static PlanillaDetalle MapDetalle(PlanillaDetalleRequest request) => new()
    {
        Id = Guid.NewGuid(),
        LegajoId = request.LegajoId,
        HorasNormales = request.HorasNormales,
        HorasExtra = request.HorasExtra,
        HorasAusencia = request.HorasAusencia,
        Observaciones = request.Observaciones?.Trim()
    };

    private static int CalculateDias(Ausencia ausencia)
    {
        var desde = ausencia.FechaDesde.Date;
        var hasta = ausencia.FechaHasta.Date;
        if (hasta < desde) return 0;
        return (int)(hasta - desde).TotalDays + 1;
    }
}

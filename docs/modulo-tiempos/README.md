# Modulo Tiempos Trabajados · Definicion inicial

## Objetivo
Gestionar fichadas, turnos y planillas para alimentar Liquidacion y RRHH.

## Alcance MVP
- ABM de turnos y horarios.
- Registro y consulta de fichadas.
- Generacion de planillas por periodo.
- Exporte de horas a Liquidacion.

## Modelo de datos (propuesto)
- **Turno**: Id, Codigo, Nombre, HoraInicio, HoraFin, ToleranciaMinutos, Activo, CreatedAt, UpdatedAt.
- **Horario**: Id, Nombre, DiasSemana, TurnoId, Activo, CreatedAt, UpdatedAt.
- **Fichada**: Id, LegajoId, FechaHora, Tipo (Entrada/Salida), Origen, Observaciones.
- **PlanillaHoras**: Id, Periodo, EmpresaId, Estado, TotalHoras, CreatedAt, UpdatedAt.
- **PlanillaDetalle**: Id, PlanillaId, LegajoId, HorasNormales, HorasExtra, Ausencias, Observaciones.

## Endpoints (propuestos)
- `GET /turnos`
- `POST /turnos`
- `PUT /turnos/{id}`
- `DELETE /turnos/{id}`

- `GET /horarios`
- `POST /horarios`
- `PUT /horarios/{id}`
- `DELETE /horarios/{id}`

- `GET /fichadas?legajoId=&desde=&hasta=`
- `POST /fichadas`

- `GET /planillas?periodo=&empresaId=`
- `POST /planillas` (generar planilla)
- `GET /planillas/{id}`
- `POST /planillas/{id}/cerrar`

- `GET /planillas/{id}/exportes`
- `POST /planillas/{id}/exportar` (push a Liquidacion)

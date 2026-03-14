# Modulo Tiempos Trabajados · Definicion inicial

## Objetivo
Gestionar fichadas, turnos, horarios y planillas para alimentar Liquidacion y RRHH.

## Alcance MVP
- ABM de turnos y horarios.
- Registro y consulta de fichadas.
- Generacion de planillas por periodo.
- Cierre de planillas.

## Modelo de datos (implementado)
- **Turno**: Id, Codigo, Nombre, HoraInicio, HoraFin, ToleranciaMinutos, Activo, CreatedAt, UpdatedAt.
- **Horario**: Id, Nombre, DiasSemana, TurnoId, Activo, CreatedAt, UpdatedAt.
- **Fichada**: Id, LegajoId, FechaHora, Tipo (Entrada/Salida), Origen, Observaciones.
- **PlanillaHoras**: Id, Periodo, EmpresaId, Estado, TotalHoras, CreatedAt, UpdatedAt.
- **PlanillaDetalle**: Id, PlanillaId, LegajoId, HorasNormales, HorasExtra, HorasAusencia, Observaciones.

## Endpoints (implementados)
- `GET /turnos`
- `POST /turnos`
- `PUT /turnos/{id}`
- `DELETE /turnos/{id}`

- `GET /horarios`
- `POST /horarios`
- `PUT /horarios/{id}`
- `DELETE /horarios/{id}`

- `GET /fichadas?legajoId=&desde=&hasta=`
- `GET /fichadas/{id}`
- `POST /fichadas`
- `PATCH /fichadas/{id}`

- `GET /planillas?periodo=&empresaId=`
- `POST /planillas` (generar planilla)
- `GET /planillas/{id}`
- `POST /planillas/{id}/cerrar`

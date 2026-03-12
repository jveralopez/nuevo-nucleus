# API propuesta · Tiempos Trabajados

## Ingesta de fichadas
- `POST /fichadas`
```json
{
  "legajoId": "LEG-001",
  "terminalId": "TERM-07",
  "evento": "ENTRADA",
  "timestamp": "2026-03-09T08:01:12Z",
  "fuente": "terminal"
}
```
- `POST /fichadas/import`
  - acepta CSV/JSON para cargas masivas (similar a `FichadasIng.Archivo`).
- `GET /fichadas?legajoId=&fechaDesde=&fechaHasta=&estado=`.
- `PATCH /fichadas/{id}` para correcciones (auditable).

## Horarios / turnos
- `GET /turnos`, `POST /turnos` (estructura de tramos horarios, descansos).
- `POST /horarios/asignaciones`
```json
{
  "legajoId": "LEG-001",
  "turnoId": "TURNO-NOCHE",
  "fechaDesde": "2026-03-01",
  "fechaHasta": "2026-03-31",
  "observaciones": "Cobertura nocturna"
}
```
- `POST /planillas/cambios-turno` (sube CSV -> procesa asíncronamente).

## Novedades / licencias
- `POST /novedades` (single) y `POST /novedades/import` (planilla según rol).
- `GET /novedades?legajoId=&estado=`.

## Procesamientos
- `POST /procesamientos`
```json
{
  "periodo": "2026-02",
  "descripcion": "Horas Febrero",
  "criterios": {
    "empresaId": "EMP01",
    "legajos": ["LEG-001", "LEG-014"],
    "incluirNovedades": true
  }
}
```
- `GET /procesamientos/{id}` → estado (`Draft`, `Calculando`, `Procesado`, `Exportado`).
- `POST /procesamientos/{id}/recalcular`.
- `GET /procesamientos/{id}/legajos/{legajoId}` → detalle de horas/conceptos.
- `GET /procesamientos/{id}/export?format=csv` → exporte tipo `ArchivoHoras`.

## Francos / banco de horas
- `GET /compensatorios?legajoId=`, `POST /compensatorios/movimientos`.
- `POST /compensatorios/{id}/aprobar` / `rechazar`.

## Catalogos / configuración
- `GET /catalogos/tipos-hora`, `GET /catalogos/tipos-novedad`, `GET /catalogos/terminales`.

## Eventos (Service Bus/Kafka)
- `FichadaRegistrada`, `FichadaCorregida`.
- `HorarioAsignado`, `HorarioActualizado`.
- `ProcesamientoCompletado` (payload incluye lista de legajos y resumen de conceptos).
- `CompensatorioGenerado`.

## Seguridad
- OAuth2/OIDC con scopes `tiempos.read`, `tiempos.write`, `tiempos.process`.
- Auditoría: cada modificación registra usuario, timestamp, origen (manual/terminal/batch).

---
*Basado en procesos descritos en `TTA.menu.xml` y el exporte `ArchivoHoras.XML`.*

# API propuesta · Vacaciones

Base path: `/api/vacaciones/v1`

## Simulador / saldos
- `GET /legajos/{legajoId}/saldos`
  - Respuesta: `{ periodo, diasDisponibles, diasTomados, diasBonificados, diasPendientes }`
- `POST /legajos/{legajoId}/simulador`
```json
{ "fechaDesde": "2026-02-10", "fechaHasta": "2026-02-20" }
```
- Devuelve cálculo (días hábiles, feriados, saldo resultante).

## Solicitudes
- `POST /solicitudes`
```json
{
  "legajoId": "LEG-001",
  "fechaDesde": "2026-02-10",
  "fechaHasta": "2026-02-20",
  "motivo": "Vacaciones verano",
  "tipo": "Ordinaria",
  "automatica": false
}
```
- `GET /solicitudes?legajoId=&estado=&aprobadorId=`
- `GET /solicitudes/{id}` / `DELETE` (cancelación antes de aprobar).

## Aprobaciones / workflow
- `POST /solicitudes/{id}/aprobar`
```json
{ "aprobadorId": "USR-100", "comentario": "Disfrutá" }
```
- `POST /solicitudes/{id}/rechazar`
- `POST /solicitudes/{id}/recalcular` (cambia fechas si la política lo permite).

## Políticas
- `GET /politicas?empresaId=&pais=`
- `POST /politicas` para definir reglas (antigüedad vs días, restricciones por temporada).

## Eventos
- `VacationRequested`, `VacationApproved`, `VacationRejected`, `VacationCancelled`.
- Payload: `{ requestId, legajoId, dias, periodo, estado }`.

## Seguridad
- Scopes: `vacaciones.read`, `vacaciones.write`, `vacaciones.approve`.
- Validar jerarquías (aprobador debe tener permisos sobre legajo).

---
*Basado en la API actual implícita en `Workflow Vacaciones`.*

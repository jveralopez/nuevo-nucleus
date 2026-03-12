# API propuesta · Seguridad física

Base path: `/api/seguridad/v1`

## Eventos de acceso
- `GET /accesos?legajoId=&zona=&fechaDesde=&fechaHasta=`
- `POST /accesos`
```json
{
  "legajoId": "LEG-001",
  "zona": "Planta Sur",
  "timestamp": "2026-03-24T08:01:00Z",
  "resultado": "Permitido",
  "dispositivo": "TURN-07"
}
```
- Recepción de eventos desde sistemas externos (Webhooks/API tokens).

## Permisos / badges
- `GET /permisos?legajoId=&zona=`
- `POST /permisos`
```json
{
  "legajoId": "LEG-120",
  "zona": "Data Center",
  "vigenciaDesde": "2026-03-25",
  "vigenciaHasta": "2026-03-26",
  "motivo": "Mantenimiento"
}
```
- `POST /permisos/{id}/aprobar`, `POST /permisos/{id}/revocar`.

## Alertas / incidentes
- `GET /alertas?estado=&tipo=`
- `POST /alertas`
```json
{
  "tipo": "Intrusión",
  "zona": "Dock",
  "descripcion": "Badge no autorizado",
  "nivel": "Alto"
}
```
- `POST /alertas/{id}/actualizar` (estado, notas).

## Rondas / checklists
- `GET /rondas?zona=&responsable=`
- `POST /rondas` (programar rondas, checklist digital).
- `POST /rondas/{id}/completar`.

## Eventos
- `AccessEventRecorded`, `SecurityAlertRaised`, `SecurityAlertClosed`, `PermitGranted`, `PermitRevoked`.

## Seguridad
- Scopes: `seguridad.read`, `seguridad.write`, `seguridad.admin`.
- Integración OIDC + permisos por zona.

---
*Blueprint para Seguridad.*

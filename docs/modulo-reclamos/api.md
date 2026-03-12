# API propuesta · Reclamos

Base path: `/api/reclamos/v1`

## Reclamos
- `POST /reclamos`
```json
{
  "titulo": "Error en recibo",
  "descripcion": "El bono no figura",
  "categoriaId": "CAT-011",
  "origenId": "ORI-PORTAL",
  "procesoId": "PROC-PAYROLL",
  "legajoId": "LEG-00341",
  "prioridad": "Alta",
  "adjuntos": [{"nombre": "recibo.pdf", "url": "..."}]
}
```
- `GET /reclamos?estado=&categoriaId=&legajoId=&asignadoA=` → filtros y paginación.
- `GET /reclamos/{id}` → detalle con timeline, comentarios y adjuntos.
- `PATCH /reclamos/{id}` para actualizar campos (p.ej. reasignar, cambiar prioridad).
- `POST /reclamos/{id}/comentarios` (mensajes, notas internas, tareas).
- `POST /reclamos/{id}/adjuntos` (upload a blob storage).

## Workflow / estados
- `POST /reclamos/{id}/acciones`
```json
{ "accion": "Clasificar", "payload": { "categoriaId": "CAT-011", "responsableId": "USR-21" } }
```
- `POST /reclamos/{id}/resolver` (adjunta solución, mensajes).
- `POST /reclamos/{id}/confirmar` (empleado confirma, estado => PendConf/Final).
- Webhook/Callback desde Workflow Engine para actualizar estado.

## Catálogos y configuración
- `GET /catalogos/reclamos/categorias`, `POST /catalogos/reclamos/categorias`.
- `GET /catalogos/reclamos/procesos`, `GET /catalogos/reclamos/origenes`.
- `GET /catalogos/reclamos/tipos-problema`, `GET /catalogos/reclamos/oficinas`.

## SLA y métricas
- `GET /sla/reclamos` (config actual), `POST /sla/reclamos` para actualizar thresholds.
- `GET /dashboard/reclamos?periodo=` → KPIs (abiertos, resueltos, NPS, SLA).

## Eventos
- `ReclamoCreado`, `ReclamoClasificado`, `ReclamoPendResol`, `ReclamoResuelto`, `ReclamoPendConf`, `ReclamoCerrado`.
- Payload: `{ reclamoId, estadoAnterior, estadoNuevo, categoriaId, legajoId, timestamp, usuario }`.

## Seguridad
- Scopes: `reclamos.read`, `reclamos.write`, `reclamos.admin`.
- Roles por actor: `Empleado` (solo sus reclamos), `MesaAyuda` (todos), `Aprobador` (según proceso), `Administrador` (catálogos/SLA).

---
*Inspirado en `lib_v11.WFReclamos.RECLAMO.cs` y `Reclamo.WF.xml`.*

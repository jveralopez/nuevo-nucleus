# API propuesta · Nucleus WF

## Definition API
- `POST /workflows`
```json
{
  "name": "CambioDatosPersonales",
  "version": "1.0.0",
  "description": "Autoservicio datos personales",
  "definition": { "states": [...], "forms": [...], "roles": [...] }
}
```
- `GET /workflows?state=published`
- `GET /workflows/{id}` / `PUT` / `DELETE`
- `POST /workflows/{id}/publish` (crea nueva versión activa).
- `POST /workflows/{id}/import-xml` (convierte `*.WF.xml` legados).
- `POST /workflows/{id}/forms` (definir formularios), `POST /workflows/{id}/roles` (mapear roles a organigrama).

## Runtime API
- `POST /instances`
```json
{
  "workflowId": "CambioDatosPersonales",
  "version": "1.0.0",
  "initiator": "legajo-001",
  "payload": { "legajoId": "LEG-001", "datos": {...} }
}
```
- `GET /instances?workflowId=&estado=&legajoId=`
- `GET /instances/{id}` → detalle, historial, tareas.
- `POST /instances/{id}/cancel`, `POST /instances/{id}/retry`.
- `GET /tasks?assignedTo=&workflowId=&estado=`
- `POST /tasks/{taskId}/complete`
```json
{ "payload": { "aprobado": true, "comentario": "Ok" } }
```
- `POST /tasks/{taskId}/delegate`, `POST /tasks/{taskId}/claim`.

## Catalogos / organigramas
- `GET /organigramas`, `POST /organigramas`.
- `POST /organigramas/{id}/roles`, `POST /organigramas/{id}/miembros`.

## Observabilidad
- `GET /metrics/workflows` (instancias activas, completadas, fallidas, SLA).
- `GET /logs/instances/{id}`.

## Eventos
- `WorkflowPublished`, `WorkflowDeprecated`.
- `WorkflowInstanceStarted`, `WorkflowInstanceCompleted`, `WorkflowInstanceFailed`.
- `WorkflowTaskAssigned`, `WorkflowTaskCompleted`.

## Seguridad
- OIDC + scopes: `wf.def.read`, `wf.def.write`, `wf.run.read`, `wf.run.write`, `wf.admin`.
- Multi-tenant: header/claim `tenantId` para aislar definiciones/instancias.

---
*Alineado con los artefactos `lib_v11.Workflows.*` y `lib_v11.Instancias.INSTANCE`.*

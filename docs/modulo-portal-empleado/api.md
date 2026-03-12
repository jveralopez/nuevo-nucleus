# API propuesta · Portal Empleado (BFF)

Base path: `/api/portal/v1`

## Home / dashboard
- `GET /home`
  - Devuelve widgets (avisos, tareas, KPIs, novedades) según configuración del usuario.
- `GET /widgets` (lista de widgets disponibles).
- `POST /home/widgets` (persistir layout preferido).

## Bandeja de tareas
- `GET /tasks?estado=&modulo=` → tareas provenientes de Nucleus WF.
- `POST /tasks/{taskId}/complete` -> delega al módulo correspondiente (Vacaciones/Reclamos/etc.).

## Atajos a servicios
- `GET /legajo/resumen` (via Personal API).
- `GET /vacaciones/saldo` (via Vacaciones API).
- `GET /tiempos/parte-diario`.
- `GET /liquidacion/recibos`.
- `GET /reclamos/resumen`.
- `GET /seleccion/avisos` (si el empleado también es candidato interno).

## MVP actual (llamadas directas desde UI)
- `GET /payrolls` y `GET /payrolls/{id}/recibos` (Liquidación).
- `GET /definitions`, `POST /definitions` (Nucleus WF).
- `GET /instances`, `POST /instances`, `POST /instances/{id}/transitions` (Nucleus WF).
- Notificaciones del dashboard se calculan desde estas respuestas (sin endpoint dedicado).
- Descarga de recibos se realiza en el browser (sin endpoint dedicado).
- Cache de notificaciones en localStorage (sin endpoint dedicado).

## BFF Notificaciones (persistentes)
- `GET /api/portal/v1/notificaciones`
- `GET /api/portal/v1/notificaciones?unreadOnly=true&limit=50&offset=0`
- `POST /api/portal/v1/notificaciones`
- `POST /api/portal/v1/notificaciones/{id}/read`
- `POST /api/portal/v1/notificaciones/read-all`
- `DELETE /api/portal/v1/notificaciones`
- `GET /api/portal/v1/notificaciones/resumen`

## BFF Exportes
- `GET /api/portal/v1/liquidacion/{id}/exports`
- `GET /api/portal/v1/liquidacion/exports/{fileName}`
- `GET /api/portal/v1/liquidacion/{id}/recibos`

## BFF Workflows
- `GET /api/portal/v1/wf/definitions`
- `POST /api/portal/v1/wf/definitions`
- `GET /api/portal/v1/wf/instances`
- `POST /api/portal/v1/wf/instances`
- `POST /api/portal/v1/wf/instances/{id}/transitions`

## BFF RH (core)
- Requiere rol `Admin`.
- `GET /api/rh/v1/organizacion/empresas`
- `POST /api/rh/v1/organizacion/empresas`
- `PUT /api/rh/v1/organizacion/empresas/{id}`
- `DELETE /api/rh/v1/organizacion/empresas/{id}`
- `GET /api/rh/v1/organizacion/unidades`
- `POST /api/rh/v1/organizacion/unidades`
- `PUT /api/rh/v1/organizacion/unidades/{id}`
- `DELETE /api/rh/v1/organizacion/unidades/{id}`
- `GET /api/rh/v1/organizacion/posiciones`
- `POST /api/rh/v1/organizacion/posiciones`
- `PUT /api/rh/v1/organizacion/posiciones/{id}`
- `DELETE /api/rh/v1/organizacion/posiciones/{id}`
- `POST /api/rh/v1/organizacion/posiciones/{id}/asignar`
- `GET /api/rh/v1/personal/legajos`
- `POST /api/rh/v1/personal/legajos`
- `PUT /api/rh/v1/personal/legajos/{id}`
- `DELETE /api/rh/v1/personal/legajos/{id}`
- `GET /api/rh/v1/liquidacion/payrolls`
- `POST /api/rh/v1/liquidacion/payrolls`
- `PATCH /api/rh/v1/liquidacion/payrolls/{id}`
- `POST /api/rh/v1/liquidacion/payrolls/{id}/legajos`
- `POST /api/rh/v1/liquidacion/payrolls/{id}/procesar`
- `GET /api/rh/v1/liquidacion/payrolls/{id}/recibos`
- `GET /api/rh/v1/liquidacion/payrolls/{id}/exports`
- `GET /api/rh/v1/integraciones/templates`
- `GET /api/rh/v1/integraciones/jobs`
- `GET /api/rh/v1/integraciones/jobs/{id}`
- `POST /api/rh/v1/integraciones/jobs`
- `POST /api/rh/v1/integraciones/jobs/{id}/retry`
- `GET /api/rh/v1/integraciones/eventos`
- `GET /api/rh/v1/wf/instances`
- `POST /api/rh/v1/wf/instances/{id}/transitions`

## Configuración
- `GET /config` (idioma, temas, notificaciones).
- `PUT /config` (actualizar preferencias).

## Notificaciones
- `GET /notificaciones` (sin leer), `POST /notificaciones/{id}/read`.
- Websocket/SignalR para tiempo real.

## Seguridad
- Autenticación OIDC (JWT). El BFF valida tokens y los reemite a servicios internos (Token exchange).
- Roles/claims determinan qué módulos se muestran.

---
*Este BFF actúa como gateway empaquetando múltiples servicios autoservicio.*

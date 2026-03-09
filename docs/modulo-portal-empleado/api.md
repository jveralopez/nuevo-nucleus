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

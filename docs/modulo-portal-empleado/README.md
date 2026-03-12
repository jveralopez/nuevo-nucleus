# Módulo Portal Empleado · Blueprint

## Objetivo
Modernizar el “Portal Empleado” descrito en `docs/10_portal_empleado.md` y apoyado por workflows (Vacaciones, Datos Personales, Reclamos) y WebCV, unificando la experiencia autoservicio (desktop/mobile) para empleados y managers.

## Alcance actual (23.01)
- Múltiples portales/parciales: WebCV (postulantes), formularios Nomad, reportes HTML, páginas ASP/ASPX.
- Funciones: consulta de legajo, recibos, solicitudes (vacaciones, datos personales), reclamos, avisos, reportes.
- Dependencia de Nomad UI/framesets (`WebCV`, `Html`, `Form`).

## Visión moderna
1. **Portal SPA (Next.js/React)** con Shell unificado (desktop + mobile), microfrontends por módulo (Vacaciones, Legajos, Tiempos, Reclamos, Integraciones, WebCV).
2. **BFF (Backend for Frontend)** que orquesta llamadas a servicios (Personal, Vacaciones, Tiempos, Liquidación, Selección, Nucleus WF, Integraciones).
3. **Identidad unificada** (OIDC) con SSO para empleados y managers, MFA opcional.
4. **Notificaciones y Bandeja de tareas** integradas con Nucleus WF.
5. **Observabilidad UX**: analítica, performance, feature flags.

## MVP implementado
- UI estática en `portal-empleado-ui/` con login JWT, consulta de liquidaciones y detalle de recibos.
- Workflows autoservicio: carga de definiciones demo y solicitudes (vacaciones, datos personales, reclamos).
- BFF MVP en `portal-bff-service/` con `/api/portal/v1/home` y `/api/portal/v1/liquidacion`.
- Notificaciones en dashboard generadas desde liquidaciones e instancias WF.
- Descarga local de recibos en JSON/CSV.
- Limpieza manual de notificaciones en UI.
- Notificaciones persistentes via Portal BFF.
- Descarga de exportes con JWT desde Liquidacion API.
- Workflows proxy via Portal BFF.

---
*Basado en `docs/10_portal_empleado.md` (no leído aún, se asumirá contenido similar), WebCV templates y workflows autoservicio.*

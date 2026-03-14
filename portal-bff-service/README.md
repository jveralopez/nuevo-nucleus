# Portal BFF Service

Backend for Frontend para Portal Empleado (MVP). Expone endpoints agregados y proxya Liquidacion API.

## Requisitos
- .NET SDK 8.0

## Seguridad
Requiere JWT emitido por `auth-service`.

## Configuracion
- `Portal.LiquidacionApi` en `appsettings.json`.
- `Portal.WfApi` en `appsettings.json`.
- `Portal.OrganizacionApi` en `appsettings.json`.
- `Portal.PersonalApi` en `appsettings.json`.
- `Portal.IntegrationHubApi` en `appsettings.json`.
- `Auth` con issuer/audience/signingKey.
- `ConnectionStrings.PortalBffDb` para persistencia de notificaciones.

## Seguridad
- Los endpoints `/api/rh/v1/*` requieren rol `Admin`.
- Rate limiting global (100 req/min).
- Swagger solo en Development.

## Endpoints
- `GET /health`
- `GET /api/portal/v1/home`
- `GET /api/portal/v1/liquidacion`
- `GET /api/portal/v1/notificaciones`
- `POST /api/portal/v1/notificaciones`
- `POST /api/portal/v1/notificaciones/{id}/read`
- `POST /api/portal/v1/notificaciones/read-all`
- `DELETE /api/portal/v1/notificaciones`
- `GET /api/portal/v1/notificaciones?unreadOnly=true&limit=50&offset=0`
- `GET /api/portal/v1/notificaciones/resumen`

### Sprint 14 - Recibos y Exports
- `GET /api/portal/v1/liquidacion/{id}/exports`
- `GET /api/portal/v1/liquidacion/exports/{fileName}`
- `GET /api/portal/v1/liquidacion/{id}/recibos`
- `GET /api/rh/v1/liquidacion/payrolls/{id}/recibos`
- `GET /api/rh/v1/liquidacion/payrolls/{id}/exports`

### Workflow
- `GET /api/portal/v1/wf/definitions`
- `POST /api/portal/v1/wf/definitions`
- `GET /api/portal/v1/wf/instances`
- `POST /api/portal/v1/wf/instances`
- `POST /api/portal/v1/wf/instances/{id}/transitions`
- `GET /api/rh/v1/wf/instances`
- `POST /api/rh/v1/wf/instances/{id}/transitions`

### Organizacion
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

### Personal
- `GET /api/rh/v1/personal/legajos`
- `POST /api/rh/v1/personal/legajos`
- `PUT /api/rh/v1/personal/legajos/{id}`
- `DELETE /api/rh/v1/personal/legajos/{id}`
- `GET /api/rh/v1/personal/legajos/{id}/domicilios`
- `GET /api/rh/v1/personal/legajos/{id}/documentos`

### Liquidacion
- `GET /api/rh/v1/liquidacion/payrolls`
- `POST /api/rh/v1/liquidacion/payrolls`
- `PATCH /api/rh/v1/liquidacion/payrolls/{id}`
- `POST /api/rh/v1/liquidacion/payrolls/{id}/legajos`
- `POST /api/rh/v1/liquidacion/payrolls/{id}/procesar`

### Tiempos
- `GET /api/rh/v1/tiempos/turnos`
- `POST /api/rh/v1/tiempos/turnos`
- `GET /api/rh/v1/tiempos/horarios`
- `POST /api/rh/v1/tiempos/horarios`
- `GET /api/rh/v1/tiempos/ausencias`
- `POST /api/rh/v1/tiempos/ausencias`
- `GET /api/rh/v1/tiempos/fichadas`
- `POST /api/rh/v1/tiempos/fichadas`
- `GET /api/rh/v1/tiempos/planillas`
- `POST /api/rh/v1/tiempos/planillas`

### Configuracion
- `GET /api/rh/v1/configuracion/catalogos/{tipo}`
- `POST /api/rh/v1/configuracion/catalogos`
- `GET /api/rh/v1/configuracion/parametros`

### Integraciones
- `GET /api/rh/v1/integraciones/templates`
- `GET /api/rh/v1/integraciones/jobs`
- `GET /api/rh/v1/integraciones/jobs/{id}`
- `POST /api/rh/v1/integraciones/jobs`
- `POST /api/rh/v1/integraciones/jobs/{id}/retry`
- `GET /api/rh/v1/integraciones/eventos`

### Medicina
- `GET /api/rh/v1/medicina/reportes`
- `GET /api/rh/v1/medicina/estadisticas`

### Tesoreria
- `GET /api/rh/v1/tesoreria/adelantos`
- `GET /api/rh/v1/tesoreria/pagos`
- `GET /api/rh/v1/tesoreria/conciliaciones`

### Presupuesto
- `GET /api/rh/v1/presupuesto/headcount`
- `GET /api/rh/v1/presupuesto/costos`

### Beneficios
- `GET /api/rh/v1/beneficios/catalogos`
- `GET /api/rh/v1/beneficios/inscripciones`

### Accidentabilidad
- `GET /api/rh/v1/accidentabilidad/incidentes`

### Seguridad
- `GET /api/rh/v1/seguridad/accesos`

### Control de Visitas
- `GET /api/rh/v1/control-visitas/visitas`

### Sprint 15 - Seleccion
- `GET /api/rh/v1/seleccion/candidates`
- `POST /api/rh/v1/seleccion/candidates`
- `GET /api/rh/v1/seleccion/candidates/{id}`
- `PUT /api/rh/v1/seleccion/candidates/{id}/estado`
- `GET /api/rh/v1/seleccion/avisos`
- `POST /api/rh/v1/seleccion/avisos`

### Sprint 15 - Evaluacion
- `GET /api/rh/v1/evaluacion/evaluaciones`
- `POST /api/rh/v1/evaluacion/evaluaciones`
- `GET /api/rh/v1/evaluacion/evaluaciones/{id}`
- `POST /api/rh/v1/evaluacion/evaluaciones/{id}/responder`
- `GET /api/rh/v1/evaluacion/metas`

### Sprint 15 - Carrera
- `GET /api/rh/v1/carrera/planes`
- `POST /api/rh/v1/carrera/planes`
- `GET /api/rh/carrera/succession`

### Sprint 16 - Capacitacion
- `GET /api/rh/v1/capacitacion/cursos`
- `GET /api/rh/v1/capacitacion/cursos/{id}`
- `GET /api/rh/v1/capacitacion/inscripciones`
- `POST /api/rh/v1/capacitacion/inscripciones`

### Sprint 16 - Clima Laboral
- `GET /api/rh/v1/clima/encuestas`
- `POST /api/rh/v1/clima/encuestas`
- `GET /api/rh/v1/clima/encuestas/{id}`
- `POST /api/rh/v1/clima/encuestas/{id}/responder`
- `GET /api/rh/v1/clima/resultados`

### Sprint 17 - Operations & Security
- `GET /api/rh/v1/auditoria/logs`
- `GET /api/rh/v1/auditoria/logs/{id}`
- `GET /api/rh/v1/seguridad/roles`
- `POST /api/rh/v1/seguridad/roles`
- `GET /api/rh/v1/seguridad/roles/{id}`
- `PUT /api/rh/v1/seguridad/roles/{id}`
- `GET /api/rh/v1/seguridad/usuarios`
- `POST /api/rh/v1/seguridad/usuarios`
- `POST /api/rh/v1/seguridad/usuarios/{id}/reset-password`
- `POST /api/rh/v1/seguridad/usuarios/{id}/activar`
- `POST /api/rh/v1/seguridad/usuarios/{id}/desactivar`
- `GET /api/rh/v1/seguridad/incidentes`
- `POST /api/rh/v1/seguridad/incidentes`
- `GET /api/rh/v1/seguridad/compliance`

### Sprint 18 - Final Integration & Testing
- `GET /api/rh/v1/sistema/health`
- `GET /api/rh/v1/sistema/metricas`
- `GET /api/rh/v1/sistema/integraciones`
- `POST /api/rh/v1/sistema/integraciones/{nombre}/sync`
- `GET /api/rh/v1/test/health-check`
- `POST /api/rh/v1/test/seed`

### Sprint 19 - Production Release
- `GET /api/rh/v1/sistema/info`
- `GET /api/rh/v1/sistema/configuracion`
- `PUT /api/rh/v1/sistema/configuracion/{clave}`
- `GET /api/rh/v1/sistema/backup`
- `POST /api/rh/v1/sistema/backup`
- `POST /api/rh/v1/sistema/backup/{id}/restore`
- `GET /api/rh/v1/sistema/mantenimiento`
- `POST /api/rh/v1/sistema/mantenimiento`
- `GET /api/rh/v1/dashboard/resumen`

## Tests
```bash
dotnet test ..\portal-bff-service.Tests\portal-bff-service.Tests.csproj
```

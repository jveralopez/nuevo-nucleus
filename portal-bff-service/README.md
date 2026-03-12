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
- `GET /api/portal/v1/liquidacion/{id}/exports`
- `GET /api/portal/v1/liquidacion/exports/{fileName}`
- `GET /api/portal/v1/liquidacion/{id}/recibos`
- `GET /api/portal/v1/wf/definitions`
- `POST /api/portal/v1/wf/definitions`
- `GET /api/portal/v1/wf/instances`
- `POST /api/portal/v1/wf/instances`
- `POST /api/portal/v1/wf/instances/{id}/transitions`
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

## Tests
```bash
dotnet test ..\portal-bff-service.Tests\portal-bff-service.Tests.csproj
```

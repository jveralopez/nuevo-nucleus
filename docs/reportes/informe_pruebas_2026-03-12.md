# Informe de pruebas · 2026-03-12

## Alcance
- Tests unitarios/integracion (dotnet test).
- Build de servicios principales.
- Diagnosticos LSP en archivos modificados.
- Pruebas E2E manuales en navegador (Portal RH y Portal Empleado).

## Entorno
- OS: Windows 10 (build 10.0.26200)
- .NET SDK: 8.0.419, 10.0.200 (instalado en perfil de usuario)
- Servicios activos: auth-service, organizacion-service, personal-service, liquidacion-service, integration-hub-service, portal-bff-service, nucleuswf-service
- UIs: portal-rh-ui (http://localhost:3001), portal-empleado-ui (http://localhost:3002)

## Health checks
- auth-service: OK (200)
- organizacion-service: OK (200)
- personal-service: OK (200)
- liquidacion-service: OK (200)
- integration-hub-service: OK (200)
- nucleuswf-service: OK (200)
- portal-bff-service: OK (200)

## Builds
- `dotnet build auth-service/auth-service.csproj` OK
- `dotnet build organizacion-service/organizacion-service.csproj` OK
- `dotnet build personal-service/personal-service.csproj` OK
- `dotnet build liquidacion-service/liquidacion-service.csproj` OK
- `dotnet build integration-hub-service/integration-hub-service.csproj` OK
- `dotnet build portal-bff-service/portal-bff-service.csproj` OK
- `dotnet build nucleuswf-service/nucleuswf-service.csproj` OK

## Tests (dotnet test)
- `integration-hub-service.Tests` OK (5/5)
- `portal-bff-service.Tests` OK (7/7)
- `liquidacion-service.Tests` OK (7/7)

## LSP diagnostics
- `liquidacion-service/Domain/Models/PayrollReceipt.cs`: sin diagnosticos
- `liquidacion-service/Infrastructure/PayrollDbContext.cs`: sin diagnosticos
- `liquidacion-service/Infrastructure/EfPayrollRepository.cs`: sin diagnosticos
- `liquidacion-service.Tests/PayrollServiceTests.cs`: sin diagnosticos
- `liquidacion-service.Tests/LiquidacionExportsApiTests.cs`: sin diagnosticos

## E2E Portal RH (http://localhost:3001)
- Login demo: OK (token cargado en UI)
- Acciones en Organización:
  - Crear empresa: FALLA (401 Unauthorized en `http://localhost:5100/empresas`)
- Acciones en Personal:
  - Crear legajo: FALLA (401 Unauthorized en `http://localhost:5200/legajos`)
- Acciones en Liquidación:
  - Crear liquidación: FALLA (401 Unauthorized en `http://localhost:5188/payrolls`)
- Integraciones:
  - Templates/Jobs/Triggers: FALLA (404 en `http://localhost:5050/templates`, `http://localhost:5050/jobs`, `http://localhost:5050/triggers`)

## E2E Portal Empleado (http://localhost:3002)
- Login demo (Auth): FALLA por CORS al llamar `http://localhost:5001/login` desde UI.
- BFF activado (default 5090): FALLA por CORS en `http://localhost:5090/api/portal/v1/*`.
- BFF desactivado (directo a APIs): FALLA por CORS en `http://localhost:5188/payrolls` y `http://localhost:5051/instances`.

## Conclusiones
- Las suites de pruebas y builds pasan sin errores.
- Los E2E quedan bloqueados por autorizacion (Portal RH) y CORS (Portal Empleado).
- Integration Hub responde health, pero endpoints de templates/jobs/triggers devuelven 404.

## Actualizacion (2026-03-12)
### Cambios aplicados
- Integration Hub: endpoint `/integraciones/triggers` ahora consulta repositorio y no rompe por `DateTimeOffset` en SQLite.
- Liquidacion: se asegura esquema para tablas/columnas de legajos, embargos, licencias y detalle de recibos.
- Organizacion: se crean tablas faltantes `Sindicatos` y `Convenios`.
- Personal: se crean tablas faltantes `Familiares` y `Licencias`, y columnas faltantes en `Legajos`.
- `.env`: CORS actualizado a `http://localhost:3001,http://localhost:3002`.

### Entorno y puertos usados (evitar colision con Docker)
- auth-service: http://localhost:6001
- organizacion-service: http://localhost:6100
- personal-service: http://localhost:6200
- liquidacion-service: http://localhost:6188
- nucleuswf-service: http://localhost:6051
- integration-hub-service: http://localhost:5052
- portal-rh-ui: http://localhost:3001
- portal-empleado-ui: http://localhost:3002

### Builds
- `dotnet build integration-hub-service/integration-hub-service.csproj` OK
- `dotnet build liquidacion-service/liquidacion-service.csproj` OK
- `dotnet build organizacion-service/organizacion-service.csproj` OK
- `dotnet build personal-service/personal-service.csproj` OK

### Tests (dotnet test)
- `integration-hub-service.Tests` OK (5/5)
- `liquidacion-service.Tests` OK (7/7)

### E2E Portal RH (http://localhost:3001)
- Configuracion de APIs a puertos alternativos (6001/6100/6200/6188/6051/5052).
- Login demo: OK.
- Refrescar datos: OK.
- Requests verificados (200): `/empresas`, `/unidades`, `/posiciones`, `/sindicatos`, `/convenios`, `/legajos`, `/payrolls`, `/integraciones/jobs`, `/integraciones/templates`, `/integraciones/triggers`, `/instances`.

### E2E Portal Empleado (http://localhost:3002)
- Configuracion de APIs a puertos alternativos (6001/6188/6051). BFF deshabilitado.
- Login demo: OK.
- Refrescar datos: OK.
- Requests verificados (200): `/payrolls`, `/definitions`, `/instances`.

### Observaciones
- Se observan errores de favicon (404) en ambos portales (no afecta funcionalidad).
- En Portal Empleado, quedan errores previos por BFF en consola del navegador antes de deshabilitarlo; luego las llamadas directas quedaron OK.

## Actualizacion (2026-03-13)
- CORS actualizado en servicios y Portal BFF para permitir `Origin: null` en Development y orígenes configurados.
- Build y tests de `nuevo-nucleus.sln` ejecutados OK en local.
- Se robustecio el parseo de `OpenTelemetry:Enabled` y `Database:ApplyMigrations` para valores vacios en docker compose.
- Docker compose prod levantado. Smoke test de health y auth OK:
  - `/health` OK: auth (5001), organizacion (5100), personal (5200), liquidacion (5188), integration-hub (5050), nucleuswf (5051), portal-bff (5090), configuracion (5300), tiempos (5400).
  - Login `POST /login` OK y `/me` OK con JWT.
- E2E Playwright OK: Portal RH y Portal Empleado (login demo + refresh sin 401).
- Sprint 3 (MVP) completado: Reclamos y Sanciones con workflows y UI.
- Script de automatización E2E: `start-e2e.ps1`.
- WF: auditoría de transiciones (actor/rol/correlation/idempotency) e idempotencia por operación.
- Servicios core: middleware de `X-Correlation-Id` e `Idempotency-Key` cuando no vienen en la solicitud.
- Liquidación: soporte de contribuciones patronales por legajo/recibo (detalle + total).
- Portal BFF: logging incluye CorrelationId e IdempotencyKey.
- Servicios core: logging incluye CorrelationId e IdempotencyKey.
- Medicina laboral: workflows base y UI operativos (solicitudes + aprobaciones).
- WF: resumen de payloads sensibles con redacción en auditoría.
- Tiempos: endpoint de ausencias usado para licencias médicas aprobadas.
- Medicina: notificación automática al aprobar licencia (Portal Empleado).
- Tiempos: resumen de ausencias médicas disponible.
- Medicina: notificaciones por aprobación/rechazo de exámenes.
- Portal RH: filtro por fechas en resumen de ausencias médicas.

### Observabilidad (OTEL)
- Stack levantado con `docker-compose.prod.yml` (otel-collector, jaeger, prometheus).
- Jaeger UI: HTTP 200 en `http://localhost:16686`.
- Prometheus UI: HTTP 302 en `http://localhost:9090`.
- Metrics OTEL: HTTP 200 en `http://localhost:8889/metrics`.
- Jaeger API `/api/services` responde solo `jaeger-all-in-one` (pendiente de visibilidad de trazas de servicios).
- OTEL configurado con `OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4318`, `OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf`, `OTEL_TRACES_SAMPLER=always_on`.

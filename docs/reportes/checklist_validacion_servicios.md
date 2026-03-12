# Checklist de validacion de servicios

## Smoke test
- [x] `docker compose up -d` ejecutado
- [x] `docker compose ps` muestra servicios `healthy`
- [x] `/health` OK en auth, organizacion, personal, liquidacion, integration-hub, nucleuswf, portal-bff, configuracion
- [x] Login `POST /login` OK
- [x] Endpoint protegido `/me` OK

## Migraciones
- [x] DB vacia: `dotnet ef database update` (organizacion-service)
- [x] DB con datos: insert + update sin perdida (organizacion-service)
- [ ] `__EFMigrationsHistory` actualizado y sin errores (resto servicios)

## CI
- [ ] build-test en PR: falla por `dotnet restore` sin solution en root

## Portales
- [x] Portal RH archivos estaticos presentes
- [x] Portal RH carga (visual)
- [x] Portal RH lista empresas (200, sin datos)
- [x] Organigrama versionado disponible (200, sin datos)
- [x] Portal Empleado archivos estaticos presentes
- [x] Portal Empleado carga (visual)
- [x] Portal Empleado carga recibos/demo (200, sin datos)
- [x] Liquidacion UI carga datos (200, sin datos)

## BFF
- [x] BFF responde `/health`
- [x] `GET /api/rh/v1/organizacion/organigramas`
- [x] `GET /api/rh/v1/organizacion/unidades/tree`
- [x] `POST /api/rh/v1/organizacion/organigramas`

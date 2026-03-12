# Checklist de validacion de servicios

## Smoke test
- [x] `docker compose up -d` ejecutado
- [x] `docker compose ps` muestra servicios `healthy`
- [x] `/health` OK en auth, organizacion, personal, liquidacion, integration-hub, nucleuswf, portal-bff, configuracion
- [x] Login `POST /login` OK
- [x] Endpoint protegido `/me` OK

## Migraciones
- [ ] DB vacia: `dotnet ef database update` por servicio
- [ ] DB con datos: backup + baseline si aplica + update
- [ ] `__EFMigrationsHistory` actualizado y sin errores

## Portales
- [ ] Portal RH carga y lista empresas
- [ ] Organigrama versionado disponible
- [ ] Portal Empleado carga recibos demo

## BFF
- [x] BFF responde `/health`
- [x] `GET /api/rh/v1/organizacion/organigramas`
- [x] `GET /api/rh/v1/organizacion/unidades/tree`
- [x] `POST /api/rh/v1/organizacion/organigramas`

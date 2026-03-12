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
- [x] Portal RH archivos estaticos presentes
- [ ] Portal RH carga y lista empresas (verificacion visual)
- [ ] Organigrama versionado disponible (verificacion visual)
- [x] Portal Empleado archivos estaticos presentes
- [ ] Portal Empleado carga recibos demo (verificacion visual)

## BFF
- [x] BFF responde `/health`
- [x] `GET /api/rh/v1/organizacion/organigramas`
- [x] `GET /api/rh/v1/organizacion/unidades/tree`
- [x] `POST /api/rh/v1/organizacion/organigramas`

# Checklist de validacion de servicios

## Smoke test
- [ ] `docker compose up -d` ejecutado
- [ ] `docker compose ps` muestra servicios `healthy`
- [ ] `/health` OK en auth, organizacion, personal, liquidacion, integration-hub, nucleuswf, portal-bff, configuracion
- [ ] Login `POST /login` OK
- [ ] Endpoint protegido `/me` OK

## Migraciones
- [ ] DB vacia: `dotnet ef database update` por servicio
- [ ] DB con datos: backup + baseline si aplica + update
- [ ] `__EFMigrationsHistory` actualizado y sin errores

## Portales
- [ ] Portal RH carga y lista empresas
- [ ] Organigrama versionado disponible
- [ ] Portal Empleado carga recibos demo

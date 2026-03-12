# Produccion · Checklist

## Infra
- Docker + docker-compose.prod.yml
- Variables en `.env.prod` (ver `.env.prod.example`)
- Volumenes para SQLite
- Servicio Configuracion: `configuracion-service` en puerto 5300

## CI/CD
- Workflow base en `.github/workflows/ci.yml` (restore/build/test)
- Recomendado: agregar job de deploy y versionado de imagenes si usan registro
- Release: `.github/workflows/release.yml` publica imagenes en GHCR con tag `v*`
- Deploy: `.github/workflows/deploy.yml` requiere secrets `DEPLOY_HOST`, `DEPLOY_USER`, `DEPLOY_SSH_KEY`, `DEPLOY_PORT`, `DEPLOY_PATH`

## Migraciones
- Las bases SQLite se migran automaticamente al arrancar cada servicio (EF Core `Migrate`) si `Database__ApplyMigrations=true` o `DB_APPLY_MIGRATIONS=true` en compose.
- Para desarrollo/local:
  - `dotnet tool restore`
  - `dotnet tool run dotnet-ef migrations list --project <service>/<service>.csproj --startup-project <service>/<service>.csproj`
- Antes de subir a produccion, hacer backup del volumen de datos SQLite.

### Arranque
```bash
docker compose -f docker-compose.prod.yml --env-file .env.prod up -d
```

## Seguridad
- Cambiar `AUTH_SIGNING_KEY`
- Verificar roles Admin para `/api/rh/v1/*`
- Rate limiting activo en BFF e Integration Hub
- Configurar `CORS_ORIGINS` en `.env.prod`
- Headers HTTP en prod: `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `Permissions-Policy`, `Content-Security-Policy`

## Secrets
- Crear `secrets.json` para Integration Hub (montado en `/data/secrets.json`)
- Persistir DataProtection keys en `/data/keys` (ver `DataProtection:KeysPath`)

## Observabilidad
- Logs JSON en Production (console formatter)
- Health checks: `/healthz` (liveness) y `/readyz` (readiness)
- Logs estructurados con TraceId
- OpenTelemetry: activar con `OTEL_ENABLED=true` y configurar `OTEL_EXPORTER_OTLP_ENDPOINT`
- Stack local: `otel-collector`, Jaeger y Prometheus (ver `docs/operacion/observabilidad.md`)

## Backups y restore (SQLite)
- Backup: detener servicios o asegurar ventana sin escrituras.
- Copiar los archivos `.db` de los volumenes (`auth-data`, `organizacion-data`, `personal-data`, `liquidacion-data`, `nucleuswf-data`, `integration-data`, `portal-bff-data`).
- Restore: reemplazar los `.db` en los volumenes y reiniciar servicios con `DB_APPLY_MIGRATIONS=true`.
- Keys: respaldar `/data/keys` para mantener DataProtection estable entre despliegues.

## Rotacion de secretos
- Cambiar `AUTH_SIGNING_KEY` en ventanas de mantenimiento.
- Regenerar `secrets.json` de Integration Hub y redeploy.
- Versionar el cambio y registrar fecha en el reporte de cambios.

## Runbooks
- Ver `docs/operacion/runbooks.md`.
- Health checks: `/health` en todos los servicios

## Runtime
- Swagger se expone solo en Development.
- Forwarded headers habilitados para TLS en proxy.

## Pruebas
- `dotnet test portal-bff-service.Tests`
- `dotnet test integration-hub-service.Tests`
- `dotnet test liquidacion-service.Tests`

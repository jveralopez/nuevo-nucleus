# Smoke test 2026-03-12

## Entorno
- Docker 29.1.3
- `.env.prod` creado desde `.env.prod.example`

## Pasos ejecutados
1. `docker compose -f docker-compose.prod.yml --env-file .env.prod up -d`
2. `docker compose -f docker-compose.prod.yml --env-file .env.prod ps`
3. Health checks en `http://localhost:<puerto>/health`
4. Login en `auth-service` y llamada protegida `/me`

## Resultado
- Todos los servicios quedaron en estado `healthy`.
- `/health` devolvio 200 en: 5001, 5100, 5200, 5188, 5050, 5051, 5090, 5300.
- `/healthz` devolvio 404 en la mayoria (esperado salvo configuracion-service).
- Login OK con `admin/admin123` y `/me` OK.

## Incidentes
- Falla inicial por ruta de `configuracion-service` en Dockerfile; corregido a `configuracion-service.csproj`.

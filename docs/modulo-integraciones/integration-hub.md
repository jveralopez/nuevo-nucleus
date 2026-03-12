# Integration Hub MVP

## Que se crea
- Servicio `integration-hub-service` (ASP.NET Core minimal API).
- Modelos para templates, conexiones y jobs.
- Persistencia local en JSON para PoC.
- Generacion de archivo simulado por job.

## Para que funcion
- Centraliza definiciones de integracion (templates).
- Permite ejecutar jobs manuales (trigger) y registrar estado.
- Deja trazabilidad basica de ejecuciones.

## Componentes
- **Templates**: definen origen, transformacion y destino.
- **Conexiones**: credenciales/hosts para conectores.
- **Jobs**: ejecuciones con estado y archivo generado.

## Flujo MVP
1. Crear template.
2. Publicar template.
3. Crear conexión.
4. Ejecutar job.
5. Consultar estado y archivo generado.

## Seguridad
- JWT obligatorio.
- Escrituras requieren rol `Admin`.

## Conector SFTP (MVP)
- Usa `Destination.Type = sftp` y `Destination.Connection`.
- Resuelve `SecretId` desde vault de secretos (archivo).

### Vault (archivo local)
`Secrets.SecretsFile` en `appsettings.json`.

## Scheduler y reintentos
- `Scheduler.IntervalSeconds` controla el polling.
- Jobs fallidos se reintentan hasta `Scheduler.MaxRetries`.

## Persistencia
- SQLite `integration-hub.db`
- `storage/exports/*.txt`

## Endpoints MVP
- `GET /integraciones/templates`
- `POST /integraciones/templates`
- `PUT /integraciones/templates/{id}`
- `POST /integraciones/templates/{id}/publish`
- `GET /integraciones/conexiones`
- `POST /integraciones/conexiones`
- `GET /integraciones/jobs`
- `POST /integraciones/jobs`
- `POST /integraciones/jobs/{id}/retry`

## Template real
- Banco Galicia: `docs/modulo-integraciones/template-banco-galicia.json`.

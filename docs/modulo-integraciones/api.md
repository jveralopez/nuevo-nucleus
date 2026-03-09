# API propuesta · Integraciones

## Templates
- `GET /integraciones/templates`
- `POST /integraciones/templates`
```json
{
  "name": "banco-galicia",
  "version": "1.0.0",
  "schedule": "0 4 * * *",
  "source": { "type": "sql", "connection": "rrhh-sql", "query": "./queries/galicia.sql" },
  "transform": { "type": "liquid", "template": "./templates/galicia.liquid" },
  "destination": { "type": "sftp", "connection": "galicia-sftp", "path": "/out/{{fecha}}.txt" }
}
```
- `PUT /integraciones/templates/{id}`, `DELETE /integraciones/templates/{id}`.
- `POST /integraciones/templates/{id}/publish`.

## Ejecuciones
- `POST /integraciones/jobs`
```json
{ "templateId": "banco-galicia", "periodo": "2026-02", "trigger": "manual" }
```
- `GET /integraciones/jobs?templateId=&estado=`
- `GET /integraciones/jobs/{jobId}` → logs, archivos generados, métricas.
- `POST /integraciones/jobs/{jobId}/retry`.

## Conexiones y secretos
- `GET /integraciones/conexiones`
- `POST /integraciones/conexiones`
```json
{ "name": "galicia-sftp", "type": "sftp", "host": "sftp.galicia.com", "username": "user", "secretId": "kv/galicia-pass" }
```
- `GET /integraciones/destinos` (bancos, legales, sindicatos, data lake, APIs).

## Webhooks / eventos
- `POST /integraciones/triggers` para registrar eventos (ej. Liquidación finalizada) → define mapping `eventName -> template`.
- Eventos emitidos: `IntegrationJobStarted`, `IntegrationJobCompleted`, `IntegrationJobFailed`.

## Seguridad
- Scopes: `integraciones.read`, `integraciones.write`, `integraciones.admin`.
- Permisos para ejecutar manualmente, editar templates, gestionar conexiones.

---
*Inspirado en el generador `InterfacesOut` y las interfaces XML actuales.*

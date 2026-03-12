# Integration Hub Service

Microservicio ASP.NET Core minimal API para administrar templates, conexiones y ejecuciones de integraciones batch/event-driven.

## Proposito
Reemplazar el esquema legacy `InterfacesOut/Interfaces` con una plataforma moderna de integraciones, auditables y configurables.

## Seguridad
Requiere JWT emitido por `auth-service`.
- Lecturas: usuario autenticado.
- Escrituras (create/update/publish/jobs): rol `Admin`.

Configurar `Auth` en `appsettings.json` con issuer/audience/signingKey.

## Requisitos
- .NET SDK 8.0

## Ejecucion
```bash
dotnet restore
dotnet run --project integration-hub-service.csproj
```

La API quedara en `https://localhost:5001` (por defecto) o la URL configurada con `ASPNETCORE_URLS`.

## Endpoints principales
- `GET /health`
- `GET /integraciones/templates`
- `POST /integraciones/templates`
- `PUT /integraciones/templates/{id}`
- `POST /integraciones/templates/{id}/publish`
- `GET /integraciones/conexiones`
- `POST /integraciones/conexiones`
- `GET /integraciones/jobs`
- `POST /integraciones/jobs`
- `POST /integraciones/jobs/{id}/retry`

## Persistencia
SQLite por defecto (`integration-hub.db`). Los archivos generados se guardan en `storage/exports/`.

Configurar `ConnectionStrings.IntegrationHubDb` en `appsettings.json`.

## SFTP (MVP)
Para `destination.type = sftp`, el `SecretId` se resuelve desde un vault de secretos.

### Vault (archivo local)
Configurar `Secrets.SecretsFile` en `appsettings.json`.
Formato esperado:
```json
{
  "galicia-sftp": "password"
}
```

## Scheduler y reintentos
Configurar en `appsettings.json`:
- `Scheduler.IntervalSeconds`
- `Scheduler.MaxRetries`

## Observabilidad
Incluye logging por request y cabecera `X-Trace-Id` en respuestas.

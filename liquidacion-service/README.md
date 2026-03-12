# Liquidacion Service

Servicio ASP.NET Core minimal API que soporta la creación y procesamiento de liquidaciones.

## Requisitos
- .NET SDK 8.0

## Seguridad
Requiere JWT emitido por `auth-service`.
- Lecturas: usuario autenticado.
- Escrituras: rol `Admin`.

Configurar `Auth` en `appsettings.json` con issuer/audience/signingKey.

## Ejecución
```bash
dotnet restore
dotnet run --project liquidacion-service.csproj
```

La API quedará en `https://localhost:7188` (por defecto) o la URL configurada con `ASPNETCORE_URLS`.

## Endpoints principales
- `GET /health`
- `GET /payrolls`
- `POST /payrolls`
- `POST /payrolls/{id}/legajos`
- `DELETE /payrolls/{id}/legajos/{legajoId}`
- `POST /payrolls/{id}/procesar`
- `GET /payrolls/{id}/recibos`
- `GET /payrolls/{id}/exports`
- `GET /payrolls/{id}/exports/empleado`
- `GET /exports/{fileName}`

## Integracion con Integration Hub
Si `ProcessPayroll` se ejecuta con `Exportar=true`, se dispara un job en Integration Hub.

Configurar en `appsettings.json`:
- `IntegrationHub.BaseUrl`
- `IntegrationHub.TemplateId`
- `IntegrationHub.AccessToken` (JWT del `auth-service` con rol Admin)

## Persistencia
SQLite por defecto (`liquidacion.db`).
Configurar `ConnectionStrings.LiquidacionDb` en `appsettings.json`.

## Observabilidad
Incluye logging por request y cabecera `X-Trace-Id` en respuestas.

Los datos se almacenan en `storage/liquidacion-db.json`; los archivos exportados en `storage/exports/`.

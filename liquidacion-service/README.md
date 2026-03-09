# Liquidacion Service

Servicio ASP.NET Core minimal API que soporta la creación y procesamiento de liquidaciones.

## Requisitos
- .NET SDK 8.0

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
- `GET /exports/{fileName}`

Los datos se almacenan en `storage/liquidacion-db.json`; los archivos exportados en `storage/exports/`.

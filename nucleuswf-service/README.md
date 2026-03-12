# Nucleus WF Service

Servicio ASP.NET Core minimal API para definicion y ejecucion de workflows.

## Requisitos
- .NET SDK 8.0

## Seguridad
Requiere JWT emitido por `auth-service`.
- Lecturas: usuario autenticado.
- Escrituras: rol `Admin`.

Configurar `Auth` en `appsettings.json` con issuer/audience/signingKey.

## Persistencia
SQLite por defecto (`nucleuswf.db`).
Configurar `ConnectionStrings.NucleusWfDb` en `appsettings.json`.

## Ejecucion
```bash
dotnet restore
dotnet run --project nucleuswf-service.csproj
```

La API quedara en `https://localhost:5001` (por defecto) o la URL configurada con `ASPNETCORE_URLS`.

## Endpoints principales
- `GET /health`
- `GET /definitions`
- `POST /definitions`
- `PUT /definitions/{id}`
- `GET /instances`
- `POST /instances`
- `POST /instances/{id}/transitions`

Los datos se almacenan en `storage/nucleuswf-db.json`.

# Personal Service

Servicio ASP.NET Core minimal API para administrar legajos y datos basicos de personal.

## Requisitos
- .NET SDK 8.0

## Seguridad
Requiere JWT emitido por `auth-service`.
- Lecturas: usuario autenticado.
- Escrituras: rol `Admin`.

Configurar `Auth` en `appsettings.json` con issuer/audience/signingKey.

## Persistencia
SQLite por defecto (`personal.db`).
Configurar `ConnectionStrings.PersonalDb` en `appsettings.json`.

## Ejecucion
```bash
dotnet restore
dotnet run --project personal-service.csproj
```

La API quedara en `https://localhost:5001` (por defecto) o la URL configurada con `ASPNETCORE_URLS`.

## Endpoints principales
- `GET /health`
- `GET /legajos`
- `POST /legajos`
- `PUT /legajos/{id}` / `DELETE /legajos/{id}`

Los datos se almacenan en `storage/personal-db.json`.

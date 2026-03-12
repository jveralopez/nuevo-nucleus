# Organizacion Service

Servicio ASP.NET Core minimal API para administrar empresas, unidades organizativas, posiciones y centros de costo.

## Requisitos
- .NET SDK 8.0

## Seguridad
Requiere JWT emitido por `auth-service`.
- Lecturas: usuario autenticado.
- Escrituras: rol `Admin`.

Configurar `Auth` en `appsettings.json` con issuer/audience/signingKey.

## Persistencia
SQLite por defecto (`organizacion.db`).
Configurar `ConnectionStrings.OrganizacionDb` en `appsettings.json`.

## Ejecucion
```bash
dotnet restore
dotnet run --project organizacion-service.csproj
```

La API quedara en `https://localhost:5001` (por defecto) o la URL configurada con `ASPNETCORE_URLS`.

## Endpoints principales
- `GET /health`
- `GET /empresas`
- `POST /empresas`
- `PUT /empresas/{id}` / `DELETE /empresas/{id}`
- `GET /unidades`
- `POST /unidades`
- `PUT /unidades/{id}` / `DELETE /unidades/{id}`
- `GET /posiciones`
- `POST /posiciones`
- `POST /posiciones/{id}/asignar`
- `PUT /posiciones/{id}` / `DELETE /posiciones/{id}`
- `GET /centros-costo`
- `POST /centros-costo`
- `PUT /centros-costo/{id}` / `DELETE /centros-costo/{id}`

Los datos se almacenan en `storage/organizacion-db.json`.

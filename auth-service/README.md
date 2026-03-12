# Auth Service

Servicio ASP.NET Core minimal API para autenticacion JWT y gestion basica de usuarios/roles.

## Requisitos
- .NET SDK 8.0

## Configuracion
Editar `appsettings.json` y cambiar `Auth.SigningKey` y las credenciales de seed admin.

## Ejecucion
```bash
dotnet restore
dotnet run --project auth-service.csproj
```

La API quedara en `https://localhost:5001` (por defecto) o la URL configurada con `ASPNETCORE_URLS`.

## Endpoints principales
- `GET /health`
- `POST /login`
- `GET /me` (requiere JWT)
- `POST /users` (requiere rol Admin)

Seed admin (por defecto): `admin` / `admin123`.

## Persistencia
SQLite por defecto (`auth.db`).
Configurar `ConnectionStrings.AuthDb` en `appsettings.json`.

## Observabilidad
Incluye logging por request y cabecera `X-Trace-Id` en respuestas.

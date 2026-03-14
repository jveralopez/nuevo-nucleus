# Tiempos Service

Servicio ASP.NET Core minimal API para gestionar turnos, horarios, fichadas y planillas de horas.

## Requisitos
- .NET SDK 8.0

## Seguridad
Requiere JWT emitido por `auth-service`.
- Lecturas: usuario autenticado.
- Escrituras: rol `Admin`.

Configurar `Auth` en `appsettings.json` con issuer/audience/signingKey.

## Persistencia
SQLite por defecto (`tiempos.db`).
Configurar `ConnectionStrings.TiemposDb` en `appsettings.json`.

## Ejecucion
```bash
dotnet restore
dotnet run --project tiempos-service.csproj
```

La API quedara en `https://localhost:5001` (por defecto) o la URL configurada con `ASPNETCORE_URLS`.

## Endpoints principales
- `GET /health`
- `GET /turnos`
- `GET /horarios`
- `GET /fichadas`
- `GET /planillas`

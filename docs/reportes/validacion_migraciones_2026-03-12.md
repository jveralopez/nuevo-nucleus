# Validacion de migraciones 2026-03-12

## Servicio probado
- `organizacion-service`

## DB vacia
```
dotnet ef database update --project organizacion-service/organizacion-service.csproj \
  --connection "Data Source=C:\proyectos\nuevo-nucleus\temp\organizacion-with-data.db"
```
Resultado: migraciones `20260312113613_InitialCreate` y `20260312154000_AddOrganigramas` aplicadas.

## DB con datos
1. Se inserto registro en `Empresas`.
2. Se ejecuto nuevamente `dotnet ef database update`.
3. Se verifico que la fila permanece.

Resultado: DB actualizada sin perdida de datos.

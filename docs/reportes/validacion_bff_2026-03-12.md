# Validacion BFF 2026-03-12

## Entorno
- Stack levantado con `docker-compose.prod.yml`.
- Token JWT generado via `auth-service`.

## Resultados
- `GET http://localhost:5090/health` -> 200
- `GET http://localhost:5090/api/rh/v1/organizacion/organigramas` -> 200
- `GET http://localhost:5090/api/rh/v1/organizacion/unidades/tree` -> 200
- `POST http://localhost:5090/api/rh/v1/organizacion/organigramas` -> 201

## Observaciones
- La falla anterior se debio a volumenes con tablas existentes sin `__EFMigrationsHistory`.
- Reinicio con volumenes nuevos resolvio los 404/405.
- Para el POST se creo `Empresa Demo` y `Unidad Central` via BFF.

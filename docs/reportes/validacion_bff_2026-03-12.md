# Validacion BFF 2026-03-12

## Entorno
- Stack levantado con `docker-compose.prod.yml`.
- Token JWT generado via `auth-service`.

## Resultados
- `GET http://localhost:5090/health` -> 200
- `GET http://localhost:5090/api/rh/v1/organizacion/organigramas` -> 404
- `GET http://localhost:5090/api/rh/v1/organizacion/unidades/tree` -> 405
- `POST http://localhost:5090/api/rh/v1/organizacion/organigramas` -> 404

## Observaciones
- Endpoints de organizacion en `organizacion-service` retornaron 404 para `/organigramas` y `/unidades/tree`.
- `GET /empresas` en `organizacion-service` retorno 200, por lo que el servicio esta arriba.

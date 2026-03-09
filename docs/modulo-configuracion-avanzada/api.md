# API propuesta · Configuración

Base path: `/api/config/v1`

## Parámetros
- `GET /parametros?ambiente=&clave=`
- `POST /parametros`
```json
{
  "clave": "vacaciones.max_dias",
  "valor": 30,
  "tipo": "int",
  "ambiente": "prod",
  "descripcion": "Días máximos a solicitar"
}
```
- `PUT /parametros/{id}` / `DELETE`
- `POST /parametros/{id}/promover` (Dev→Test→Prod).

## Catálogos
- `GET /catalogos/{nombre}`
- `POST /catalogos/{nombre}` (agregar entrada), `PUT /catalogos/{nombre}/{codigo}`, `DELETE`.

## Feature flags / releases
- `GET /flags`, `POST /flags` (definir flag, targets).
- `POST /flags/{id}/activar` / `desactivar`.
- `GET /releases`, `POST /releases` (paquetes de config para despliegue).

## Conexiones
- `GET /conexiones`, `POST /conexiones` (`tipo`, `config`, `secretRef`).
- Integración con vault para secretos.

## Auditoría
- `GET /auditoria?entidad=&usuario=&fechaDesde=`
- Registro de cambios, approvals, comentarios.

## Eventos
- `ConfigParameterUpdated`, `CatalogUpdated`, `FeatureFlagChanged`, `ConfigReleaseCreated`.

## Seguridad
- Scopes: `config.read`, `config.write`, `config.admin`.
- RBAC + approvals.

---
*Blueprint para Configuración.*

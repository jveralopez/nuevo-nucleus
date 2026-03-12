# API propuesta · Organización

Base path: `/api/organizacion/v1`

## Empresas
- `GET /empresas`
- `POST /empresas` (`nombre`, `pais`, `moneda`, `estado`).
- `PUT /empresas/{id}` / `DELETE`.

## Unidades organizativas
- `GET /unidades?empresaId=&padreId=&tipo=`
- `POST /unidades`
```json
{
  "empresaId": "EMP01",
  "nombre": "Operaciones Latam",
  "tipo": "Area",
  "padreId": "UO-001",
  "centroCostoId": "CC-100"
}
```
- `PUT /unidades/{id}` (cambios, reubicaciones), `DELETE` (soft delete).

## Posiciones
- `GET /posiciones?unidadId=&estado=`
- `POST /posiciones` (nombre, nivel, perfil, unidad).
- `POST /posiciones/{id}/asignar` para asociar legajos (referencia a módulo Personal).

## Organigramas / estructuras
- `GET /organigramas` (lista de versiones).
- `POST /organigramas`
```json
{
  "nombre": "Org 2026",
  "version": "v2026.1",
  "vigenciaDesde": "2026-01-01",
  "nodos": [...],
  "edges": [...]
}
```
- `POST /organigramas/{id}/publish`, `POST /organigramas/{id}/clone`.
- `GET /organigramas/{id}/export?format=graphml`.

## Centros de costo
- `GET /centros-costo?empresaId=`
- `POST /centros-costo` (código, descripción, moneda, estado).

## Eventos
- `OrgUnitCreated`, `OrgUnitUpdated`, `OrgUnitDeleted`.
- `PositionCreated`, `PositionUpdated`, `PositionDeleted`.
- `OrgStructurePublished`.

## Seguridad
- Scopes: `org.read`, `org.write`, `org.publish`.
- Governance: revisión y aprobación antes de publicar una estructura.

---
*Basado en los catálogos de Organización actuales.*

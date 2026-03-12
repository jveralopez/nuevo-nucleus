# API propuesta · Analytics

Base path: `/api/analytics/v1` (GraphQL u OData)

## Datasets / consultas
- `GET /datasets` (lista de datasets disponibles: Headcount, Ausentismo, Costos, Vacaciones, Reclamos, etc.).
- `POST /query`
```json
{
  "dataset": "headcount",
  "filters": {"periodo": "2026-03", "orgUnitId": "UO-100"},
  "metrics": ["FTE", "Costo"]
}
```
- GraphQL: `query { headcount(periodo:"2026-03", orgUnit:"UO-100") { fte costo } }`

## Dashboards / widgets
- `GET /dashboards`, `GET /dashboards/{id}`
- `POST /dashboards` (definir panel, widgets, filtros).

## Alerts / insights
- `GET /alerts`, `POST /alerts` (reglas basadas en métricas, ex. ausentismo > 5%).
- `POST /insights` (crear insight manual/IA).

## ML endpoints
- `POST /ml/predictions/rotacion`
```json
{ "legajoId": "LEG-001", "features": {"antiguedad": 4, "edad": 30, ...} }
```
- `POST /ml/predictions/ausentismo`.

## Integraciones
- Exportes a BI (Power BI) via datasets OData.
- Webhooks para eventos `AnalyticsAlertTriggered`.

## Seguridad
- Scopes: `analytics.read`, `analytics.write`, `analytics.admin`.
- Row-level security por unidad/rol, auditors logs.

---
*Blueprint para Analytics.*

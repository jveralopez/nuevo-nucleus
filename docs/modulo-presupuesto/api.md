# API propuesta · Presupuesto

Base path: `/api/presupuesto/v1`

## Presupuestos y versiones
- `GET /presupuestos?periodo=&escenario=`
- `POST /presupuestos`
```json
{
  "nombre": "Budget 2026",
  "periodo": "2026",
  "escenario": "Base",
  "moneda": "USD",
  "supuestos": {
    "incrementoSalarial": 0.08,
    "beneficios": 0.12
  }
}
```
- `GET /presupuestos/{id}` / `PUT` / `DELETE`.
- `POST /presupuestos/{id}/versiones`
```json
{
  "nombre": "V1",
  "descripcion": "Carga inicial",
  "fuente": "TopDown"
}
```
- `POST /versiones/{id}/publicar`.

## Líneas presupuestarias
- `GET /versiones/{id}/lineas?orgUnitId=&positionId=`
- `POST /versiones/{id}/lineas`
```json
{
  "orgUnitId": "UO-100",
  "positionId": "POS-200",
  "fte": 2,
  "costoMensual": 520000,
  "comentarios": "Nuevo equipo"
}
```
- `PUT /lineas/{id}` (ajustes), `DELETE /lineas/{id}`.

## Supuestos
- `GET /presupuestos/{id}/supuestos`
- `POST /presupuestos/{id}/supuestos`
```json
{
  "tipo": "IncrementoSalarial",
  "valor": 0.08,
  "detalle": "Ajuste anual"
}
```

## Variaciones y reales
- `GET /versiones/{id}/variaciones?periodo=` (compara vs actual).
- `POST /presupuestos/{id}/actuals`
```json
{
  "periodo": "2026-02",
  "orgUnitId": "UO-100",
  "costoReal": 480000,
  "fuente": "Liquidacion"
}
```

## Workflow
- `POST /presupuestos/{id}/submit` (envía a aprobación via Nucleus WF).
- `POST /presupuestos/{id}/aprobar` / `rechazar`.

## Eventos
- `BudgetCreated`, `BudgetVersionPublished`, `BudgetSubmitted`, `BudgetApproved`, `BudgetVarianceDetected`.

## Seguridad
- Scopes: `budget.read`, `budget.write`, `budget.approve`.
- Access control por empresa/unidad.

---
*Blueprint para Presupuesto.*

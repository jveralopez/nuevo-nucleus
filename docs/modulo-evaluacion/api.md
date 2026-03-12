# API propuesta · Evaluación

Base path: `/api/evaluaciones/v1`

## Ciclos
- `GET /ciclos`
- `POST /ciclos`
```json
{
  "nombre": "Performance 2026",
  "periodo": "2026-Q1",
  "inicio": "2026-03-01",
  "fin": "2026-05-31",
  "config": {
    "etapas": ["Auto", "Jefe", "Pares", "HR", "Calibración"],
    "competencias": ["Colaboración", "Customer Focus"],
    "escala": [1,2,3,4,5]
  }
}
```

## Objetivos y competencias
- `GET /ciclos/{id}/objetivos`
- `POST /ciclos/{id}/objetivos` (asignar plantillas de objetivos).
- `POST /legajos/{legajoId}/objetivos` (objetivos individuales).
- `GET /competencias`, `POST /competencias`.

## Evaluaciones
- `POST /evaluaciones`
```json
{
  "cycleId": "CIC-2026-Q1",
  "evaluadoId": "LEG-001",
  "evaluadorId": "LEG-010",
  "tipo": "Jefe"
}
```
- `GET /evaluaciones?cycleId=&evaluadoId=&estado=`
- `PUT /evaluaciones/{id}` (guardar respuestas) con payload de objetivos, competencias y comentarios.
- `POST /evaluaciones/{id}/submit`
- `POST /evaluaciones/{id}/aprobar` (HR/calibración).

## Feedback continuo
- `POST /feedback`
```json
{ "evaluadoId": "LEG-001", "autorId": "LEG-050", "contenido": "Excelente trabajo en el proyecto X" }
```
- `GET /feedback?evaluadoId=`

## Calibraciones
- `GET /calibraciones?cycleId=&orgUnitId=`
- `POST /calibraciones` (crear sesión, asignar participantes, cargar notas).
- `POST /calibraciones/{id}/cerrar`.

## Eventos
- `EvaluationCreated`, `EvaluationCompleted`, `EvaluationSubmitted`, `CalibrationCompleted`.

## Seguridad
- Scopes: `eval.read`, `eval.write`, `eval.admin`.
- Rules: Solo evaluadores asignados pueden editar; HRBP/Comité tienen acceso ampliado.

---
*Blueprint para modernizar el módulo de Evaluación.*

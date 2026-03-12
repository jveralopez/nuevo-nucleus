# API propuesta · Carrera & Sucesión

Base path: `/api/carrera/v1`

## Planes de carrera
- `GET /planes?legajoId=&estado=`
- `POST /planes`
```json
{
  "legajoId": "LEG-041",
  "horizonte": "18 meses",
  "objetivos": ["Liderar equipo"],
  "mentorId": "LEG-010",
  "acciones": [
    { "tipo": "Capacitacion", "cursoId": "CURS-001", "fecha": "2026-05-01" }
  ]
}
```
- `PUT /planes/{id}`, `POST /planes/{id}/aprobar`.

## Posiciones críticas y sucesores
- `GET /posiciones-criticas?orgUnitId=`
- `POST /posiciones-criticas`
```json
{
  "positionId": "POS-300",
  "criticidad": "Alta",
  "sucesores": [
    { "legajoId": "LEG-210", "readiness": "12 meses", "prioridad": 1 },
    { "legajoId": "LEG-112", "readiness": "18 meses", "prioridad": 2 }
  ]
}
```
- `POST /posiciones-criticas/{id}/actualizar`.

## Talent pools
- `GET /pools`, `POST /pools`
```json
{
  "nombre": "Gerencia TI",
  "criterios": {"nivel": "Sr", "potencial": "Alto"},
  "miembros": ["LEG-210", "LEG-310"]
}
```
- `POST /pools/{id}/miembros`.

## Integraciones
- `POST /planes/{id}/sync` (sincroniza con Capacitaciones/Evaluación).
- `POST /posiciones-criticas/{id}/eventos` (publica eventos para Portal/Analytics).

## Eventos
- `CareerPlanCreated`, `CareerPlanUpdated`, `SuccessionMapUpdated`, `TalentPoolUpdated`.

## Seguridad
- Scopes: `career.read`, `career.write`, `career.admin`.
- Access control por unidad y rol (Comité, HRBP, manager).

---
*Blueprint para Carrera/Sucesión.*

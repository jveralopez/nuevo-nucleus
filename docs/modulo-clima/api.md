# API propuesta · Clima / Encuestas

Base path: `/api/clima/v1`

## Campañas
- `GET /campanias?estado=&tipo=`
- `POST /campanias`
```json
{
  "nombre": "Clima Semestral",
  "tipo": "Engagement",
  "inicio": "2026-05-01",
  "fin": "2026-05-31",
  "segmentos": ["ORG-100", "ORG-200"],
  "anonima": true
}
```
- `PUT /campanias/{id}`, `POST /campanias/{id}/publicar`, `POST /campanias/{id}/cerrar`.

## Preguntas / cuestionarios
- `POST /campanias/{id}/preguntas` (Likert, texto, NPS, etc.).
- `GET /campanias/{id}/preguntas`.

## Respuestas
- `POST /campanias/{id}/respuestas`
```json
{
  "segmento": "ORG-100",
  "respuestas": [
    {"preguntaId": "P-01", "valor": 5},
    {"preguntaId": "P-02", "valor": 3}
  ]
}
```
- `GET /campanias/{id}/resumen` (agregados), `GET /campanias/{id}/detalles` (según permisos).

## Planes de acción
- `POST /campanias/{id}/acciones`
```json
{ "orgUnitId": "UO-100", "responsableId": "LEG-010", "descripcion": "Programa de mentoring" }
```
- `POST /acciones/{id}/actualizar` (estado, notas).

## Eventos
- `SurveyCampaignCreated`, `SurveyResponseSubmitted`, `SurveyCampaignClosed`, `SurveyActionCreated`.

## Seguridad
- Scopes: `clima.read`, `clima.write`, `clima.admin`.
- Controles para anonimato y acceso a datos sensibles.

---
*Blueprint para Clima.*

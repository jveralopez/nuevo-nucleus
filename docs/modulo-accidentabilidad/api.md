# API propuesta · Accidentabilidad

Base path: `/api/accidentes/v1`

## Incidentes
- `GET /incidentes?legajoId=&estado=&tipo=&gravedad=`
- `POST /incidentes`
```json
{
  "legajoId": "LEG-123",
  "fecha": "2026-03-21T10:15:00Z",
  "ubicacion": "Planta Rosario",
  "tipo": "Accidente",
  "gravedad": "Moderada",
  "descripcion": "Golpe en mano",
  "testigos": ["LEG-333"],
  "adjuntos": ["foto.jpg"]
}
```
- `GET /incidentes/{id}`
- `PUT /incidentes/{id}` (actualizar datos, estado).
- `POST /incidentes/{id}/cerrar`.

## Investigación y acciones
- `POST /incidentes/{id}/investigaciones`
```json
{
  "responsable": "HSQE-1",
  "hallazgos": "Falta de guardas",
  "causas": "Procedimiento no seguido"
}
```
- `POST /incidentes/{id}/acciones` (acciones correctivas/preventivas).
- `POST /acciones/{id}/actualizar` (estado, comentarios).

## Notificaciones / ART
- `POST /incidentes/{id}/notificar`
```json
{ "destino": "ART", "medio": "API", "resultado": "Enviado" }
```
- `GET /incidentes/{id}/notificaciones`.

## Reportes
- `GET /reportes/frecuencia?periodo=` (TIR/TFR), `GET /reportes/gravedad`.
- `GET /reportes/acciones?estado=`.

## Eventos
- `IncidentReported`, `InvestigationStarted`, `ActionAssigned`, `IncidentClosed`.

## Seguridad
- Scopes: `accidentes.read`, `accidentes.write`, `accidentes.admin`.
- Controles para datos sensibles (salud, testimonios).

---
*Blueprint para el módulo de Accidentabilidad.*

# API propuesta · Selección

Base path: `/api/seleccion/v1`

## Candidatos
- `POST /candidatos`
```json
{
  "nombre": "Ana",
  "apellido": "Nieto",
  "email": "ana@example.com",
  "telefono": "+54 11 5555-0000",
  "fuente": "WebCV",
  "cv": { "archivoUrl": "...", "texto": "..." }
}
```
- `GET /candidatos?search=&skill=&estado=`
- `GET /candidatos/{id}` / `PUT` / `DELETE`
- `POST /candidatos/{id}/exp` (experiencia), `POST /candidatos/{id}/educacion`, etc.

## Avisos
- `POST /avisos`
```json
{
  "titulo": "QA Senior",
  "descripcion": "Automatización",
  "ubicacion": "Buenos Aires / Remoto",
  "categoria": "IT",
  "publicarEn": ["Portal", "LinkedIn"]
}
```
- `GET /avisos?estado=&ubicacion=`
- `POST /avisos/{id}/publicar`, `POST /avisos/{id}/cerrar`

## Postulaciones
- `POST /avisos/{id}/postular`
```json
{
  "candidateId": "CAN-123",
  "fuente": "Portal",
  "respuestas": [ {"pregunta": "Expectativa salarial", "respuesta": "4000 USD"} ]
}
```
- `GET /postulaciones?avisos=&estado=&recruiter=`
- `PATCH /postulaciones/{id}` (cambiar estado, responsable, prioridad).

## Pipeline / entrevistas
- `POST /postulaciones/{id}/etapas`
```json
{
  "nombre": "Entrevista técnica",
  "fecha": "2026-03-11T15:00:00Z",
  "participantes": ["USR-101", "USR-205"],
  "notas": "Evaluar automatización"
}
```
- `POST /postulaciones/{id}/feedback`.
- `POST /postulaciones/{id}/oferta` (propone condiciones).

## Portal / autenticación
- `POST /portal/login`, `POST /portal/registro`, `POST /portal/password-reset` (BFF o Auth service).
- `GET /portal/candidatos/{id}/postulaciones` (candidato ve su estado).

## Integraciones
- Webhooks: `POST /webhooks/external` para recibir postulaciones externas.
- Eventos emitidos: `CandidateCreated`, `ApplicationSubmitted`, `StageChanged`, `OfferAccepted`, `CandidateHired`.

## Seguridad
- OIDC B2C (candidatos) y OIDC corporativo (recruiters/admins).
- Roles: `Recruiter`, `HiringManager`, `Admin`, `Candidate`.
- Rate limits para endpoints públicos (portal).

---
*Basado en flujos actuales de WebCV y clases CVs.*

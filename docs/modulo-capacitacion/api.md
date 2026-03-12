# API propuesta · Capacitaciones

Base path: `/api/capacitaciones/v1`

## Cursos y sesiones
- `GET /cursos?estado=&modalidad=`
- `POST /cursos`
```json
{
  "nombre": "Liderazgo 101",
  "descripcion": "Fundamentos de liderazgo",
  "tipo": "Soft Skill",
  "duracionHoras": 8,
  "modalidad": "Presencial"
}
```
- `PUT /cursos/{id}` / `DELETE`.
- `POST /cursos/{id}/sesiones`
```json
{
  "fecha": "2026-04-15T14:00:00Z",
  "lugar": "Sala A",
  "capacidad": 25,
  "instructorId": "INST-001"
}
```
- `GET /sesiones?cursoId=&estado=`

## Inscripciones
- `POST /sesiones/{id}/inscripciones`
```json
{ "legajoId": "LEG-020" }
```
- `GET /sesiones/{id}/inscripciones`
- `POST /inscripciones/{id}/aprobar` (si requiere aprobación), `POST /inscripciones/{id}/cancelar`.

## Asistencias y evaluaciones
- `POST /inscripciones/{id}/asistencia` (registrar check-in/out).
- `POST /inscripciones/{id}/evaluacion`
```json
{ "puntaje": 4, "comentarios": "Excelente contenido" }
```
- `POST /inscripciones/{id}/certificar` (emite certificado con link/QR).

## Contenido / LMS
- `GET /cursos/{id}/contenido`, `POST /cursos/{id}/contenido` (videos, SCORM, PDFs).
- `POST /cursos/{id}/plan` (planes de aprendizaje con múltiples cursos).

## Eventos
- `CourseCreated`, `SessionScheduled`, `EnrollmentApproved`, `CourseCompleted`, `CertificateIssued`.

## Seguridad
- Scopes: `learning.read`, `learning.write`, `learning.admin`.
- Reglas: inscripciones/acciones según rol (empleado, jefe, RRHH).

---
*Blueprint para Capacitaciones.*

# API propuesta · Sanciones

Base path: `/api/sanciones/v1`

## Casos disciplinarios
- `GET /casos?legajoId=&estado=&tipo=`
- `POST /casos`
```json
{
  "legajoId": "LEG-210",
  "tipo": "Amonestacion",
  "motivoId": "MOT-001",
  "descripcion": "Incumplimiento horario",
  "reportadoPor": "LEG-050"
}
```
- `GET /casos/{id}`, `PUT /casos/{id}` (actualizar detalles, estado).
- `POST /casos/{id}/cerrar`.

## Etapas / descargos
- `POST /casos/{id}/etapas`
```json
{ "etapa": "Investigacion", "responsableId": "RRHH-01", "notas": "Se entrevista al supervisor" }
```
- `POST /casos/{id}/descargo`
```json
{ "legajoId": "LEG-210", "mensaje": "Presento descargo", "adjuntos": ["descargo.pdf"] }
```

## Documentos / planes de mejora
- `POST /casos/{id}/documentos`
- `POST /casos/{id}/planes`
```json
{ "descripcion": "Plan puntualidad", "responsableId": "LEG-210", "fechaCompromiso": "2026-04-15" }
```
- `POST /planes/{id}/actualizar`.

## Eventos
- `DisciplinaryCaseCreated`, `DisciplinaryStepCompleted`, `DisciplinaryCaseClosed`, `DisciplinaryPlanUpdated`.

## Seguridad
- Scopes: `discipline.read`, `discipline.write`, `discipline.admin`.
- Acuerdos legales, privacidad (datos sensibles).

---
*Blueprint para Sanciones.*

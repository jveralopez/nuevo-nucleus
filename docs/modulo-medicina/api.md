# API propuesta · Medicina Laboral

Base path: `/api/medicina/v1`

## Exámenes médicos
- `GET /examenes?legajoId=&tipo=&estado=`
- `POST /examenes`
```json
{
  "legajoId": "LEG-001",
  "tipo": "Ingreso",
  "proveedorId": "PROV-01",
  "fecha": "2026-04-05",
  "notas": "Ingresar en ayunas"
}
```
- `PUT /examenes/{id}` (resultado, apto/no apto, adjuntos).
- `POST /examenes/{id}/notificar` (envía resultado a empleado/jefe).

## Licencias médicas
- `GET /licencias?legajoId=&estado=`
- `POST /licencias`
```json
{
  "legajoId": "LEG-001",
  "tipo": "Enfermedad",
  "fechaDesde": "2026-03-15",
  "fechaHasta": "2026-03-20",
  "diagnostico": "Gripe",
  "adjuntos": ["certificado.pdf"]
}
```
- `POST /licencias/{id}/aprobar`, `POST /licencias/{id}/rechazar`.
- `POST /licencias/{id}/cerrar` (alta médica).

### Integración con Tiempos
- Al aprobar licencias médicas se registra ausencia en `tiempos-service` (`POST /ausencias`).
- Reporte básico: `GET /ausencias/resumen` devuelve resumen por tipo y por legajo.

### Notificaciones
- Al aprobar o rechazar exámenes médicos se genera notificación en Portal Empleado (via Portal BFF).

## Proveedores / campañas
- `GET /proveedores`, `POST /proveedores`.
- `GET /campanias`, `POST /campanias` (vacunas, chequeos masivos), `POST /campanias/{id}/inscribir`.

## Accidentes / ART
- `GET /accidentes?legajoId=&estado=`
- `POST /accidentes` (detalles, notificaciones a ART, seguimiento).

## Eventos
- `MedicalExamScheduled`, `MedicalExamResult`, `MedicalLicenseApproved`, `MedicalLicenseClosed`, `MedicalCampaignLaunched`.

## Seguridad
- Scopes: `medicina.read`, `medicina.write`, `medicina.admin`.
- Datos sensibles → cifrado, auditoría, controles por rol.

---
*Blueprint para modernizar Medicina Laboral.*

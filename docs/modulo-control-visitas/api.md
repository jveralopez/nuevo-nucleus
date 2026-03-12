# API propuesta · Control de Visitas

Base path: `/api/visitas/v1`

## Invitaciones
- `GET /invitaciones?hostId=&fecha=`
- `POST /invitaciones`
```json
{
  "hostLegajoId": "LEG-005",
  "visitante": {
    "nombre": "Carlos Perez",
    "empresa": "Proveedor SA",
    "email": "carlos@proveedor.com",
    "documento": "12345678"
  },
  "fecha": "2026-04-05T14:00:00Z",
  "notas": "Reunión proyecto X"
}
```
- `POST /invitaciones/{id}/aprobar`, `POST /invitaciones/{id}/rechazar`.
- `POST /invitaciones/{id}/reenviar` (email/QR).

## Visitas (check-in/out)
- `POST /visitas/{invitationId}/checkin`
```json
{
  "usuario": "recepcion-01",
  "badgeId": "B-123",
  "observaciones": "Entrega de badge"
}
```
- `POST /visitas/{invitationId}/checkout`.
- `GET /visitas?fecha=&estado=`

## Seguridad / alertas
- `GET /alertas?estado=` (visitantes bloqueados, accesos no autorizados).
- `POST /alertas` (registrar incidente, notificar seguridad).

## Configuración
- `GET /hosts/autorizados`
- `POST /areas` / `POST /areas/{id}/accesos` (definir áreas permitidas).
- `GET /proveedores` / `POST /proveedores` (empresas invitadas frecuentes).

## Eventos
- `VisitInvitationCreated`, `VisitApproved`, `VisitCheckedIn`, `VisitCheckedOut`, `VisitAlertRaised`.

## Seguridad
- Scopes: `visitas.read`, `visitas.write`, `visitas.security`.
- Validar que hosts sólo creen invitaciones en sus áreas permitidas.

---
*Blueprint para el módulo de Control de Visitas.*

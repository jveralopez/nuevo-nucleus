# API propuesta · Beneficios

Base path: `/api/beneficios/v1`

## Catálogo
- `GET /beneficios?categoria=&estado=&proveedor=`
- `POST /beneficios`
```json
{
  "nombre": "Seguro Médico Premium",
  "categoria": "Salud",
  "descripcion": "Cobertura integral",
  "costo": 30000,
  "moneda": "ARS",
  "proveedorId": "PROV-001",
  "elegibilidad": {
    "antiguedadMeses": 6,
    "pais": "AR"
  }
}
```
- `PUT /beneficios/{id}` / `DELETE`.

## Inscripciones
- `POST /beneficios/{id}/inscripciones`
```json
{ "legajoId": "LEG-050", "plan": "Familiar" }
```
- `GET /beneficios/{id}/inscripciones`, `GET /inscripciones?legajoId=&estado=`
- `POST /inscripciones/{id}/aprobar` / `rechazar`, `POST /inscripciones/{id}/cancelar`.

## Reembolsos / claims
- `POST /inscripciones/{id}/reembolsos`
```json
{ "monto": 15000, "moneda": "ARS", "concepto": "Gimnasio", "adjuntos": ["ticket.pdf"] }
```
- `POST /reembolsos/{id}/aprobar` / `rechazar`.

## Wallet flexible
- `GET /wallets/{legajoId}`
- `POST /wallets/{legajoId}/movimientos` (cargar/descargar saldo).

## Integraciones
- `POST /exportes/proveedores` (envía altas/bajas a proveedor).
- `POST /exportes/liquidacion` (beneficios monetarios para descuentos/pagos).

## Eventos
- `BenefitCreated`, `BenefitEnrollmentRequested`, `BenefitEnrollmentApproved`, `BenefitClaimSubmitted`, `BenefitClaimApproved`.

## Seguridad
- Scopes: `beneficios.read`, `beneficios.write`, `beneficios.admin`.
- Controles para datos sensibles (salud, dependientes).

---
*Blueprint para Beneficios.*

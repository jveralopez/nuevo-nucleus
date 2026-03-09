# API propuesta · Tesorería

Base path: `/api/tesoreria/v1`

## Solicitudes de pago
- `GET /solicitudes?legajoId=&tipo=&estado=`
- `POST /solicitudes`
```json
{
  "legajoId": "LEG-010",
  "tipo": "Adelanto",
  "monto": 150000,
  "moneda": "ARS",
  "motivo": "Gastos médicos",
  "adjuntos": ["factura.pdf"]
}
```
- `POST /solicitudes/{id}/aprobar` / `rechazar`.
- `POST /solicitudes/{id}/programar` (fecha de pago, cuenta, método).

## Pagos
- `GET /pagos?periodo=&cuenta=&estado=`
- `POST /pagos`
```json
{
  "solicitudId": "SOL-123",
  "fechaPago": "2026-03-25",
  "metodo": "Transferencia",
  "cuenta": "BANCO-GALICIA",
  "referencia": "OP-20260325-001"
}
```
- `POST /pagos/{id}/conciliar` (vincular con extracto bancario).

## Fondos / conciliaciones
- `GET /fondos`, `POST /fondos` (fondo fijo, caja chica).
- `GET /conciliaciones?periodo=&cuenta=`
- `POST /conciliaciones`
```json
{
  "periodo": "2026-03",
  "cuenta": "BANCO-GALICIA",
  "montoBanco": 12500000,
  "montoSistema": 12480000,
  "diferencia": 20000,
  "notas": "Pendiente ingresar pago EP-123"
}
```

## Integraciones
- `POST /exportes/bancos` (genera archivo/transfer para un lote de pagos).
- `POST /exportes/contables` (asientos para ERP).

## Eventos
- `PaymentRequestCreated`, `PaymentRequestApproved`, `PaymentScheduled`, `PaymentCompleted`, `PaymentReconciled`.

## Seguridad
- Scopes: `treasury.read`, `treasury.write`, `treasury.approve`.
- SoD: distintos roles para solicitar, aprobar y ejecutar pagos.

---
*Blueprint para Tesorería.*

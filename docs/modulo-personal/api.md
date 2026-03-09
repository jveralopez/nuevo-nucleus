# API propuesta · Personal Service

Base path: `/api/personal/v1`

## Legajos
- `GET /legajos?empresaId=&posicionId=&search=` → Lista paginada.
- `POST /legajos` → Alta. Body:
```json
{
  "numero": "00045",
  "nombre": "Ana",
  "apellido": "Nieto",
  "cuil": "27-12345678-9",
  "sexo": "F",
  "fechaIngreso": "2023-01-10",
  "empresaId": "EMP01",
  "posicionId": "POS123",
  "estado": "Activo"
}
```
- `GET /legajos/{id}` / `PUT` / `DELETE`.
- `GET /legajos/{id}/liquidacion-profile` → Devuelve: datos personales (CUIL, documento), laborales (empresa, posición, sindicato, centro de costo), domicilio fiscal.

## Familias y documentos
- `GET /legajos/{id}/familiares`, `POST`, `PUT`, `DELETE` (campos: tipo, nombre, documento, fecha nacimiento, esCarga).
- `GET /legajos/{id}/documentos`, `POST` (tipo, número, vigencia, archivo opcional).
- `GET /legajos/{id}/domicilios`, `POST` (tipo, calle, número, localidadId, esFiscal).

## Solicitudes de cambio
- `POST /legajos/{id}/solicitudes`
```json
{
  "tipo": "DatosPersonales",
  "payload": {
    "nombre": "Ana María",
    "domicilio": {
      "calle": "Av. Siempre Viva",
      "numero": "742",
      "localidadId": "LOC123"
    }
  }
}
```
- `GET /legajos/{id}/solicitudes?estado=PENDAPROB`
- `POST /solicitudes/{solId}/aprobar`, `POST /solicitudes/{solId}/rechazar`.

## Catálogos
- `GET /catalogos/tipos-documento`
- `GET /catalogos/nacionalidades`
- `GET /catalogos/tipos-domicilio`
- `GET /catalogos/grupos-sanguineos`

## Eventos
- `LegajoCreated`, `LegajoUpdated`, `LegajoDeleted`, `LegajoSolicitudAprobada`.
  - Payload: `{ legajoId, numero, empresaId, cambio: { campo, valorAnterior, valorNuevo }, timestamp }`.

## Seguridad / Auditoría
- Endpoints protegidos con OIDC + scopes (`personal.read`, `personal.write`).
- Auditoría automática en cada `PUT/POST` con metadata (usuario, timestamp, campos modificados).

---
*Inspirado en los métodos `GetPer`, `GetFams`, `EditPer`, `AprobarSolicitud` de la versión 23.01.*

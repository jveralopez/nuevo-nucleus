# API propuesta · Personal Service

Base path: `/api/personal/v1`

## Legajos
- `GET /legajos?estado=` → Lista.
- `POST /legajos` → Alta. Body:
```json
{
  "numero": "00045",
  "nombre": "Ana",
  "apellido": "Nieto",
  "cuil": "27-12345678-9",
  "fechaIngreso": "2023-01-10",
  "convenio": "CCT-130-75",
  "categoria": "Administrativo A",
  "obraSocial": "OSDE",
  "sindicato": "FAECYS",
  "tipoPersonal": "1",
  "ubicacion": "CASA_CENTRAL",
  "estado": "Activo",
  "familiares": [],
  "licencias": []
}
```
- `GET /legajos/{id}` / `PUT` / `DELETE`.

## Familias y licencias
- `GET /legajos/{id}/familiares` → Lista familiares.
- `PUT /legajos/{id}/familiares` → Reemplaza familiares.
- `GET /legajos/{id}/licencias` → Lista licencias.
- `PUT /legajos/{id}/licencias` → Reemplaza licencias.

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

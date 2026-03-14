# Validacion modulo Tiempos 2026-03-13

## Entorno
- Docker compose prod con env:
  - `AUTH_ISSUER=nucleus-auth`
  - `AUTH_AUDIENCE=nucleus-api`
  - `AUTH_SIGNING_KEY=CHANGE_ME_SUPER_SECRET_KEY_32_CHARS_LONG`
  - `DB_APPLY_MIGRATIONS=true`
  - `OTEL_ENABLED=false`
- Portal RH servido local: `python -m http.server 4173` en `portal-rh-ui/`.

## Validacion BFF
- Login admin OK (`POST http://localhost:5001/login`).
- `GET /api/rh/v1/tiempos/turnos` -> 200.
- `GET /api/rh/v1/tiempos/horarios` -> 200.
- `GET /api/rh/v1/tiempos/fichadas` -> 200.
- `GET /api/rh/v1/tiempos/planillas` -> 200.

## Validacion UI (Portal RH)
- Crear legajo demo (Personal): `900 · Ana`.
- Crear turno: `TUR-DIA` y `TUR-DIA2`.
- Crear horario: `Semana Base`.
- Registrar fichada: `Entrada`.
- Crear planilla: periodo `2026-03`, 168 hs.
- Cerrar planilla: estado `Cerrada`.

## Observaciones
- Se registran errores 404 en consola para `/api/rh/v1/integraciones/triggers` (no bloquea modulo Tiempos).
- Los endpoints de Tiempos responden OK via BFF.

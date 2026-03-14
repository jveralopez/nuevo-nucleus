# Mejoras detectadas

## Accesos y CORS
- Portal RH recibe 401 en organizacion/personal/liquidacion: validar que el token se envia con prefijo `Bearer` y que los servicios aceptan el issuer/audience actuales.
- Portal Empleado falla por CORS tanto via BFF (5090) como directo a servicios: configurar CORS para `http://localhost:3002` en auth, liquidacion y workflow, y en portal-bff.

### Estado (2026-03-13)
- CORS actualizado en servicios y BFF para permitir `Origin: null` (UI servida via `file://`) solo en Development y orígenes configurados.
- E2E validado en Portal RH y Portal Empleado (Playwright).

## Integraciones
- Integration Hub responde health pero `/templates`, `/jobs`, `/triggers` devuelven 404: revisar rutas expuestas o base path esperado por el UI.

### Estado (2026-03-13)
- Portal BFF ahora expone `/api/rh/v1/integraciones/triggers` (GET/POST/PUT/execute).

## UX y observabilidad
- Mostrar mensaje de error con detalle HTTP y sugerir accion (por ejemplo: "falta Bearer" o "CORS no habilitado").
- Agregar indicador de estado para cada modulo (auth/org/personal/liq/hub/wf) con resultado del health check.

### Estado (2026-03-13)
- Portal RH y Portal Empleado muestran estado de servicios (health) y errores con detalle HTTP.

## Operaciones core RRHH
- Reclamos y Sanciones operativos con workflow base y UI de gestión.
- Auditoría e idempotencia en WF para evitar transiciones duplicadas.
- Correlación e idempotencia en BFF y servicios core.

## Calidad y automatizacion
- Añadir suite E2E automatizada (Playwright) con datos semilla para evitar 401/CORS en entorno local.
- Incorporar migraciones de EF para evitar inconsistencias cuando se agregan tablas nuevas.

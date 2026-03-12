# Validacion de Portales 2026-03-12

## Verificacion estatica
- `portal-rh-ui/index.html`, `app.js`, `styles.css` presentes.
- `portal-empleado-ui/index.html`, `app.js`, `styles.css` presentes.
- `liquidacion-ui/index.html`, `app.js`, `styles.css` presentes.

## Verificacion visual (Playwright)
- Portal RH: carga OK, `Login demo` autentica, pero llamadas a APIs directas responden 401.
- Portal Empleado: carga OK, `Login demo` autentica, pero endpoints BFF `/api/portal/v1/*` responden 404.
- Liquidacion UI: carga OK, pero `/payrolls` responde 401 (alerta de no autorizado).

## Resultado
- UI renderiza correctamente.
- Flujos con datos requieren revisar autenticacion/headers y endpoints BFF faltantes.

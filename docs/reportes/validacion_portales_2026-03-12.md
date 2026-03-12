# Validacion de Portales 2026-03-12

## Verificacion estatica
- `portal-rh-ui/index.html`, `app.js`, `styles.css` presentes.
- `portal-empleado-ui/index.html`, `app.js`, `styles.css` presentes.
- `liquidacion-ui/index.html`, `app.js`, `styles.css` presentes.

## Verificacion visual (Playwright)
- Portal RH: carga OK usando BFF por defecto, token presente; requests 200 (sin datos).
- Portal Empleado: carga OK con BFF, sin errores de consola (sin datos).
- Liquidacion UI: token cargado manualmente, `/payrolls` responde 200 (sin datos).

## Resultado
- UI renderiza correctamente.
- Flujos con datos quedan pendientes de seed; la autenticacion funciona.

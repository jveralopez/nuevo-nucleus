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
- Seed data disponible en `seed-data.js` para populacion de bases de datos.
- La autenticacion funciona correctamente.

## Actualizacion (2026-03-13)
- E2E con Playwright: Portal RH y Portal Empleado OK (login demo + refresh sin 401).
- Validacion manual: Portal RH carga datos base y Portal Empleado muestra listas sin datos (API 200).
- Portal RH: vistas de Reclamos y Sanciones operativas.

## Seed Data
- Archivo: `seed-data.js`
- Contiene datos de prueba para: auth, organizacion, personal, liquidacion, tiempos, vacaciones, seleccion, evaluacion, capacitacion, clima.
- Para usar: cada servicio tiene su propio endpoint o puede insertarse directamente en SQLite.
- Portal Empleado: formulario de descargo por sanción disponible.
- Portal Empleado: formularios de Medicina Laboral (examen/licencia).
- Portal RH: bandejas de Medicina Laboral (examen/licencia).

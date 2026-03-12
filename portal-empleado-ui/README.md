# Portal Empleado UI (MVP)

UI estatica para demo de autoservicio (workflows + recibos con detalle + notificaciones + export local + limpiar + exportes con JWT via BFF + resumen).

Incluye `recibo.html` como render imprimible de un recibo de sueldo.

Abrí `recibo.html` para cargar JSON y preparar impresión.

## Uso
1. Ejecutar `auth-service`, `liquidacion-service`, `nucleuswf-service` y `portal-bff-service`.
2. Abrir `index.html`.
3. Ingresar credenciales (admin/admin123) y hacer login.
4. Cargar workflows demo y crear solicitudes.
5. Configurar Portal BFF para notificaciones, exportes y workflows.

Si no configurás BFF, el portal usa `liquidacion-service` y `nucleuswf-service` directos.

Los endpoints usan `Authorization: Bearer <token>`.

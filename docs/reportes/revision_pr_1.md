# Revision PR-1

## Auditoria de archivos grandes y binarios

### Binarios trackeados (revisar licencias y necesidad)
- `data/ganancias/sources/Deducciones-personales-art-30-ene-a-jun-2026.pdf` (573573 bytes)
- `data/ganancias/sources/Tabla-Art-94-LIG-per-ene-a-jun-2026.pdf` (598758 bytes)
- `portal-empleado-cors-error.png` (338196 bytes)
- `portal-empleado-dashboard-error.png` (338147 bytes)
- `portal-rh-dashboard.png` (270856 bytes)
- `portal-rh-liquidacion-error.png` (189073 bytes)
- `portal-rh-organizacion-error.png` (256814 bytes)
- `test/evidencia-superposicion.png` (661053 bytes)

### Archivos grandes no trackeados
- Se detectaron DLLs > 5MB bajo `**/bin/**` producto de builds locales. Estan ignorados por `.gitignore`.

### Resultado
- No se encontraron binarios grandes fuera de `data/` y `test/`.
- PDFs y PNGs son assets de referencia/demos; mantener vigilancia de tamaño y licencia.

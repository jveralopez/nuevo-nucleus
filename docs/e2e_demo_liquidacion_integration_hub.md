# Demo end-to-end · Liquidacion -> Integration Hub (Banco Galicia)

## Objetivo
Validar flujo completo con JWT: crear template, publicar, procesar liquidacion y disparar job.

## Precondiciones
- `auth-service`, `integration-hub-service`, `liquidacion-service` en ejecucion.
- `integration-hub-service` y `liquidacion-service` configurados con `Auth`.

## Pasos
1. Obtener JWT Admin
```bash
curl -s -X POST http://localhost:5001/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

2. Crear template Banco Galicia
```bash
curl -s -X POST http://localhost:5050/integraciones/templates \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN>" \
  -d @docs/modulo-integraciones/template-banco-galicia.json
```

3. Publicar template
```bash
curl -s -X POST http://localhost:5050/integraciones/templates/<TEMPLATE_ID>/publish \
  -H "Authorization: Bearer <TOKEN>"
```

4. Configurar IntegrationHub en `liquidacion-service/appsettings.json`
- `IntegrationHub.TemplateId = <TEMPLATE_ID>`
- `IntegrationHub.AccessToken = <TOKEN>`

5. Crear liquidacion
```bash
curl -s -X POST http://localhost:5188/payrolls \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN>" \
  -d '{"periodo":"2026-02","tipo":"Mensual","descripcion":"Demo"}'
```

6. Agregar legajo
```bash
curl -s -X POST http://localhost:5188/payrolls/<ID>/legajos \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN>" \
  -d '{"numero":"100","nombre":"Ana Perez","cuil":"20-00000000-0","basico":1000,"antiguedad":100,"adicionales":50,"descuentos":10}'
```

7. Procesar + exportar (dispara Integration Hub)
```bash
curl -s -X POST http://localhost:5188/payrolls/<ID>/procesar \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN>" \
  -d '{"exportar":true}'
```

8. Ver job en Integration Hub
```bash
curl -s http://localhost:5050/integraciones/jobs \
  -H "Authorization: Bearer <TOKEN>"
```

## Resultado esperado
- Liquidacion en estado Exportado.
- Job creado en Integration Hub con estado Completado.
- Archivo en `integration-hub-service/storage/exports/`.

## Opcional: Portal Empleado MVP
1. Abrir `portal-empleado-ui/index.html`.
2. Hacer login con el mismo JWT.
3. Cargar liquidaciones desde el portal.

## Opcional: Portal BFF
1. Ejecutar `portal-bff-service`.
2. Consumir `http://localhost:5002/api/portal/v1/home` con JWT.

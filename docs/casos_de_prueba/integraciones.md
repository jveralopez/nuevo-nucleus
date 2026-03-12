# Casos de prueba · Integraciones

## CT-INT-01 Listar templates
- **Relacion**: CU-RH-12, CU-RH-15
- **Precondiciones**: Integration Hub operativo.
- **Pasos**:
  1. Ejecutar `GET /templates`.
- **Resultado esperado**: Lista de templates.

## CT-INT-02 Crear job de exporte
- **Relacion**: CU-RH-12
- **Precondiciones**: Template valido.
- **Pasos**:
  1. Ejecutar `POST /jobs` con templateId y payload.
- **Resultado esperado**: Job creado.

## CT-INT-03 Ver detalle de job
- **Relacion**: CU-RH-13
- **Precondiciones**: Job existente.
- **Pasos**:
  1. Ejecutar `GET /jobs/{id}`.
- **Resultado esperado**: Detalle completo.

## CT-INT-04 Reintentar job
- **Relacion**: CU-RH-14
- **Precondiciones**: Job en estado fallido.
- **Pasos**:
  1. Ejecutar `POST /jobs/{id}/retry`.
- **Resultado esperado**: Nuevo intento registrado.

## CT-INT-05 Triggers y eventos
- **Relacion**: CU-RH-16, CU-RH-17, CU-RH-18, CU-RH-19
- **Precondiciones**: Template y trigger disponibles.
- **Pasos**:
  1. Crear trigger.
  2. Ejecutar trigger.
  3. Listar eventos.
- **Resultado esperado**: Job generado y eventos visibles.

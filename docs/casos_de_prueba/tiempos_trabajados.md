# Casos de prueba · Tiempos Trabajados

## CT-TT-01 Gestionar turnos
- **Relacion**: CU-TT-01
- **Precondiciones**: JWT Admin.
- **Pasos**:
  1. Ejecutar `POST /turnos`.
  2. Ejecutar `PUT /turnos/{id}`.
- **Resultado esperado**: Turno creado y actualizado.

## CT-TT-02 Gestionar horarios
- **Relacion**: CU-TT-02
- **Precondiciones**: Turno existente.
- **Pasos**:
  1. Ejecutar `POST /horarios`.
  2. Ejecutar `PUT /horarios/{id}`.
- **Resultado esperado**: Horario creado y actualizado.

## CT-TT-03 Registrar fichada
- **Relacion**: CU-TT-03
- **Precondiciones**: Legajo existente.
- **Pasos**:
  1. Ejecutar `POST /fichadas`.
- **Resultado esperado**: Fichada creada.

## CT-TT-04 Consultar fichadas
- **Relacion**: CU-TT-04
- **Precondiciones**: Fichadas existentes.
- **Pasos**:
  1. Ejecutar `GET /fichadas?legajoId=&desde=&hasta=`.
- **Resultado esperado**: HTTP 200 con listado.

## CT-TT-05 Generar planilla
- **Relacion**: CU-TT-05
- **Precondiciones**: Fichadas existentes.
- **Pasos**:
  1. Ejecutar `POST /planillas`.
- **Resultado esperado**: Planilla creada.

## CT-TT-06 Cerrar planilla
- **Relacion**: CU-TT-06
- **Precondiciones**: Planilla creada.
- **Pasos**:
  1. Ejecutar `POST /planillas/{id}/cerrar`.
- **Resultado esperado**: Planilla cerrada.

## CT-TT-07 Exportar planilla
- **Relacion**: CU-TT-07
- **Precondiciones**: Planilla cerrada.
- **Pasos**:
  1. Ejecutar `POST /api/rh/v1/integraciones/jobs` con payload de exporte.
  2. Verificar `GET /api/rh/v1/integraciones/jobs/{id}` - estado "Completado".
- **Resultado esperado**: Job completado, archivo generado.

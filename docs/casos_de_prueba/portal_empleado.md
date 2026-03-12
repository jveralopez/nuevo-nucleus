# Casos de prueba · Portal Empleado

## CT-PE-01 Login demo
- **Relacion**: CU-PORT-01
- **Precondiciones**: Auth service operativo.
- **Pasos**:
  1. Abrir `portal-empleado-ui/index.html`.
  2. Click "Login demo".
- **Resultado esperado**: Estado "Con sesion" y token guardado.

## CT-PE-02 Ver liquidaciones y recibos
- **Relacion**: CU-PORT-02, CU-PORT-02B
- **Precondiciones**: Liquidacion procesada con recibos.
- **Pasos**:
  1. Refrescar datos.
  2. Abrir vista Recibos y cargar detalle.
- **Resultado esperado**: Listado y detalle visibles.

## CT-PE-03 Descargar recibos
- **Relacion**: CU-PORT-02D, CU-PORT-02F
- **Precondiciones**: Exportes disponibles.
- **Pasos**:
  1. Click "Descargar JSON".
  2. Click "Descargar CSV".
  3. Click "Descargar exportes".
- **Resultado esperado**: Descargas iniciadas.

## CT-PE-04 Notificaciones
- **Relacion**: CU-PORT-02C, CU-PORT-02E, CU-PORT-02H, CU-PORT-02I, CU-PORT-02J, CU-PORT-02K, CU-PORT-02L
- **Precondiciones**: BFF disponible o modo local.
- **Pasos**:
  1. Refrescar notificaciones.
  2. Filtrar no leidas.
  3. Marcar todas.
  4. Limpiar.
- **Resultado esperado**: Cambios visibles en el panel.

## CT-PE-05 Workflows demo y solicitudes
- **Relacion**: CU-PORT-05, CU-PORT-07, CU-PORT-08, CU-PORT-09
- **Precondiciones**: Workflow service operativo.
- **Pasos**:
  1. Click "Cargar workflows".
  2. Iniciar workflow generico.
  3. Enviar solicitud de vacaciones.
  4. Enviar datos personales.
  5. Enviar reclamo.
- **Resultado esperado**: Solicitudes creadas y listadas.

## CT-PE-06 Transiciones
- **Relacion**: CU-PORT-04, CU-PORT-06
- **Precondiciones**: Instancias existentes.
- **Pasos**:
  1. Aplicar transicion desde la vista Tareas.
- **Resultado esperado**: Estado actualizado.

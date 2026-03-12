# Casos de prueba · Liquidación

## CT-LIQ-01 Listar liquidaciones
- **Relacion**: CU-LIQ-01
- **Precondiciones**: Auth service operativo, JWT valido.
- **Pasos**:
  1. Ejecutar `GET /payrolls`.
- **Resultado esperado**: HTTP 200 y lista (vacia o con elementos).

## CT-LIQ-02 Crear liquidacion
- **Relacion**: CU-LIQ-02
- **Precondiciones**: JWT Admin.
- **Pasos**:
  1. Ejecutar `POST /payrolls` con periodo, tipo, descripcion.
- **Resultado esperado**: HTTP 200/201 y `id` generado.

## CT-LIQ-03 Agregar legajo a liquidacion
- **Relacion**: CU-LIQ-06
- **Precondiciones**: Liquidacion en Draft.
- **Pasos**:
  1. Ejecutar `POST /payrolls/{id}/legajos` con datos del legajo.
- **Resultado esperado**: HTTP 200 y legajo asociado.

## CT-LIQ-04 Procesar liquidacion sin exporte
- **Relacion**: CU-LIQ-03
- **Precondiciones**: Legajos cargados.
- **Pasos**:
  1. Ejecutar `POST /payrolls/{id}/procesar` con `Exportar=false`.
- **Resultado esperado**: Estado `Procesado` y recibos generados.

## CT-LIQ-05 Procesar liquidacion con exporte
- **Relacion**: CU-LIQ-03, CU-LIQ-05
- **Precondiciones**: Legajos cargados.
- **Pasos**:
  1. Ejecutar `POST /payrolls/{id}/procesar` con `Exportar=true`.
  2. Ejecutar `GET /payrolls/{id}/exports`.
- **Resultado esperado**: Archivos JSON/CSV listados.

## CT-LIQ-06 Consultar recibos
- **Relacion**: CU-LIQ-04
- **Precondiciones**: Liquidacion procesada.
- **Pasos**:
  1. Ejecutar `GET /payrolls/{id}/recibos`.
- **Resultado esperado**: Recibos con `detalle` no vacio.

## CT-LIQ-07 Exportes empleado
- **Relacion**: CU-LIQ-07
- **Precondiciones**: Liquidacion exportada, JWT usuario.
- **Pasos**:
  1. Ejecutar `GET /payrolls/{id}/exports/empleado`.
- **Resultado esperado**: Lista de archivos disponible.

## CT-LIQ-08 Calculo de ganancias
- **Relacion**: CU-LIQ-08
- **Precondiciones**: art30/art94 actualizados, legajo con AplicaGanancias.
- **Pasos**:
  1. Procesar liquidacion.
  2. Verificar recibo con concepto "Impuesto a las ganancias".
- **Resultado esperado**: Deduccion aplicada segun tabla.

## CT-LIQ-09 Aplicar embargos
- **Relacion**: CU-LIQ-09
- **Precondiciones**: Legajo con embargo activo.
- **Pasos**:
  1. Procesar liquidacion.
  2. Verificar recibo con concepto "Embargos".
- **Resultado esperado**: Deduccion por embargo aplicada.

## CT-LIQ-10 Aplicar licencias y vacaciones
- **Relacion**: CU-LIQ-10
- **Precondiciones**: Legajo con licencias y/o workflow aprobado.
- **Pasos**:
  1. Procesar con `AplicarVacacionesWorkflow=true`.
  2. Verificar conceptos de vacaciones/licencias.
- **Resultado esperado**: Ajustes reflejados en el recibo.

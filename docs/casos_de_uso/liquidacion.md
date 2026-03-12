# Casos de uso · Liquidacion

## CU-LIQ-01 Consultar liquidaciones
- **Actor**: Usuario autenticado
- **Objetivo**: Ver lista de liquidaciones.
- **Precondiciones**: Token JWT valido.
- **Flujo principal**:
  1. El actor consulta `GET /payrolls`.
  2. El sistema devuelve la lista.
- **Postcondicion**: Datos visibles.

## CU-LIQ-02 Crear liquidacion
- **Actor**: Admin
- **Objetivo**: Crear ciclo de liquidacion.
- **Precondiciones**: Rol Admin.
- **Flujo principal**:
  1. El actor envia periodo/tipo/descripcion.
  2. El sistema crea la liquidacion.
- **Postcondicion**: Liquidacion creada.

## CU-LIQ-03 Procesar liquidacion
- **Actor**: Admin
- **Objetivo**: Calcular recibos y opcionalmente exportar.
- **Precondiciones**: Liquidacion con legajos.
- **Flujo principal**:
  1. El actor ejecuta `POST /payrolls/{id}/procesar`.
  2. El sistema calcula recibos y actualiza estado.
  3. Si exporta, genera archivos.
- **Postcondicion**: Liquidacion procesada/exportada.

## CU-LIQ-04 Consultar recibos
- **Actor**: Usuario autenticado
- **Objetivo**: Ver recibos de una liquidacion.
- **Precondiciones**: JWT valido.
- **Flujo principal**:
  1. El actor consulta `GET /payrolls/{id}/recibos`.
  2. El sistema devuelve recibos.
- **Postcondicion**: Recibos visibles.

## CU-LIQ-05 Consultar exportes disponibles
- **Actor**: Admin
- **Objetivo**: Obtener exportes listos para descargar.
- **Precondiciones**: Liquidacion exportada. Rol Admin.
- **Flujo principal**:
  1. El actor consulta `GET /payrolls/{id}/exports`.
  2. El sistema devuelve archivos disponibles.
- **Postcondicion**: Exportes listados.

## CU-LIQ-06 Agregar legajo a liquidacion
- **Actor**: Admin
- **Objetivo**: Incluir legajo en el lote.
- **Precondiciones**: Liquidacion en estado Draft.
- **Flujo principal**:
  1. El actor envia datos del legajo.
  2. El sistema agrega el legajo al lote.
- **Postcondicion**: Legajo agregado.

## CU-LIQ-07 Consultar exportes empleado
- **Actor**: Usuario autenticado
- **Objetivo**: Descargar exportes generados.
- **Precondiciones**: Liquidacion exportada.
- **Flujo principal**:
  1. El actor consulta `GET /payrolls/{id}/exports/empleado`.
  2. El sistema devuelve archivos.
- **Postcondicion**: Exportes listados.

## CU-LIQ-08 Calcular impuesto a las ganancias
- **Actor**: Admin
- **Objetivo**: Aplicar Ganancias segun reglas vigentes.
- **Precondiciones**: Tablas Art. 30 y Art. 94 actualizadas. Legajo con AplicaGanancias.
- **Flujo principal**:
  1. El actor procesa la liquidacion.
  2. El sistema calcula deducciones personales y alicuota.
  3. El recibo incluye el concepto de Ganancias cuando corresponde.
- **Postcondicion**: Deduccion de Ganancias aplicada en el recibo.

## CU-LIQ-09 Aplicar embargos
- **Actor**: Admin
- **Objetivo**: Descontar embargos segun base y porcentaje/monto.
- **Precondiciones**: Legajo con embargos activos.
- **Flujo principal**:
  1. El actor procesa la liquidacion.
  2. El sistema calcula el embargo sobre la base configurada.
  3. El recibo agrega el concepto de embargo.
- **Postcondicion**: Embargo descontado en el neto.

## CU-LIQ-10 Aplicar licencias y vacaciones workflow
- **Actor**: Admin
- **Objetivo**: Ajustar el recibo por dias de licencia y vacaciones.
- **Precondiciones**: Legajo con licencias y/o workflow de vacaciones aprobado.
- **Flujo principal**:
  1. El actor procesa la liquidacion con `AplicarVacacionesWorkflow`.
  2. El sistema calcula vacaciones y licencias con o sin goce.
  3. El recibo refleja los conceptos correspondientes.
- **Postcondicion**: Conceptos de vacaciones/licencias aplicados.

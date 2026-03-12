# Casos de prueba · Portal RH

## CT-RH-01 Login demo
- **Relacion**: CU-RH-01
- **Precondiciones**: Auth service operativo.
- **Pasos**:
  1. Abrir `portal-rh-ui/index.html`.
  2. Click "Login demo".
- **Resultado esperado**: Estado "Con sesion" y token guardado.

## CT-RH-02 Crear/editar empresa
- **Relacion**: CU-RH-01, CU-RH-02
- **Precondiciones**: JWT Admin.
- **Pasos**:
  1. Crear empresa desde formulario.
  2. Editar la empresa creada.
- **Resultado esperado**: Empresa aparece en listas y cambios persistidos.

## CT-RH-03 Crear unidad
- **Relacion**: CU-RH-03
- **Precondiciones**: Empresa existente.
- **Pasos**:
  1. Crear unidad con empresa seleccionada.
- **Resultado esperado**: Unidad visible en listado.

## CT-RH-04 Crear posicion y asignar legajo
- **Relacion**: CU-RH-04, CU-RH-05
- **Precondiciones**: Unidad y legajo existentes.
- **Pasos**:
  1. Crear posicion.
  2. Asignar legajo a posicion.
- **Resultado esperado**: Asignacion reflejada.

## CT-RH-05 Crear/editar liquidacion
- **Relacion**: CU-RH-06, CU-RH-07
- **Precondiciones**: JWT Admin.
- **Pasos**:
  1. Crear liquidacion.
  2. Editar periodo/tipo.
- **Resultado esperado**: Liquidacion actualizada.

## CT-RH-06 Procesar y exportar
- **Relacion**: CU-RH-08, CU-RH-09
- **Precondiciones**: Legajos cargados.
- **Pasos**:
  1. Procesar/exportar liquidacion.
  2. Ver listado de exportes.
- **Resultado esperado**: Exportes disponibles.

## CT-RH-07 Ver detalle de liquidacion
- **Relacion**: CU-RH-10
- **Precondiciones**: Liquidacion procesada.
- **Pasos**:
  1. Cargar recibos y detalle.
- **Resultado esperado**: Recibos con detalle visible.

## CT-RH-08 Gestion de vacaciones
- **Relacion**: CU-RH-11
- **Precondiciones**: Solicitudes creadas en workflow.
- **Pasos**:
  1. Listar solicitudes.
  2. Aprobar/rechazar.
- **Resultado esperado**: Estado actualizado.

## CT-RH-09 Integraciones: jobs y eventos
- **Relacion**: CU-RH-12 a CU-RH-17
- **Precondiciones**: Integration Hub operativo.
- **Pasos**:
  1. Iniciar job desde template.
  2. Consultar detalle.
  3. Ejecutar trigger y listar eventos.
- **Resultado esperado**: Job creado, eventos visibles.

## CT-RH-10 Triggers
- **Relacion**: CU-RH-18 a CU-RH-19
- **Precondiciones**: Template disponible.
- **Pasos**:
  1. Crear trigger.
  2. Editar trigger.
- **Resultado esperado**: Trigger creado y actualizado.

## CT-RH-11 Sindicato y convenio
- **Relacion**: CU-RH-20 a CU-RH-23
- **Precondiciones**: JWT Admin.
- **Pasos**:
  1. Crear sindicato y editarlo.
  2. Crear convenio y editarlo.
- **Resultado esperado**: Registros visibles.

## CT-RH-12 Versionar organigrama via BFF
- **Relacion**: CU-RH-24
- **Precondiciones**: Portal BFF configurado, JWT Admin, unidades cargadas.
- **Pasos**:
  1. Ejecutar `POST /api/rh/v1/organizacion/organigramas`.
  2. Ejecutar `GET /api/rh/v1/organizacion/organigramas`.
- **Resultado esperado**: HTTP 201 en la creacion y HTTP 200 con listado.

## CT-RH-13 Flujo organigramas en UI
- **Relacion**: CU-RH-24
- **Precondiciones**: Portal RH abierto, config-url del BFF seteado, JWT Admin.
- **Pasos**:
  1. Ingresar a Organizacion en Portal RH.
  2. Crear unidades organizativas si no existen.
  3. Ejecutar versionado desde el formulario asociado (POST via BFF).
  4. Verificar listado de versiones en UI (GET via BFF).
- **Resultado esperado**: Version creada y visible en listado.

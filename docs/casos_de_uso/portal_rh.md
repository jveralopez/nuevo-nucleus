# Casos de uso · Portal RH

## CU-RH-01 Crear empresa
- **Actor**: RRHH Admin
- **Objetivo**: Registrar una empresa.
- **Precondiciones**: JWT Admin.
- **Flujo principal**:
  1. Completa formulario.
  2. El sistema crea la empresa.
- **Postcondicion**: Empresa visible.

## CU-RH-02 Editar empresa
- **Actor**: RRHH Admin
- **Objetivo**: Actualizar datos.
- **Precondiciones**: Empresa existente.
- **Flujo principal**:
  1. Selecciona empresa.
  2. Actualiza campos.
  3. El sistema guarda.
- **Postcondicion**: Empresa actualizada.

## CU-RH-03 Crear unidad
- **Actor**: RRHH Admin
- **Objetivo**: Crear unidad organizativa.
- **Precondiciones**: Empresa existente.
- **Flujo principal**:
  1. Selecciona empresa.
  2. Completa datos.
  3. El sistema crea unidad.
- **Postcondicion**: Unidad visible.

## CU-RH-04 Crear posición
- **Actor**: RRHH Admin
- **Objetivo**: Crear posición.
- **Precondiciones**: Unidad existente.
- **Flujo principal**:
  1. Selecciona unidad.
  2. Completa datos.
  3. El sistema crea posición.
- **Postcondicion**: Posición visible.

## CU-RH-05 Asignar legajo a posición
- **Actor**: RRHH Admin
- **Objetivo**: Asociar legajo.
- **Precondiciones**: Legajo y posición existentes.
- **Flujo principal**:
  1. Selecciona posición y legajo.
  2. El sistema asigna.
- **Postcondicion**: Posición actualizada.

## CU-RH-06 Crear liquidación
- **Actor**: RRHH Admin
- **Objetivo**: Crear ciclo de liquidación.
- **Precondiciones**: JWT Admin.
- **Flujo principal**:
  1. Completa formulario.
  2. El sistema crea.
- **Postcondicion**: Liquidación visible.

## CU-RH-07 Editar liquidación
- **Actor**: RRHH Admin
- **Objetivo**: Actualizar datos de liquidación.
- **Precondiciones**: Liquidación existente.
- **Flujo principal**:
  1. Selecciona liquidación.
  2. Actualiza datos.
  3. El sistema guarda.
- **Postcondicion**: Liquidación actualizada.

## CU-RH-08 Procesar y exportar liquidación
- **Actor**: RRHH Admin
- **Objetivo**: Generar recibos y exportes.
- **Precondiciones**: Legajos cargados.
- **Flujo principal**:
  1. Ejecuta procesar/exportar.
  2. Se generan recibos y exportes.
- **Postcondicion**: Liquidación exportada.

## CU-RH-09 Descargar exportes
- **Actor**: RRHH Admin
- **Objetivo**: Descargar JSON/CSV de exportes.
- **Precondiciones**: Liquidación exportada. Rol Admin.
- **Flujo principal**:
  1. Selecciona liquidación.
  2. El sistema consulta exportes disponibles.
  3. Descarga archivos.
- **Postcondicion**: Exportes descargados.

## CU-RH-10 Ver detalle de liquidación
- **Actor**: RRHH Admin
- **Objetivo**: Ver recibos y exportes.
- **Precondiciones**: Liquidación existente.
- **Flujo principal**:
  1. Selecciona liquidación.
  2. Sistema muestra recibos y links de exportes.
- **Postcondicion**: Detalle visible.

## CU-RH-11 Gestionar vacaciones
- **Actor**: RRHH Admin
- **Objetivo**: Aprobar o rechazar solicitudes.
- **Precondiciones**: Instancias de vacaciones disponibles.
- **Flujo principal**:
  1. Visualiza solicitudes.
  2. Ejecuta aprobar/rechazar.
- **Postcondicion**: Instancia actualizada.

## CU-RH-12 Iniciar exporte de horas
- **Actor**: RRHH Admin
- **Objetivo**: Disparar job de exporte.
- **Precondiciones**: Template de integración disponible.
- **Flujo principal**:
  1. Selecciona template y periodo.
  2. Inicia job.
- **Postcondicion**: Job registrado.

## CU-RH-13 Ver detalle de job
- **Actor**: RRHH Admin
- **Objetivo**: Ver estado y archivo generado.
- **Precondiciones**: Job existente.
- **Flujo principal**:
  1. Selecciona job.
  2. El sistema muestra detalle.
- **Postcondicion**: Detalle visible.

## CU-RH-14 Reintentar job
- **Actor**: RRHH Admin
- **Objetivo**: Reintentar integración fallida.
- **Precondiciones**: Job fallido.
- **Flujo principal**:
  1. Ejecuta reintento.
  2. El sistema registra el reintento.
- **Postcondicion**: Job reintentado.

## CU-RH-15 Operar via BFF
- **Actor**: RRHH Admin
- **Objetivo**: Consumir APIs centralizadas.
- **Precondiciones**: Portal BFF configurado.
- **Flujo principal**:
  1. El portal usa `/api/rh/v1/*`.
  2. Se proxyan operaciones RH.
- **Postcondicion**: Operaciones RH centralizadas.

## CU-RH-16 Ver eventos de integración
- **Actor**: RRHH Admin
- **Objetivo**: Auditar ejecuciones.
- **Precondiciones**: Jobs existentes.
- **Flujo principal**:
  1. Filtra por job.
  2. El portal consulta eventos.
- **Postcondicion**: Eventos visibles.

## CU-RH-17 Ejecutar trigger
- **Actor**: RRHH Admin
- **Objetivo**: Disparar integración por evento.
- **Precondiciones**: Trigger configurado.
- **Flujo principal**:
  1. Ejecuta trigger.
  2. Se crea job.
- **Postcondicion**: Job creado.

## CU-RH-18 Crear trigger
- **Actor**: RRHH Admin
- **Objetivo**: Configurar eventos.
- **Precondiciones**: Template disponible.
- **Flujo principal**:
  1. Completa eventName y template.
  2. El sistema crea trigger.
- **Postcondicion**: Trigger disponible.

## CU-RH-19 Editar trigger
- **Actor**: RRHH Admin
- **Objetivo**: Ajustar evento o template.
- **Precondiciones**: Trigger existente.
- **Flujo principal**:
  1. Selecciona trigger.
  2. Actualiza datos.
- **Postcondicion**: Trigger actualizado.

## CU-RH-20 Crear sindicato
- **Actor**: RRHH Admin
- **Objetivo**: Registrar un sindicato.
- **Precondiciones**: JWT Admin.
- **Flujo principal**:
  1. Completa datos del sindicato.
  2. El sistema crea el sindicato.
- **Postcondicion**: Sindicato visible.

## CU-RH-21 Editar sindicato
- **Actor**: RRHH Admin
- **Objetivo**: Actualizar sindicato.
- **Precondiciones**: Sindicato existente.
- **Flujo principal**:
  1. Selecciona sindicato.
  2. Actualiza datos.
  3. El sistema guarda.
- **Postcondicion**: Sindicato actualizado.

## CU-RH-22 Crear convenio
- **Actor**: RRHH Admin
- **Objetivo**: Registrar convenio colectivo.
- **Precondiciones**: Sindicato existente.
- **Flujo principal**:
  1. Selecciona sindicato.
  2. Completa datos del convenio.
  3. El sistema crea el convenio.
- **Postcondicion**: Convenio visible.

## CU-RH-23 Editar convenio
- **Actor**: RRHH Admin
- **Objetivo**: Actualizar datos del convenio.
- **Precondiciones**: Convenio existente.
- **Flujo principal**:
  1. Selecciona convenio.
  2. Ajusta datos y sindicato.
  3. El sistema guarda.
- **Postcondicion**: Convenio actualizado.

## CU-RH-24 Versionar organigrama via BFF
- **Actor**: RRHH Admin
- **Objetivo**: Crear y consultar versiones del organigrama desde Portal RH.
- **Precondiciones**: Portal BFF configurado y unidades cargadas.
- **Flujo principal**:
  1. Configura el BFF en Portal RH (usa `/api/rh/v1/*`).
  2. Crea version con `POST /api/rh/v1/organizacion/organigramas`.
  3. Lista versiones con `GET /api/rh/v1/organizacion/organigramas`.
- **Postcondicion**: Version creada y visible en el listado.

# Casos de uso · Tiempos Trabajados

## CU-TT-01 Gestionar turnos
- **Actor**: RRHH Admin
- **Objetivo**: Crear/editar turnos.
- **Precondiciones**: JWT Admin.
- **Flujo principal**:
  1. El actor ejecuta `POST /turnos`.
  2. El actor ejecuta `PUT /turnos/{id}`.
- **Postcondicion**: Turno disponible.

## CU-TT-02 Gestionar horarios
- **Actor**: RRHH Admin
- **Objetivo**: Crear/editar horarios semanales.
- **Precondiciones**: Turnos existentes.
- **Flujo principal**:
  1. El actor ejecuta `POST /horarios`.
  2. El actor ejecuta `PUT /horarios/{id}`.
- **Postcondicion**: Horario disponible.

## CU-TT-03 Registrar fichadas
- **Actor**: Empleado / Integracion
- **Objetivo**: Registrar entrada/salida.
- **Precondiciones**: Legajo existente.
- **Flujo principal**:
  1. El actor ejecuta `POST /fichadas`.
- **Postcondicion**: Fichada registrada.

## CU-TT-04 Consultar fichadas
- **Actor**: RRHH Admin
- **Objetivo**: Ver fichadas por legajo y periodo.
- **Precondiciones**: Fichadas existentes.
- **Flujo principal**:
  1. El actor ejecuta `GET /fichadas?legajoId=&desde=&hasta=`.
- **Postcondicion**: Fichadas visibles.

## CU-TT-05 Generar planilla
- **Actor**: RRHH Admin
- **Objetivo**: Consolidar horas por periodo.
- **Precondiciones**: Fichadas cargadas.
- **Flujo principal**:
  1. El actor ejecuta `POST /planillas`.
- **Postcondicion**: Planilla creada.

## CU-TT-06 Cerrar planilla
- **Actor**: RRHH Admin
- **Objetivo**: Cerrar planilla para exporte.
- **Precondiciones**: Planilla creada.
- **Flujo principal**:
  1. El actor ejecuta `POST /planillas/{id}/cerrar`.
- **Postcondicion**: Planilla cerrada.

## CU-TT-07 Exportar horas a Liquidacion
- **Actor**: RRHH Admin
- **Objetivo**: Enviar planilla a Liquidacion.
- **Precondiciones**: Planilla cerrada.
- **Flujo principal**:
  1. El actor ejecuta `POST /integraciones/jobs` con template "exporte-horas-liquidacion".
  2. El sistema crea un job de integracion.
  3. El sistema procesa la planilla y genera archivo para liquidacion.
  4. El sistema Notifica a liquidacion-service via evento.
- **Postcondicion**: Job de exporte creado y ejecutandose.
- **Endpoints relacionados**:
  - `POST /api/rh/v1/integraciones/jobs` - Crear job
  - `GET /api/rh/v1/integraciones/jobs/{id}` - Ver estado
  - `POST /api/rh/v1/integraciones/jobs/{id}/retry` - Reintentar

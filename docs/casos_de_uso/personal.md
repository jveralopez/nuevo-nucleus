# Casos de uso · Personal

## CU-PER-01 Consultar legajos
- **Actor**: Usuario autenticado
- **Objetivo**: Ver legajos.
- **Precondiciones**: Token JWT valido.
- **Flujo principal**:
  1. El actor consulta `GET /legajos` con filtros opcionales (estado, numero, documento, cuil).
  2. El sistema devuelve la lista.
- **Postcondicion**: Datos visibles.

## CU-PER-01B Consultar legajo por numero
- **Actor**: Usuario autenticado
- **Objetivo**: Ver un legajo puntual.
- **Precondiciones**: Token JWT valido.
- **Flujo principal**:
  1. El actor consulta `GET /legajos/numero/{numero}`.
  2. El sistema devuelve el legajo.
- **Postcondicion**: Legajo visible.

## CU-PER-02 Crear legajo
- **Actor**: Admin
- **Objetivo**: Registrar un legajo.
- **Precondiciones**: Rol Admin.
- **Flujo principal**:
  1. El actor envia datos obligatorios.
  2. El sistema valida y crea.
- **Postcondicion**: Legajo creado.

## CU-PER-03 Editar legajo
- **Actor**: Admin
- **Objetivo**: Actualizar datos del legajo.
- **Precondiciones**: Legajo existente.
- **Flujo principal**:
  1. El actor selecciona el legajo.
  2. Actualiza campos.
  3. El sistema guarda.
- **Postcondicion**: Legajo actualizado.

## CU-PER-04 Gestionar familiares
- **Actor**: Admin
- **Objetivo**: Registrar familiares a cargo.
- **Precondiciones**: Legajo existente.
- **Flujo principal**:
  1. El actor agrega familiar (tipo, nombre, documento).
  2. El sistema guarda.
- **Postcondicion**: Familiar asociado al legajo.

## CU-PER-05 Gestionar licencias
- **Actor**: Admin
- **Objetivo**: Registrar licencias con o sin goce.
- **Precondiciones**: Legajo existente.
- **Flujo principal**:
  1. El actor agrega licencia (tipo, dias, conGoce).
  2. El sistema guarda.
- **Postcondicion**: Licencia asociada al legajo.

## CU-PER-06 Gestionar domicilios
- **Actor**: Admin
- **Objetivo**: Registrar domicilios del legajo.
- **Precondiciones**: Legajo existente.
- **Flujo principal**:
  1. El actor registra domicilios (tipo, calle, localidad).
  2. El sistema guarda.
- **Postcondicion**: Domicilios asociados al legajo.

## CU-PER-07 Gestionar documentos
- **Actor**: Admin
- **Objetivo**: Registrar documentos personales.
- **Precondiciones**: Legajo existente.
- **Flujo principal**:
  1. El actor registra documentos (tipo, numero, vigencia).
  2. El sistema guarda.
- **Postcondicion**: Documentos asociados al legajo.

## CU-PER-08 Solicitar cambio de datos
- **Actor**: Empleado
- **Objetivo**: Registrar solicitud de cambio.
- **Precondiciones**: Legajo existente.
- **Flujo principal**:
  1. El actor crea la solicitud via `POST /solicitudes`.
  2. El sistema queda en estado `PendAprob`.
- **Postcondicion**: Solicitud creada.

## CU-PER-09 Aprobar/Rechazar solicitud
- **Actor**: Admin
- **Objetivo**: Resolver la solicitud.
- **Precondiciones**: Solicitud existente.
- **Flujo principal**:
  1. El admin aprueba o rechaza.
  2. El sistema actualiza el estado.
- **Postcondicion**: Solicitud resuelta.

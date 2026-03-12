# Casos de prueba · Personal

## CT-PER-01 Listar legajos
- **Relacion**: CU-PER-01
- **Precondiciones**: JWT valido.
- **Pasos**:
- **Pasos**:
  1. Ejecutar `GET /legajos`.
  2. Repetir con filtros `?estado=Activo` y `?numero=...`.
- **Resultado esperado**: HTTP 200 con lista.

## CT-PER-01B Consultar legajo por numero
- **Relacion**: CU-PER-01B
- **Precondiciones**: Legajo existente.
- **Pasos**:
  1. Ejecutar `GET /legajos/numero/{numero}`.
- **Resultado esperado**: HTTP 200 con legajo.

## CT-PER-02 Crear legajo
- **Relacion**: CU-PER-02
- **Precondiciones**: JWT Admin.
- **Pasos**:
  1. Ejecutar `POST /legajos` con datos obligatorios.
- **Resultado esperado**: Legajo creado.

## CT-PER-03 Editar legajo
- **Relacion**: CU-PER-03
- **Precondiciones**: Legajo existente.
- **Pasos**:
  1. Ejecutar `PUT /legajos/{id}` con cambios.
- **Resultado esperado**: Legajo actualizado.

## CT-PER-04 Agregar familiar
- **Relacion**: CU-PER-04
- **Precondiciones**: Legajo existente.
- **Pasos**:
- **Pasos**:
  1. Ejecutar `PUT /legajos/{id}/familiares`.
- **Resultado esperado**: Familiar asociado.

## CT-PER-05 Agregar licencia
- **Relacion**: CU-PER-05
- **Precondiciones**: Legajo existente.
- **Pasos**:
- **Pasos**:
  1. Ejecutar `PUT /legajos/{id}/licencias`.
- **Resultado esperado**: Licencia asociada.

## CT-PER-06 Agregar domicilios
- **Relacion**: CU-PER-06
- **Precondiciones**: Legajo existente.
- **Pasos**:
  1. Ejecutar `PUT /legajos/{id}/domicilios`.
- **Resultado esperado**: Domicilios asociados.

## CT-PER-07 Agregar documentos
- **Relacion**: CU-PER-07
- **Precondiciones**: Legajo existente.
- **Pasos**:
  1. Ejecutar `PUT /legajos/{id}/documentos`.
- **Resultado esperado**: Documentos asociados.

## CT-PER-08 Crear solicitud
- **Relacion**: CU-PER-08
- **Precondiciones**: Legajo existente.
- **Pasos**:
  1. Ejecutar `POST /solicitudes`.
- **Resultado esperado**: Solicitud creada.

## CT-PER-09 Aprobar/Rechazar solicitud
- **Relacion**: CU-PER-09
- **Precondiciones**: Solicitud existente.
- **Pasos**:
  1. Ejecutar `POST /solicitudes/{id}/aprobar`.
  2. Ejecutar `POST /solicitudes/{id}/rechazar`.
- **Resultado esperado**: Estados actualizados.

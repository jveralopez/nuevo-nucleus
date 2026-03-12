# Casos de prueba · Organización

## CT-ORG-01 Listar empresas
- **Relacion**: CU-ORG-01
- **Precondiciones**: JWT valido.
- **Pasos**:
  1. Ejecutar `GET /empresas`.
- **Resultado esperado**: HTTP 200 con lista.

## CT-ORG-02 Crear empresa
- **Relacion**: CU-ORG-02
- **Precondiciones**: JWT Admin.
- **Pasos**:
  1. Ejecutar `POST /empresas`.
- **Resultado esperado**: Empresa creada.

## CT-ORG-03 Gestionar unidades
- **Relacion**: CU-ORG-03
- **Precondiciones**: Empresa existente.
- **Pasos**:
  1. Ejecutar `POST /unidades`.
  2. Ejecutar `PUT /unidades/{id}`.
- **Resultado esperado**: Unidad creada y actualizada.

## CT-ORG-03B Ver organigrama
- **Relacion**: CU-ORG-03B
- **Precondiciones**: Unidades cargadas.
- **Pasos**:
  1. Ejecutar `GET /unidades/tree`.
- **Resultado esperado**: Arbol de unidades.

## CT-ORG-04 Gestionar posiciones
- **Relacion**: CU-ORG-04
- **Precondiciones**: Unidad existente.
- **Pasos**:
  1. Ejecutar `POST /posiciones`.
  2. Ejecutar `PUT /posiciones/{id}`.
- **Resultado esperado**: Posicion creada y actualizada.

## CT-ORG-05 Asignar/desasignar legajo
- **Relacion**: CU-ORG-05
- **Precondiciones**: Legajo y posicion existentes.
- **Pasos**:
  1. Ejecutar `POST /posiciones/{id}/asignar`.
  2. Ejecutar `POST /posiciones/{id}/desasignar`.
- **Resultado esperado**: Posicion actualizada.

## CT-ORG-06 Gestionar centros de costo
- **Relacion**: CU-ORG-06
- **Precondiciones**: Empresa existente.
- **Pasos**:
  1. Ejecutar `POST /centros-costo`.
  2. Ejecutar `PUT /centros-costo/{id}`.
- **Resultado esperado**: Centro creado y actualizado.

## CT-ORG-07 Gestionar sindicatos
- **Relacion**: CU-ORG-04
- **Precondiciones**: JWT Admin.
- **Pasos**:
  1. Ejecutar `POST /sindicatos`.
  2. Ejecutar `PUT /sindicatos/{id}`.
- **Resultado esperado**: Sindicato creado y actualizado.

## CT-ORG-08 Gestionar convenios
- **Relacion**: CU-ORG-05
- **Precondiciones**: Sindicato existente.
- **Pasos**:
  1. Ejecutar `POST /convenios`.
  2. Ejecutar `PUT /convenios/{id}`.
- **Resultado esperado**: Convenio creado y actualizado.

## CT-ORG-09 Versionar organigrama
- **Relacion**: CU-ORG-09
- **Precondiciones**: Empresa existente y unidades cargadas.
- **Pasos**:
  1. Ejecutar `POST /organigramas` con `EmpresaId` y `Nombre`.
- **Resultado esperado**: HTTP 201 con la version creada.

## CT-ORG-10 Consultar versiones de organigrama
- **Relacion**: CU-ORG-10
- **Precondiciones**: Versiones existentes.
- **Pasos**:
  1. Ejecutar `GET /organigramas?empresaId={id}`.
  2. Ejecutar `GET /organigramas/{id}`.
- **Resultado esperado**: HTTP 200 con listado y detalle.

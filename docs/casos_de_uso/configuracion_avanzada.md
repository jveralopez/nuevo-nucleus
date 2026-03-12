# Casos de uso · Configuracion Avanzada

## CU-CONF-01 Crear catalogo
- **Actor**: Admin
- **Objetivo**: Registrar item de catalogo.
- **Precondiciones**: JWT Admin.
- **Flujo principal**:
  1. Envia POST `/catalogos`.
  2. El sistema guarda el item.
- **Postcondicion**: Item disponible.

## CU-CONF-02 Listar catalogo por tipo
- **Actor**: Admin
- **Objetivo**: Ver items de un tipo.
- **Precondiciones**: JWT Admin.
- **Flujo principal**:
  1. Llama GET `/catalogos/{tipo}`.
  2. El sistema devuelve lista.
- **Postcondicion**: Lista visible.

## CU-CONF-03 Editar catalogo
- **Actor**: Admin
- **Objetivo**: Actualizar item.
- **Precondiciones**: Item existente.
- **Flujo principal**:
  1. Envia PUT `/catalogos/{id}`.
  2. El sistema actualiza.
- **Postcondicion**: Item actualizado.

## CU-CONF-04 Eliminar catalogo
- **Actor**: Admin
- **Objetivo**: Eliminar item.
- **Precondiciones**: Item existente.
- **Flujo principal**:
  1. Envia DELETE `/catalogos/{id}`.
  2. El sistema elimina.
- **Postcondicion**: Item eliminado.

## CU-CONF-05 Upsert parametro
- **Actor**: Admin
- **Objetivo**: Crear o actualizar parametro.
- **Precondiciones**: JWT Admin.
- **Flujo principal**:
  1. Envia POST `/parametros`.
  2. El sistema guarda.
- **Postcondicion**: Parametro disponible.

## CU-CONF-06 Consultar parametro
- **Actor**: Admin
- **Objetivo**: Consultar parametro por clave.
- **Precondiciones**: JWT Admin.
- **Flujo principal**:
  1. Llama GET `/parametros/{clave}`.
  2. El sistema devuelve valor.
- **Postcondicion**: Parametro visible.

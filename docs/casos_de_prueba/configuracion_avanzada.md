# Casos de prueba · Configuracion Avanzada

## CP-CONF-01 Crear catalogo
- POST `/catalogos` con tipo, codigo y nombre.
- Esperado: 201 y item creado.

## CP-CONF-02 Listar catalogo
- GET `/catalogos/{tipo}`.
- Esperado: 200 y lista con items.

## CP-CONF-03 Editar catalogo
- PUT `/catalogos/{id}` con cambios.
- Esperado: 200 y item actualizado.

## CP-CONF-04 Eliminar catalogo
- DELETE `/catalogos/{id}`.
- Esperado: 204.

## CP-CONF-05 Upsert parametro
- POST `/parametros` con clave y valor.
- Esperado: 200 y parametro actualizado/creado.

## CP-CONF-06 Consultar parametro
- GET `/parametros/{clave}`.
- Esperado: 200 con valor.

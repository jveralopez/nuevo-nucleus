# Casos de prueba · Workflow (NucleusWF)

## CT-WF-01 Cargar definiciones demo
- **Relacion**: CU-PORT-07
- **Precondiciones**: JWT Admin.
- **Pasos**:
  1. Ejecutar `POST /definitions` con definiciones base.
- **Resultado esperado**: Definiciones creadas.

## CT-WF-02 Crear instancia
- **Relacion**: CU-PORT-05, CU-PORT-08, CU-PORT-09
- **Precondiciones**: Definicion disponible.
- **Pasos**:
  1. Ejecutar `POST /instances` con key/version/datos.
- **Resultado esperado**: Instancia creada.

## CT-WF-03 Transicionar instancia
- **Relacion**: CU-PORT-04, CU-PORT-06
- **Precondiciones**: Instancia existente.
- **Pasos**:
  1. Ejecutar `POST /instances/{id}/transitions`.
- **Resultado esperado**: Estado actualizado.

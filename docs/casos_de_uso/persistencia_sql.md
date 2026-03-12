# Casos de uso · Persistencia SQL

## CU-SQL-01 Guardar entidades
- **Actor**: Sistema
- **Objetivo**: Persistir datos en SQLite.
- **Precondiciones**: ConnectionStrings configuradas.
- **Flujo principal**:
  1. Servicio guarda entidad.
  2. DB almacena cambios.
- **Postcondicion**: Datos persistidos.

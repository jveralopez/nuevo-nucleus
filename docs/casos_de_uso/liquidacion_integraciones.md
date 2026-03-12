# Casos de uso · Liquidacion -> Integration Hub

## CU-LIQ-INT-01 Disparar job post-liquidacion
- **Actor**: Sistema
- **Objetivo**: Ejecutar template de integracion al exportar liquidacion.
- **Precondiciones**: Integration Hub configurado y template publicado.
- **Flujo principal**:
  1. Operador procesa liquidacion con exportar.
  2. El sistema genera archivos de recibos.
  3. El sistema dispara job en Integration Hub.
- **Postcondicion**: Job creado y trazado.

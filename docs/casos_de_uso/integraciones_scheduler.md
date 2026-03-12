# Casos de uso · Scheduler Integration Hub

## CU-INT-SCH-01 Ejecutar por cron
- **Actor**: Scheduler
- **Objetivo**: Disparar jobs segun cron.
- **Precondiciones**: Template publicado y cron valido.
- **Flujo principal**:
  1. Scheduler evalua cron.
  2. Dispara job.
  3. Actualiza `LastRunAt`.
- **Postcondicion**: Job creado.

## CU-INT-SCH-02 Reintentar job fallido
- **Actor**: Scheduler
- **Objetivo**: Reintentar jobs fallidos.
- **Precondiciones**: Job en estado Fallido.
- **Flujo principal**:
  1. Scheduler detecta job fallido.
  2. Ejecuta retry.
- **Postcondicion**: Job actualizado.

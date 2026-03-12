# Casos de prueba · Scheduler Integration Hub

## CP-INT-SCH-01 Cron ejecuta job
- **Dado** template publicado con cron valido
- **Cuando** llega el horario
- **Entonces** se crea job

## CP-INT-SCH-02 Reintento automatico
- **Dado** job fallido
- **Cuando** scheduler corre
- **Entonces** se incrementa RetryCount

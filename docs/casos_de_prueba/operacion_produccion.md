# Casos de prueba · Operacion y produccion

## CP-OP-01 · CI build/test
**Objetivo:** validar workflow CI.
**Pasos:**
1. Ejecutar workflow `ci`.
2. Verificar etapas Restore/Build/Test.
**Resultado esperado:** workflow en verde.

## CP-OP-02 · Release con tag
**Objetivo:** generar imagenes release.
**Pasos:**
1. Crear tag `vX.Y.Z`.
2. Verificar workflow `release`.
**Resultado esperado:** imagenes publicadas en GHCR.

## CP-OP-02b · Deploy workflow
**Objetivo:** validar despliegue automatizado.
**Pasos:**
1. Ejecutar workflow `deploy`.
2. Verificar que los contenedores se actualizan en el host.
**Resultado esperado:** servicios en `healthy` post-deploy.

## CP-OP-03 · Health checks
**Objetivo:** validar liveness/readiness.
**Pasos:**
1. Llamar `/healthz` y `/readyz`.
2. Confirmar status HTTP 200.
**Resultado esperado:** ambos endpoints responden ok.

## CP-OP-04 · OTEL activo
**Objetivo:** validar emision de trazas/metricas.
**Pasos:**
1. Setear `OTEL_ENABLED=true` y `OTEL_EXPORTER_OTLP_ENDPOINT`.
2. Generar trafico.
**Resultado esperado:** trazas/metricas visibles en backend.

## CP-OP-05 · Backup/Restore
**Objetivo:** validar recuperacion de datos.
**Pasos:**
1. Backup de `.db` y `/data/keys`.
2. Restaurar y reiniciar.
**Resultado esperado:** datos consistentes y autenticacion estable.

## CP-OP-06 · Runbooks
**Objetivo:** validar runbooks de incidentes.
**Pasos:**
1. Simular fallo en un servicio (parar contenedor).
2. Aplicar `RB-01` y reiniciar.
**Resultado esperado:** servicio vuelve a `healthy`.

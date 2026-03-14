# Runbooks operativos

## RB-01 · Servicio no responde
**Sintoma:** `/healthz` o `/readyz` no responde.
**Acciones:**
1. Revisar logs del contenedor.
2. Verificar conexion a SQLite y permisos de volumen.
3. Reiniciar el servicio con `docker compose restart <servicio>`.

## RB-02 · Errores 401/403
**Sintoma:** usuarios no autenticados o permisos insuficientes.
**Acciones:**
1. Confirmar `AUTH_ISSUER/AUDIENCE/SIGNING_KEY`.
2. Validar token y rol.
3. Verificar que no se haya rotado el signing key sin redeploy.

## RB-03 · DB bloqueada
**Sintoma:** errores SQLite "database is locked".
**Acciones:**
1. Verificar procesos concurrentes en el volumen.
2. Reducir concurrencia y reiniciar servicio.
3. Si persiste, evaluar migrar a DB server.

## RB-04 · Observabilidad sin datos
**Sintoma:** Jaeger/Prometheus sin trazas/metricas.
**Acciones:**
1. Verificar `OTEL_ENABLED=true` y `OTEL_EXPORTER_OTLP_ENDPOINT`.
2. Revisar logs del `otel-collector`.
3. Confirmar puertos 4317/8889 accesibles.

## RB-05 · Rotacion de secretos
**Sintoma:** despliegue con llaves nuevas.
**Acciones:**
1. Cambiar `AUTH_SIGNING_KEY` en `.env.prod`.
2. Redeploy de todos los servicios.
3. Validar login y APIs protegidas.

## RB-06 · Smoke test post-deploy
**Sintoma:** validacion rapida tras despliegue.
**Acciones:**
1. Verificar `docker compose ps` y estados `healthy`.
2. Probar `/healthz` y `/readyz` de servicios core.
3. Probar login y una llamada protegida con JWT.
4. Probar flujo basico de Portal RH (listar empresas y organigrama).
5. Confirmar logs sin errores criticos.

## RB-07 · E2E Playwright
**Sintoma:** validar portal RH/Empleado con flujo base.
**Acciones:**
1. Levantar docker compose prod.
2. Servir UIs en `http://localhost:3001` y `http://localhost:3002`.
3. Ejecutar `npm run test:e2e`.
4. Alternativa rápida: ejecutar `start-e2e.ps1` en el root.

## RB-08 · Idempotencia y correlación (WF)
**Sintoma:** transiciones duplicadas o auditoría incompleta.
**Acciones:**
1. Enviar `Idempotency-Key` único por operación (start/transition).
2. Enviar `X-Correlation-Id` por request para trazabilidad.
3. Verificar que el historial de la instancia guarda actor, rol y correlation.
4. El Portal BFF ya inyecta ambos headers si no vienen en la solicitud.
5. Servicios core también generan headers si están ausentes.
6. Campos sensibles (diagnóstico/notas/detalle) se redactan en el resumen de auditoría.

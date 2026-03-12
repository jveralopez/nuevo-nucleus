# Observabilidad

## Stack local de observabilidad
- Collector OTEL: `otel-collector` (recepcion OTLP gRPC/HTTP)
- Jaeger UI: http://localhost:16686
- Prometheus: http://localhost:9090
 - Jaeger OTLP habilitado con `COLLECTOR_OTLP_ENABLED=true`

## Activacion
1. Configurar `.env.prod`:
   - `OTEL_ENABLED=true`
   - `OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4318`
   - `OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf`
   - `OTEL_TRACES_SAMPLER=always_on`
2. Levantar el stack: `docker compose -f docker-compose.prod.yml --env-file .env.prod up -d`.

## Validacion
- Verificar trazas en Jaeger con el servicio seleccionado.
- Verificar metricas en Prometheus (`otelcol_*`).

## Troubleshooting
- Si no hay trazas: confirmar `OTEL_ENABLED=true` y endpoint correcto.
- Si no hay metricas: revisar `otel-collector` y el puerto 8889.

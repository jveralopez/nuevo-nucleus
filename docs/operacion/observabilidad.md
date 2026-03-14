# Observabilidad

## Stack local de observabilidad
- Collector OTEL: `otel-collector` (recepcion OTLP gRPC/HTTP)
- Jaeger UI: http://localhost:16686
- Prometheus: http://localhost:9090
  - Jaeger OTLP habilitado con `COLLECTOR_OTLP_ENABLED=true`

## Activacion
1. El archivo `.env` ya incluye la configuracion de OpenTelemetry:
   - `OTEL_ENABLED=true`
   - `OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317`
   - `OTEL_EXPORTER_OTLP_PROTOCOL=grpc`
   - `OTEL_TRACES_SAMPLER=always_on`
2. Levantar el stack: `docker compose -f docker-compose.prod.yml up -d`.

## Verificar trazas en Jaeger

### Paso 1: Acceder a Jaeger
- Abrir http://localhost:16686 en el navegador

### Paso 2: Buscar servicios
- En el dropdown "Service" deberian aparecer todos los servicios:
  - auth-service
  - organizacion-service
  - personal-service
  - liquidacion-service
  - tiempos-service
  - nucleuswf-service
  - integration-hub-service
  - portal-bff-service
  - configuracion-service

### Paso 3: Si no aparecen servicios
- Verificar que los servicios esten corriendo: `docker compose ps`
- Verificar logs del collector: `docker compose logs otel-collector`
- Verificar que los servicios envian trazas:
  ```bash
  curl http://localhost:16686/api/services
  ```
- Esperar 1-2 minutos para que Jaeger procese las trazas

### Paso 4: Ver trazas detalladas
- Seleccionar un servicio
- Click en "Find Traces"
- Explorar las trazas en el panel derecho

## Validacion
- Verificar trazas en Jaeger con el servicio seleccionado.
- Verificar metricas en Prometheus (`otelcol_*`).

## Troubleshooting

### Problema: Solo aparece "jaeger-all-in-one"
- **Causa**: Jaeger no ha recibido trazas de los servicios aun
- **Solucion**:
  1. Asegurarse que `OTEL_ENABLED=true` en los servicios
  2. Hacer requests a los endpoints de los servicios para generar trazas
  3. Esperar hasta 2 minutos
  4. Refrescar la pagina de Jaeger

### Problema: No hay trazas
- **Causa**: Los servicios no pueden conectar al collector
- **Solucion**:
  1. Verificar que `otel-collector` este corriendo
  2. Verificar el endpoint: debe ser `http://otel-collector:4317` (gRPC)
  3. Revisar logs: `docker compose logs otel-collector`

### Problema: No hay metricas
- Revisar que el collector tenga el exporter de Prometheus configurado
- Verificar puerto 8889 en Prometheus

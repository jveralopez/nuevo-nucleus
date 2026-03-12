# Casos de uso · Operacion y produccion

## CU-OP-01 · Despliegue con docker-compose.prod
**Objetivo:** levantar el stack en modo produccion.
**Actores:** operador.
**Precondiciones:** `.env.prod` configurado, volumenes disponibles.
**Flujo:**
1. Ejecutar `docker compose -f docker-compose.prod.yml --env-file .env.prod up -d`.
2. Verificar `/healthz` en todos los servicios.
3. Si `DB_APPLY_MIGRATIONS=true`, confirmar migraciones aplicadas en el primer arranque.
**Resultado esperado:** servicios en `healthy` y endpoints activos.

## CU-OP-02 · Publicacion de imagenes
**Objetivo:** publicar imagenes en GHCR.
**Actores:** equipo de release.
**Precondiciones:** tag `v*` o workflow_dispatch.
**Flujo:**
1. Crear tag `vX.Y.Z` y pushear.
2. Workflow `release` construye y publica imagenes.
**Resultado esperado:** imagenes con tags `vX.Y.Z` y `latest`.

## CU-OP-02b · Deploy automatizado
**Objetivo:** ejecutar despliegue via workflow `deploy`.
**Actores:** equipo de release.
**Precondiciones:** secrets `DEPLOY_HOST`, `DEPLOY_USER`, `DEPLOY_SSH_KEY`, `DEPLOY_PORT`, `DEPLOY_PATH`.
**Flujo:**
1. Ejecutar workflow `deploy`.
2. El servidor ejecuta `docker compose pull` y `up -d`.
**Resultado esperado:** servicios actualizados en el servidor.

## CU-OP-03 · Observabilidad con OTLP
**Objetivo:** emitir trazas/metricas a colector OTLP.
**Actores:** operador.
**Precondiciones:** `OTEL_EXPORTER_OTLP_ENDPOINT` disponible.
**Flujo:**
1. Setear `OTEL_ENABLED=true`.
2. Confirmar que los endpoints procesan trafico.
**Resultado esperado:** trazas y metricas visibles en el backend de observabilidad.

## CU-OP-05 · Runbooks operativos
**Objetivo:** guiar la resolucion de incidentes.
**Actores:** operador.
**Precondiciones:** acceso a logs y dashboards.
**Flujo:**
1. Identificar el sintoma (health/401/db locked/otel).
2. Seguir el runbook correspondiente.
**Resultado esperado:** servicio estabilizado.

## CU-OP-04 · Backup y restore de SQLite
**Objetivo:** respaldar y restaurar datos.
**Actores:** operador.
**Precondiciones:** ventana sin escrituras o servicios detenidos.
**Flujo:**
1. Copiar archivos `.db` de los volumenes.
2. Respaldar `/data/keys`.
3. Restaurar archivos y reiniciar servicios con `DB_APPLY_MIGRATIONS=true`.
**Resultado esperado:** datos y credenciales recuperados.

## CU-OP-06 · Baseline de migraciones en SQLite existente
**Objetivo:** alinear `__EFMigrationsHistory` cuando la base ya tiene tablas creadas.
**Actores:** operador.
**Precondiciones:** base SQLite existente y acceso a un cliente SQLite.
**Flujo:**
1. Verificar que las tablas existen y que `__EFMigrationsHistory` no contiene migraciones.
2. Crear `__EFMigrationsHistory` si no existe.
3. Insertar las migraciones ya aplicadas manualmente.
4. Ejecutar `dotnet ef database update` para validar que no hay migraciones pendientes.
**Resultado esperado:** migraciones alineadas sin recrear tablas.

Ejemplo de baseline:

```sql
CREATE TABLE IF NOT EXISTS __EFMigrationsHistory (
  MigrationId TEXT NOT NULL CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY,
  ProductVersion TEXT NOT NULL
);

INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
SELECT '20260312113613_InitialCreate', '8.0.6'
WHERE NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20260312113613_InitialCreate');

INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
SELECT '20260312154000_AddOrganigramas', '8.0.6'
WHERE NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20260312154000_AddOrganigramas');
```

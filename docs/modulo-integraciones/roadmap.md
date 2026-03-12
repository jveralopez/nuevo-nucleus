# Roadmap · Integraciones

## Fase 0 · Descubrimiento (completo)
- [x] Analizar `InterfacesOut` (generador generico, templates) e interfaces de entrada.
- [x] Definir arquitectura moderna (Integration Hub + jobs + eventos).

## Fase 1 · Foundation
- [ ] Diseñar modelo de templates/conexiones y repositorio.
- [ ] Implementar `integration-api` (CRUD templates, conexiones, jobs) con almacenamiento en archivo/DB.
- [ ] Crear motor de ejecución simple (lectura SQL + render Liquid + escribir archivo local).

## Fase 2 · Conectores y scheduler
- [ ] Agregar scheduler (cron) y triggers event-driven.
- [ ] Construir conectores SFTP, e-mail, Blob Storage, HTTP.
- [ ] Implementar reporte básico de ejecuciones + alertas.

## Fase 3 · Integraciones críticas
- [ ] Migrar interfaces bancarias, legales, sindicatos, LSD, SICOSS, AFIP.
- [ ] Migrar interfaces de entrada (ArchivoHoras, licencias, novedades) a pipelines event-driven.
- [ ] Integrar con Data Lake / BI.

## Fase 4 · Hardening
- [ ] Seguridad (secret vault, RBAC), auditoría, versionado.
- [ ] Portal de monitoreo y observabilidad avanzados.
- [ ] Package de SDK/conectores reutilizables.

---
*Última actualización: 2026-03-09*

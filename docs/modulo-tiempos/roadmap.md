# Roadmap · Tiempos Trabajados

## Fase 0 · Descubrimiento (completa)
- [x] Relevar menú TTA, interfaces y scripts SQL.
- [x] Definir dominios, arquitectura y API.

## Fase 1 · Ingesta y horarios (PoC)
- [ ] Implementar `tta-ingest-service` con almacenamiento en archivo + colas.
- [ ] Desarrollar API de horarios y asignaciones básicas.
- [ ] UI inicial para ver fichadas y turnos.

## Fase 2 · Procesamiento de horas
- [ ] Motor de cálculo (regrasar horas normales/extras) + repositorio.
- [ ] Integración con Legajos/Liquidación (perfil sync + exporte JSON/CSV).
- [ ] Reporte básico (parte diario, horas por legajo).

## Fase 3 · Francos/compensatorios y planillas
- [ ] Banco de horas y aprobaciones.
- [ ] Planillas masivas (cambios de turno, licencias, novedades) con validaciones.
- [ ] APIs para licencias/novedades según rol.

## Fase 4 · Endurecimiento e integraciones
- [ ] Integrar con Kubo/relojes vía APIs reales.
- [ ] Observabilidad y tracing end-to-end.
- [ ] Hardening seguridad (roles, auditoría) y pipelines CI/CD.

---
*Última actualización: 2026-03-09*

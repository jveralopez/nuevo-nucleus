# Roadmap · Vacaciones / Autoservicio

## Fase 0 · Descubrimiento (completa)
- [x] Analizar workflow y clases actuales (`Solicitud.WF.xml`, `lib_v11.WFSolicitud.SOLICITUD`).
- [x] Definir arquitectura y API moderna.

## Fase 1 · Backend básico
- [ ] Implementar `vacaciones-service` con saldos y solicitudes (almacenamiento base JSON/SQL ligth).
- [ ] Integrar con Legajos para datos del empleado y saldo inicial.
- [ ] Exponer simulador de saldo + validaciones.

## Fase 2 · Workflow y aprobaciones
- [ ] Migrar workflow a Nucleus WF (Temporal/Durable) con etapas configurables.
- [ ] Bandeja de aprobaciones, notificaciones y delegaciones.
- [ ] Integración con Tiempos (bloqueo) y Liquidación (eventos) automática.

## Fase 3 · Experiencia de usuario
- [ ] UI portal empleado (calendario, historial, simulador, estado) y UI aprobador.
- [ ] Mobile-ready + notificaciones push/email.
- [ ] Políticas multi-país / multi-empresa.

## Fase 4 · Analytics y compliance
- [ ] Dashboard de cobertura, pendientes, forecasting.
- [ ] Políticas avanzadas (topes por temporada, pausas, licencias especiales).
- [ ] Auditoría completa, cumplimiento legal (ej. legislación argentina/latam).

---
*Última actualización: 2026-03-09*

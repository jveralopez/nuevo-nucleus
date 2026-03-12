# Roadmap · Reclamos / Nucleus WF

## Fase 0 · Descubrimiento (completo)
- [x] Analizar workflow y clases (`Reclamo.WF.xml`, `lib_v11.WFReclamos.RECLAMO.cs`).
- [x] Definir arquitectura/API.

## Fase 1 · Servicio base
- [ ] Diseñar modelo de datos (reclamos, estados, comentarios, adjuntos, catálogos).
- [ ] Implementar API CRUD + endpoints de comentarios/adjuntos (store temporal JSON o SQL lite).
- [ ] Integración mínima con Legajos para datos del empleado.

## Fase 2 · Workflow moderno
- [ ] Migrar etapas a Temporal/Durable (clasificar, resolver, confirmar).
- [ ] Reglas de SLA y escalamiento.
- [ ] Notificaciones (email/Teams) y publicación de eventos.

## Fase 3 · UI y experiencia
- [ ] Portal empleado (autoservicio) + bandeja para mesa de ayuda.
- [ ] Timeline detallado (similar a actual) con filtros/KPIs.
- [ ] Gestión de adjuntos y plantillas de respuesta.

## Fase 4 · Integraciones y hardening
- [ ] Integrar con ITSM / ServiceNow / Jira Ops.
- [ ] Reportes y dashboards (Power BI / Grafana) para SLA.
- [ ] Auditoría completa, OIDC y pipeline CI/CD.

---
*Última actualización: 2026-03-09*

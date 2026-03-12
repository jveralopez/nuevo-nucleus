# Roadmap · Nucleus WF

## Fase 0 · Descubrimiento (completo)
- [x] Relevar clases y componentes (`lib_v11.Workflows.*`, `lib_v11.Instancias.*`, `docs/11_nucleus_wf.md`).
- [x] Definir arquitectura/API modernizada.

## Fase 1 · Definition Service
- [ ] Diseñar modelo de datos (Workflows, Formularios, Roles, Versiones).
- [ ] Implementar `wf-definition-service` (CRUD, publicación, importación XML legado).
- [ ] Crear parser/migrador de `*.WF.xml` para acelerar porting.

## Fase 2 · Runtime Service
- [ ] Integrar con Temporal/Durable y exponer Runtime API (instancias, tareas, logs).
- [ ] Conectar con módulos existentes (Liquidación, Personal, Reclamos) mediante SDK/eventos.
- [ ] Implementar notificaciones y SLA.

## Fase 3 · Designer & Consolas
- [ ] UI visual (drag & drop) con versionado y preview.
- [ ] Consola de tareas e instancias (backoffice) con dashboards.
- [ ] Reportes y trazas (Grafana/Power BI) + auditoría.

## Fase 4 · Integraciones y seguridad
- [ ] Webhooks y extensibilidad (acciones serverless, connectors).
- [ ] Multi-tenant y aislamiento de datos.
- [ ] Hardening (OIDC, RBAC granular, pipelines CI/CD).

---
*Última actualización: 2026-03-09*

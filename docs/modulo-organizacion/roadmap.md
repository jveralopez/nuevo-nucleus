# Roadmap · Organización

## Fase 0 · Descubrimiento (completo)
- [x] Relevar clases `Organizacion` y dependencias en módulos.
- [x] Definir arquitectura/API moderna.

## Fase 1 · API base
- [ ] Modelar entidades (Empresas, Unidades, Posiciones, Centros de costo) en EF Core.
- [ ] Implementar endpoints CRUD y relaciones básicas.
- [ ] Integrar con Legajos para validar referencias.

## Fase 2 · Org Designer
- [ ] Diseñar UI de organigramas y versión de estructuras.
- [ ] Funcionalidades de simulación (what-if), comparaciones, exportes.
- [ ] Workflows de publicación/aprobación.

## Fase 3 · Integraciones
- [ ] Publicar eventos `OrgUnit*`, `Position*`, `OrgStructurePublished`.
- [ ] Integrar con Presupuesto, Liquidación, Tiempos, Portal y Data Lake.
- [ ] Importar datos desde sistemas maestros (ERP) si aplica.

## Fase 4 · Gobernanza
- [ ] Auditoría completa, historización, políticas de cambio.
- [ ] Herramientas de compliance (SoD, aprobaciones multi-step).
- [ ] Observabilidad y dashboards (headcount, ratios, span of control).

---
*Última actualización: 2026-03-09*

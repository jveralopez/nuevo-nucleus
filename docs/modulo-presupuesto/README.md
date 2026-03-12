# Módulo Presupuesto · Blueprint

## Objetivo
Modernizar el módulo de Presupuesto (referenciado en `Application.xml`) para gestionar planificación y control de headcount/costos de RRHH: presupuesto de personal, salarios, beneficios, gastos asociados y variaciones.

## Visión actual (23.01)
- Formularios y clases Nomad (no documentadas en detalle) que permiten cargar presupuestos por centro de costo, comparar vs real y exportar reportes.
- Integraciones presumibles con Liquidación (costos reales) y Organización (centros, posiciones).

## Propuesta moderna
1. **Presupuesto Service**: API para presupuestos de headcount y costos, con escenarios, versiones y comparaciones vs real.
2. **UI FP&A / HR**: dashboards y formularios para planificación, aprobaciones y análisis de variaciones.
3. **Integraciones**: Liquidación (costos reales), Organización (centros/posiciones), Personal (legajos), Finanzas/ERP (consolidación).
4. **Workflow**: Nucleus WF para ciclos de presupuesto (plan → revisión → aprobación → seguimiento).

---
*Blueprint conceptual basado en prácticas de FP&A HR.*

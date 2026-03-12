# Módulo Beneficios · Blueprint

## Objetivo
Modernizar el módulo de Beneficios (perks, convenios, compensaciones flexibles) para ofrecer un catálogo digital, gestión de inscripciones y administración de proveedores.

## Visión actual (23.01)
- Formularios/Clases Nomad (`Class/NucleusRH/Base/Beneficios/*`) para administrar beneficios, convenios y asignaciones.
- Interacciones básicas con Legajos y Liquidación (beneficios monetarios, descuentos).

## Propuesta moderna
1. **Beneficios Service**: API para catálogos de beneficios, campañas, elegibilidad, inscripciones y seguimiento de uso.
2. **Portal Empleado**: módulo para descubrir, solicitar y gestionar beneficios, con notificaciones y seguimiento en tiempo real.
3. **Integraciones**: proveedores externos, Liquidación (beneficios monetarios), Presupuesto (costos), Tesorería (pagos), Integrations Hub (reportes).
4. **Workflow**: Nucleus WF para aprobaciones (beneficios especiales, reembolsos, gadgets) y renovaciones.

---
*Blueprint conceptual basado en las necesidades típicas de Beneficios.*

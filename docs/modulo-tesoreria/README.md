# Módulo Tesorería · Blueprint

## Objetivo
Modernizar el módulo Tesorería (referenciado en `Application.xml`) que gestiona pagos, fondos y conciliaciones relacionados con RRHH (adelantos, préstamos, viáticos, etc.).

## Visión actual (23.01)
- Formularios/Clases Nomad (Tesoreria) que administran solicitudes de fondos, pagos y conciliaciones.
- Integraciones con Liquidación (nómina), Presupuesto (fondos) y Tesorería corporativa.

## Propuesta moderna
1. **Tesorería Service**: API para gestionar pagos RRHH (adelantos, Órdenes de pago), caja de adelantos, conciliaciones y fondos especiales.
2. **Workflow**: Nucleus WF para solicitudes/aprobaciones (adelantos, viáticos, préstamos), ciclo de pagos y conciliaciones.
3. **Integraciones**: Liquidación (resultados), Integrations Hub (archivos bancarios), ERP financiero, Presupuesto (seguimiento).

---
*Blueprint conceptual basado en necesidades de Tesorería RRHH.*

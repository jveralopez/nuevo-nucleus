# Módulo Sanciones / Disciplina · Blueprint

## Objetivo
Modernizar el módulo de Sanciones (disciplinario) para gestionar antecedentes disciplinarios, sanciones, notificaciones y acuerdos, alineado con Personal, Legajos y Nucleus WF.

## Visión actual (23.01)
- Clases/menús `Class/NucleusRH/Base/Sanciones/*` (ABM de sanciones, motivos, legajos) y workflows manuales.

## Propuesta moderna
1. **Sanciones Service**: API para sanciones, actas, acuerdos, historial y documentación adjunta.
2. **Workflow**: Nucleus WF para procesos disciplinarios (registro, investigación, descargo, resolución, seguimiento).
3. **Integraciones**: Legajos, Organizació n, Evaluación (impacto en puntajes), Portal Empleado (notificaciones/descargos), Integrations Hub (reportes legales).

## MVP implementado (2026-03-13)
- Workflow base en Nucleus WF (key `sanciones`, version `1.0.0`).
- Portal Empleado: descargo por sanción.
- Portal RH: bandeja de sanciones con acciones resolver/descartar.

---
*Blueprint conceptual.*

# Módulo Sanciones / Disciplina · Blueprint

## Objetivo
Modernizar el módulo de Sanciones (disciplinario) para gestionar antecedentes disciplinarios, sanciones, notificaciones y acuerdos, alineado con Personal, Legajos y Nucleus WF.

## Visión actual (23.01)
- Clases/menús `Class/NucleusRH/Base/Sanciones/*` (ABM de sanciones, motivos, legajos) y workflows manuales.

## Propuesta moderna
1. **Sanciones Service**: API para sanciones, actas, acuerdos, historial y documentación adjunta.
2. **Workflow**: Nucleus WF para procesos disciplinarios (registro, investigación, descargo, resolución, seguimiento).
3. **Integraciones**: Legajos, Organizació n, Evaluación (impacto en puntajes), Portal Empleado (notificaciones/descargos), Integrations Hub (reportes legales).

---
*Blueprint conceptual.*

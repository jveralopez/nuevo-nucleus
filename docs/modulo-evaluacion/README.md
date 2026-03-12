# Módulo Evaluación de Desempeño · Blueprint

## Objetivo
Actualizar el módulo de Evaluación de desempeño (referenciado en `Application.xml` y menús `Evaluacion`) para soportar procesos modernos de performance management: objetivos, feedback continuo, calibraciones y desarrollo.

## Visión actual (23.01)
- Formularios Nomad para evaluaciones (metas, competencias, peso, calificación).
- Workflow clásico: autoevaluación → evaluación jefe → RRHH → calibración.
- Reportes e interfaces para exportar resultados (no documentados en profundidad, deducidos del repositorio).

## Propuesta moderna
1. **Evaluación Service**: API para ciclos de performance, objetivos, competencias, formularios y evaluaciones.
2. **Workflow Nucleus WF**: orquestar etapas (auto, jefe, comité, calibración, cierre) con SLA y recordatorios.
3. **UI**: SPA para empleados, jefes y HR Business Partners (autoevaluaciones, feedback, dashboard de cumplimiento).
4. **Integraciones**: Legajos (datos del colaborador), Organización (estructura), Vacaciones (impacto en métricas), Tiempos (asistencia) y Presupuesto (bonus/merit).

---
*Basado en referencias del repositorio (menús Evaluación, docs generales) y buenas prácticas de performance management.*

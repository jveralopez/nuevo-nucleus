# Módulo Capacitaciones · Blueprint

## Objetivo
Modernizar el módulo de Capacitaciones (Capacitacion) que gestiona cursos, inscripciones, dictados y certificaciones.

## Estado actual (23.01)
- Formularios/Clases Nomad (`Class/NucleusRH/Base/Capacitacion/*`), menús y reportes.
- Funciones típicas: catálogos de cursos, programación de dictados, inscripciones, asistencia, evaluaciones, reportes legales (cap. mínima, etc.).

## Propuesta moderna
1. **Capacitacion Service**: API para cursos, versiones, sesiones, instructores, inscripciones y asistencias.
2. **Portal Empleado**: módulo para ver cursos, inscribirse, consumir contenido y hacer evaluaciones.
3. **Learning Ops UI**: para RRHH/Capacitación (crear planes, aprobar inscripciones, generar certificados).
4. **Integraciones**: con Legajos, Evaluación (planes de desarrollo), Medicina (campañas), Integrations Hub (reportes).

---
*Basado en funciones estándar de capacitación y referencias en Application.xml.*

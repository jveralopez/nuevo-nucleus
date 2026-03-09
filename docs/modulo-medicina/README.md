# Módulo Medicina Laboral · Blueprint

## Objetivo
Modernizar el módulo de Medicina Laboral (referenciado en `Application.xml`) para gestionar exámenes médicos, aptos, licencias médicas, campañas y seguimiento de salud ocupacional.

## Estado actual (23.01)
- Formularios y clases Nomad (no documentadas en detalle en `docs/` pero presentes en `Class/NucleusRH/Base/MedicinaLaboral/*`).
- Funciones usuales: agenda de exámenes, resultados, seguimiento de casos, reportes legales.
- Integración con Tiempos (ausencias), Legajos (datos del empleado) y Reclamos (accidentes).

## Propuesta moderna
1. **Medicina API**: microservicio para exámenes, licencias médicas, campañas de salud, proveedores médicos y reportes regulatorios.
2. **Workflow**: uso de Nucleus WF para procesos (programar examen, recepción de resultados, apto/no apto, licencias, regreso al trabajo).
3. **UI**: panel para equipo médico/RRHH, portal para empleados (consultar resultados, subir certificados).
4. **Integraciones**: con Tiempos (ausencias automáticas), Reclamos (accidentes), Integraciones (reportes legales), Portal Empleado.

---
*Basado en estructura del repositorio y necesidades típicas de Medicina Laboral.*

# Módulo Clima / Encuestas · Blueprint

## Objetivo
Modernizar el módulo de Clima/Encuestas (referencias en Application.xml) para gestionar encuestas de clima, engagement y comunicaciones internas, con analítica en tiempo real y acciones derivadas.

## Visión actual (23.01)
- Formularios/reportes específicos (no detallados) para encuestas periódicas; resultados se consumen vía reportes nominales.

## Propuesta moderna
1. **Clima Service**: API para campañas de encuestas, preguntas, respuestas, segmentación, y acciones correctivas.
2. **Survey UI**: front para empleados (Portal Empleado) y dashboards para RRHH/Managers.
3. **Integraciones**: Analytics (dashboards, alertas), Carrera/Sucesión (planes), Beneficios (campañas), Portal Empleado (microsurveys).
4. **Workflow**: Nucleus WF para seguimiento de planes derivados de resultados.

---
*Blueprint conceptual.*

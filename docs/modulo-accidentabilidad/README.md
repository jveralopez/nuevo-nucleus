# Módulo Accidentabilidad · Blueprint

## Objetivo
Modernizar el módulo de Accidentabilidad (accidents/incidentes laborales) para gestionar reportes, investigaciones, acciones correctivas y cumplimiento legal.

## Visión actual (23.01)
- Clases/menús en `Class/NucleusRH/Base/Accidentabilidad/*` (no documentadas en detalle) y workflows para reportar accidentes.
- Funcionalidades típicas: registro de incidentes, notificaciones a ART, seguimiento, reportes.
- Dependencias: Medicina Laboral, Tiempos, Seguridad e Integraciones legales.

## Propuesta moderna
1. **Accidentabilidad API**: registro de incidentes, investigaciones, acciones correctivas, relaciones con Legajos y Medicina.
2. **Workflow**: Nucleus WF para etapas (registro, investigación, notificación, cierre, seguimiento).
3. **Integraciones**: ART, ministerios, Data Lake para indicadores, Integrations Hub para reportes.
4. **UI**: panel para Seguridad/HSQE, formularios portal empleado, dashboards con KPI (frecuencia, gravedad, TIR, etc.).

---
*Basado en módulos legacy y buenas prácticas de seguridad industrial.*

# Módulo Seguridad / Control de Accesos · Blueprint

## Objetivo
Modernizar el módulo Seguridad (seguridad física y control de accesos) mencionado en `Application.xml`, alineándolo con Control de Visitas y Accidentabilidad para ofrecer monitoreo integral de accesos, alertas y cumplimiento.

## Visión actual (23.01)
- Formularios/Clases Nomad (`Class/NucleusRH/Base/Seguridad/*`) para registrar accesos, alertas, rondas, permisos, etc. Integrado a Control de Visitas y Tiempos.

## Propuesta moderna
1. **Seguridad Service**: API que registra accesos, rondas, incidentes de seguridad, permisos especiales y alertas.
2. **Integraciones**: sistemas de control de acceso físico (turnstiles, badges), cámaras, Control de Visitas, Accidentabilidad.
3. **UI Seguridad**: dashboard para equipo de seguridad/HSQE con monitoreo en tiempo real, alertas configurables, reportes.
4. **Workflow**: Nucleus WF para procesos de permisos especiales, alertas, investigaciones.

---
*Blueprint conceptual basado en la línea de seguridad física.*

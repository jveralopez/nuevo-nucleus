# Resumen de descubrimiento y próximos pasos

## Qué se hizo
- **Relevamiento completo del repo 23.01**: analizamos documentos (`docs/*.md`), workflows XML, clases Nomad (`Class/NucleusRH/Base/*`), interfaces (`Interfaces/`, `InterfacesOut/`), menús (`Menu/NucleusRH/Base/*`) y templates (`WebCV`).
- **Blueprint por módulo**: generamos documentación moderna (arquitectura, API, roadmap) para:
  - Liquidación, Personal, Tiempos, Vacaciones, Reclamos, Selección/WebCV, Nucleus WF, Integraciones, Portal Empleado, Organización, Evaluación, Capacitaciones, Beneficios, Medicina Laboral, Control de Visitas, Accidentabilidad, Presupuesto, Tesorería, Seguridad, Configuración Avanzada, Sanciones, Analytics/Reportes, Carrera/Sucesión, Clima/Encuestas.
- **Documento de interacciones** (`interacciones_modulos.md`): describe cómo se relaciona cada módulo (eventos, APIs, dependencias).
- **Maqueta navegable** (`maqueta_portal.html`): UI demo con tarjetas y simulador para todos los módulos.
- **Diseño de base de datos** (`db_design_overview.md`): tabla resumen de entidades y relaciones para implementar las nuevas bases.

## Qué aprendimos sobre el sistema actual
- Recibos y liquidaciones se generan en `Class/NucleusRH/Base/Liquidacion/lib_v11.*`; las corridas y exportes usan `Liquidacion.LIQUIDACION.*` y `InterfacesOut`.
- Workflows (Vacaciones, Personal, Reclamos) están definidos en `Workflow/NucleusRH/Base/*/*.WF.xml` y ejecutados por clases `WFSolicitud`/`WFReclamos`.
- Tiempos (TTA) controla fichadas, turnos, licencias y exporta `ArchivoHoras.XML`.
- Integraciones legacy usan `Generico.exe` + templates XML; será reemplazado por un Integration Hub.
- Portal Empleado actual son múltiples páginas/formularios; lo consolidaremos en un SPA + BFF.

## Dónde iniciar en el nuevo entorno
1. **Configurar infraestructura base**: repos (mono o multi), CI/CD (GitHub Actions), repos de config (GitOps), contenedores (Docker), IaC (Terraform/Bicep).
2. **Servicios core prioritarios**:
   - **Organization/Personal** (definir datos maestros) → necesario para los demás.
   - **Liquidación/Tesorería/Integraciones** (punto crítico empresarial).
   - **Tiempos/Vacaciones** (autoservicio más usado).
3. **Portal Empleado + Nucleus WF**: montar BFF + shell y el motor workflow (Definition + Runtime) que usaremos en Vacaciones, Reclamos, Sanciones, etc.
4. **Data Platform**: levantar pipelines y warehouse para Analytics.
5. **Plan de migración**: seguir el roadmap por fases que definimos en cada módulo (Fase 0 completada = descubrimiento).

## Orden recomendado de trabajo
1. **Foundation**: Organization + Personal + Configuración + Portal BFF + Nucleus WF + Integrations Hub.
2. **Autoservicio crítico**: Liquidación, Tiempos, Vacaciones, Portal Empleado (primera versión), Integraciones bancarias.
3. **Soporte**: Reclamos, Sanciones, Medicina, Tesorería, Presupuesto.
4. **Talent**: Selección, Evaluación, Carrera/Sucesión, Capacitaciones, Beneficios, Clima.
5. **Seguridad física / Control de Visitas / Accidentabilidad**.
6. **Analytics**: Data platform y dashboards.

## Cómo retomar rápidamente
- Revisar `docs/interacciones_modulos.md` para entender dependencias.
- Revisar `docs/db_design_overview.md` para iniciar migraciones o diseñar esquemas físicos.
- Usar `maqueta_portal.html` como referencia visual para priorizar desarrollo UI.
- Consultar cada `docs/modulo-*/README.md` para recordar alcance, API y roadmap de cada módulo.
- Llevar el backlog de implementación siguiendo los roadmaps por módulo (Fase 1, Fase 2, etc.).

Con esto, la etapa de descubrimiento queda formalmente cerrada y tenemos la hoja de ruta clara para empezar el desarrollo en un entorno nuevo sin re-analizar el repositorio legado.

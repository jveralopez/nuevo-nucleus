# Backlog inicial (arranque)

## Objetivo
Convertir el resumen de descubrimiento en un plan ejecutable por sprints, con dependencias claras y criterios de terminado (DoD) para iniciar el nuevo entorno.

## Supuestos operativos
- Este backlog no cambia el comportamiento funcional del sistema legado.
- La fase actual es Foundation + primeros servicios core.
- El backlog se ejecuta por entregas incrementales y se valida por demos funcionales.

## Epics y prioridades
1. Foundation (infra + plataforma)
2. Identity & Access (autenticacion, autorizacion, tenant)
3. Organization & Personal (datos maestros)
4. Nucleus WF (definition + runtime)
5. Integration Hub (sustituto de InterfacesOut)
6. Portal Empleado (shell + BFF)
7. Liquidacion/Tesoreria (primer flujo critico)
8. Tiempos/Vacaciones (autoservicio)
9. Data Platform (analytics basico)

## Sprints propuestos (4 semanas, 2 semanas cada uno)

### Sprint 0 (Semana 1-2) - Foundation
**Objetivo:** entorno listo para desarrollar y desplegar servicios core.
**Entregables:**
- Repos base (mono o multi), estructura estandar y convenciones.
- CI/CD inicial (build, test, lint, seguridad basica).
- Contenedores base y entorno local reproducible.
- IaC baseline (ambientes dev/test) y secretos gestionados.
- Observabilidad minima (logging estructurado, trazas basicas).

**DoD:**
- Build y tests corren en CI en cada PR.
- Entorno local levanta 1 servicio ejemplo + base.
- Despliegue automatizado a dev sin pasos manuales.

### Sprint 1 (Semana 3-4) - Core Master Data
**Objetivo:** habilitar datos maestros para el resto de modulos.
**Entregables:**
- Servicio Organization (estructura organizacional, centros, puestos).
- Servicio Personal (legajos, datos basicos, documentos).
- Esquema fisico inicial (migraciones) y versionado.
- API base documentada (OpenAPI) y contrato estable.

**DoD:**
- CRUD completo con validaciones y auditoria.
- Migraciones automatizadas en CI.
- Datos maestros consumibles por otros servicios.

### Sprint 2 (Semana 5-6) - Nucleus WF + Integration Hub (MVP)
**Objetivo:** motor workflow funcional y canal de integraciones.
**Entregables:**
- Nucleus WF Definition (modelos, versionado, publicacion).
- Nucleus WF Runtime (ejecucion, estados, transiciones).
- Integration Hub MVP (entrada/salida con plantillas basicas).

**DoD:**
- Workflow demo ejecuta inicio->aprobacion->cierre.
- Runtime expone eventos y estado consultable.
- Hub procesa al menos 1 interfaz real simulada.

### Sprint 3 (Semana 7-8) - Portal Empleado + BFF (v1)
**Objetivo:** portal funcional con primer flujo de autoservicio.
**Entregables:**
- Portal shell (navegacion, layout, autenticacion).
- BFF con endpoints para portal.
- Primer flujo: Solicitud simple (ej. vacaciones basicas) usando WF.

**DoD:**
- Usuario puede autenticarse, navegar y crear solicitud.
- Solicitud dispara workflow y puede consultarse su estado.
- UI soporta escritorio y mobile.

## Backlog detallado por epic (primeras historias)

### Epic: Foundation
- Definir estructura de repos (apps, libs, services, infra).
- Pipeline CI/CD: build/test/lint/security.
- Base de logs estructurados + trazas.
- Plantilla de servicio base (health, config, telemetry).

### Epic: Identity & Access
- Proveedor de identidad (OIDC) definido y documentado.
- Autorizacion por roles (portal + APIs).
- Token propagation y trazabilidad.

### Epic: Organization & Personal
- Modelo de datos base (organizacion, persona, contrato, puesto).
- Validaciones de negocio (campos obligatorios, duplicados).
- Import inicial (batch simple) para pruebas.

### Epic: Nucleus WF
- DSL o schema para definiciones de workflow.
- Persistencia de instancias y transiciones.
- API de consultas (estado, historial, tareas).

### Epic: Integration Hub
- Conector de archivos (CSV/XML) + mapping basico.
- Plantillas y versionado.
- Auditoria y trazabilidad de ejecucion.

### Epic: Portal Empleado
- Shell SPA + routing.
- Componentes base (formularios, tarjetas, estados).
- Flujo inicial de solicitud con seguimiento.

## Dependencias clave
- Organization/Personal depende de Foundation.
- Nucleus WF depende de Foundation + Identity.
- Portal depende de Identity + BFF + WF.
- Integration Hub depende de Foundation + Observabilidad.

## Riesgos y mitigaciones
- Ambito funcional sobredimensionado -> dividir por MVPs medibles.
- Integraciones legacy -> priorizar 1 interfaz critica primero.
- Datos maestros incompletos -> plan de carga inicial y mapeo.

## Siguiente paso inmediato (Semana 1)
- Confirmar stack y estructura de repos.
- Definir primer servicio plantilla y pipeline.
- Alinear DoD con stakeholders.

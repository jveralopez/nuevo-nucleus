# Módulo Nucleus WF (motor de workflows) · Blueprint

## Objetivo
Modernizar el motor de workflows propietario (definición + ejecución) descrito en `docs/11_nucleus_wf.md`, manteniendo sus capacidades de definición de procesos, formularios, variables y organigramas, pero migrando a una plataforma actual (Temporal.io / Durable Functions) con APIs declarativas y tooling moderno.

## Funciones actu ales (23.01)
- **Definición**: clases `lib_v11.Workflows.*` permiten crear WF, procesos, nodos, formularios, variables y organigramas; menús `WRK.menu.xml` administran estas entidades.
- **Ejecución**: `lib_v11.Instancias.INSTANCE` maneja instancias, logs y errores; reportes HTML muestran bandejas y tareas.
- **Integraciones**: `INT_MAG` y configuraciones en `Menu/NucleusWF/Base/WRK.menu.xml` conectan workflows con módulos (Vacaciones, Personal, Reclamos, etc.).

## Enfoque moderno
1. **Workflow Platform**: motor central basado en Temporal/Durable Functions con DSL declarativa (YAML/JSON) para definir procesos, formularios, roles y acciones.
2. **Definition API**: microservicio `wf-definition-service` para CRUD de procesos, nodos, formularios, variables y organigramas.
3. **Runtime API**: `wf-runtime-service` para disparar instancias, administrar tareas, logs y eventos.
4. **Designer UI**: web app interactiva (drag & drop) para diseñar workflows, versionar y publicar.
5. **Observers & Integrations**: webhooks/eventos para notificar módulos (Personal, Liquidación, Reclamos) y permitir acciones automáticas.

---
*Basado en `docs/11_nucleus_wf.md`, `Class/NucleusWF/Base/*`, `Menu/NucleusWF/Base/WRK.menu.xml`.*

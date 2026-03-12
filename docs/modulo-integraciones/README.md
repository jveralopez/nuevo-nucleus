# Módulo de Integraciones · Blueprint

## Objetivo
Modernizar el esquema de interfaces batch `InterfacesOut`/`Interfaces` (archivo `Generico.exe`, templates XML y definiciones para bancos/legales/sindicatos) en una plataforma event/batch hybrid que permita integraciones seguras, auditables y configurables (REST, eventos, archivos, SFTP).

## Componentes actuales (23.01)
- **InterfacesOut**: generador genérico (`InterfacesOut/Source/Generico/Program.cs`, `clsGenericWriter.cs`, `clsFunctions.cs`) que toma definiciones XML (`InterfacesOut/Definitions/*.xml`) y produce archivos planos/binary para bancos, legales, sindicatos.
- **Interfaces (entrada)**: definiciones XML en `Interfaces/NucleusRH/Base/*` (ej. `Liquidacion/*.XML`, `Tiempos_Trabajados/Liquidacion/ArchivoHoras.XML`).
- **Servicios Nomad**: `SQLService`, `FileServiceIO`, `StoreService`, `OutputMails`.

## Propuesta moderna
1. **Integration Hub**: microservicio `integration-orchestrator` que gestiona templates, jobs, conexiones y monitoreo.
2. **Connectores**: componentes especializados para bancos, AFIP/SICOSS, sindicatos, Data Lake, APIs SaaS.
3. **Pipelines**: event-driven (Service Bus/Kafka) + batch orchestrado (Durable Functions/Logic Apps/Azure Data Factory).
4. **Configuración declarativa**: templates YAML/JSON (similar a actuales XML) guardadas en repo/config store.
5. **UI de monitoreo**: panel para programar, lanzar y revisar integraciones, con logs y alerta.

## MVP implementado
- Scheduler con reintentos.
- Templates, conexiones, jobs y eventos en SQLite.
- Endpoints de eventos para auditoria.
- Triggers configurables con ejecucion manual.
- Rate limiting global en Integration Hub.

---
*Basado en `docs/05_interfaces_e_integraciones.md`, `InterfacesOut/*`, `Interfaces/*`.*

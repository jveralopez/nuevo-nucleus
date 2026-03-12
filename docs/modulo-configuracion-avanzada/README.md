# Módulo Configuración Avanzada · Blueprint

## Objetivo
Modernizar el módulo de Configuración (parámetros, catálogos, diccionarios, políticas) que soporta toda la suite Nucleus RH (`docs/02_arquitectura_y_componentes.md`, `Config/` y `Menu/NucleusRH/Base/CONFIG`).

## Alcance actual (23.01)
- Archivos XML (`Config/*`), diccionarios Nomad, ABMs en `Menu/NucleusRH/Base/CONFIG`, catálogos (tipos de documento, licencias, sindicatos, etc.).
- Configuración distribuida, sin versionado ni auditoría centralizada.

## Propuesta moderna
1. **Configuration Service**: API centralizada para parámetros, catálogos, diccionarios, feature flags, integraciones.
2. **UI Config Center**: portal para administrar catálogos, entornos, plantillas y migraciones.
3. **Versioning & GitOps**: configuración como código (YAML/JSON), versionado en Git, despliegues automatizados.
4. **Audit & Security**: control de cambios, approvals, roles granulares.

## MVP implementado
- `configuracion-service/` con catalogos y parametros (CRUD basico).
- SQLite + EF Core + migraciones.

---
*Blueprint conceptual basado en las necesidades de configuración global.*

# Módulo Organización · Blueprint

## Objetivo
Modernizar el módulo de Organización (empresas, posiciones, estructuras, organigramas, centros de costo) presente en Nucleus RH (`Class/NucleusRH/Base/Organizacion/*`, `docs/02_arquitectura_y_componentes.md`). Este módulo provee la base jerárquica para Legajos, Vacaciones, Tiempos, Liquidación y Presupuesto.

## Alcance actual (23.01)
- Clases `lib_v11.*` (ORG03_EMPRESAS, ORG04_POSICIONES, organigramas) y configuraciones XML.
- Menús y formularios Nomad para ABMs de empresas, áreas, puestos, centros de costo.
- Dependencia en workflows (Personal/Vacaciones) para combos y validaciones.

## Visión moderna
1. **Organización Service**: API consolidada para empresas, estructuras, posiciones, centros de costo y organigramas (DDD + EF Core).
2. **Org Designer UI**: interfaz visual para construir organigramas, unidades, matrices y versiones de estructura.
3. **Integración con Legajos, Presupuesto y Liquidación**: exposiciones de datos (REST/GraphQL) y eventos `OrgUnitUpdated`, `PositionCreated`.

---
*Basado en `Class/NucleusRH/Base/Organizacion`, `docs/02_arquitectura_y_componentes.md` y dependencias en workflows.*

# Módulo Analytics / Reportes · Blueprint

## Objetivo
Modernizar la capa de reportes (documentada en `docs/04_reportes.md` y `Html/*`) para ofrecer Analytics centralizados (data warehouse, dashboards, self-service BI) con datos de todos los módulos.

## Visión actual (23.01)
- Reportes HTML y exportes puntuales (Nomad, Crystal/HTML). Cada módulo genera sus informes de forma separada.
- Falta un modelo de datos central y pipelines modernos.

## Propuesta moderna
1. **Data Platform**: pipelines ETL/ELT (Data Lake + Warehouse) que consolidan datos de Liquidación, Personal, Tiempos, etc. (Azure Data Factory/Databricks/Synapse/BigQuery).
2. **Analytics Service**: API/GraphQL para consultas agregadas + dashboards preconstruidos (Power BI/Looker/Grafana).
3. **Self-service**: Portal Analytics integrado al Portal Empleado/Backoffice con permisos y datasets disponibles.
4. **Advanced Insights**: modelos IA/ML (rotación, ausentismo, cost forecasting).

---
*Blueprint conceptual.*

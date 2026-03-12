# Módulo de Liquidación · Nucleus RH Next

## Objetivo
Construir una versión moderna del módulo de liquidación que reemplace la lógica heredada basada en Nomad (`Class/NucleusRH/Base/Liquidacion/*`, `Interfaces/NucleusRH/Base/Liquidacion/*.XML`). Debe permitir crear ciclos de nómina, administrar legajos incluidos y generar recibos/listados listos para integraciones bancarias y legales.

## Contexto analizado
- **Clases heredadas**: `lib_v11.Liquidacion.LIQUIDACION.cs`, `lib_v11.Legajo_Liquidacion.cs`, `lib_v11.Empresa_Liquidacion.EMPRESA.cs` concentran la lógica actual (cálculos, workflows y exportaciones).
- **Interfaces**: `Empresa_Liquidacion.InterfaceContable.XML`, `Empresa_Liquidacion.InterfaceSIJP.XML` y `Legajo_Liquidacion.ReporteCertificacionHaberes.xml` definen la salida plana/bancos.
- **Procesos relacionados**: exportación de horas (ver `Interfaces/NucleusRH/Base/Tiempos_Trabajados/Liquidacion/ArchivoHoras.XML`) y casos de prueba CP-15 (`docs/08_casos_de_prueba.md`).

## Enfoque de reconstrucción
1. **Microservicio .NET 8 (Liquidacion Service)** con dominio DDD simplificado (PayrollRun, Legajo, Recibo).
2. **API pública REST** para crear ciclos, agregar o quitar legajos y procesar/emitir recibos.
3. **Motor de reglas inicial** basado en cálculos determinísticos (básico, antigüedad, descuentos). Extendible con estrategias por convenio.
4. **Persistencia SQL** (SQLite en PoC) con EF Core y rehidratación de colecciones con altas explícitas; migrable a SQL Server.
5. **UI mínima** (SPA ligera) que consume la API y demuestra el flujo end-to-end.

## Artefactos entregados
- `liquidacion-service/`: servicio ASP.NET Core 8 con endpoints y lógica de negocio.
- `liquidacion-ui/`: interfaz HTML/JS para operar el módulo.
- Integración base con `integration-hub-service` para disparar jobs al exportar.
- Documentos de arquitectura y API (en esta carpeta).
- Listado de exportes disponible via `GET /payrolls/{id}/exports`.
- Base de conocimiento de liquidación argentina: `docs/modulo-liquidacion/base_conocimiento_argentina.md`.
- Esquema de versionado de tablas de Ganancias: `docs/modulo-liquidacion/tablas_ganancias_versionado.md`.
- Proceso de actualización de Ganancias: `docs/modulo-liquidacion/proceso_actualizacion_ganancias.md`.
- Variables IG observadas en legado: `docs/modulo-liquidacion/variables_ganancias_legado.md`.
- Herramientas de importacion: `tools/ganancias/README.md`.
- Extractor de PDFs oficiales: `tools/ganancias/extract_pdf_tables.py`.
- Validador de tablas oficiales: `tools/ganancias/validate_tables.py`.
- Catálogo base de ítems de sueldo: `docs/modulo-liquidacion/catalogo_items_sueldo_base.md`.
- Catálogos exportados: `data/catalogos/README.md`.
- Reglas de conceptos (versionadas): `data/reglas/conceptos/reglas.json`.
- Reglas por convenio: `data/reglas/conceptos/convenios/README.md`.
- Generación de reglas por convenio desde catálogo: `data/reglas/conceptos/README.md`.
- Material reutilizable 23.01: `docs/modulo-liquidacion/material_reutilizable_23.01.md`.

## Próximos hitos
| Hito | Descripción | Estado |
| --- | --- | --- |
| Fundaciones | Análisis del módulo legado y definición de dominio | ✅ |
| Servicio base | API REST con persistencia SQL y cálculos netos | ✅ |
| UI operativa | Panel capaz de crear/gestionar liquidaciones desde navegador | ✅ |
| Integraciones | Exportes contables/bancos + hooks event-driven | 🚧 |

---
*Documento generado el 2026-03-09 a partir del repositorio `23.01` y la nueva implementación en `C:\trabajo\nuevo-nucleus`.*

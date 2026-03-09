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
4. **Almacenamiento ligero** (JSON file store) para la PoC; migrable a SQL Server/EF Core.
5. **UI mínima** (SPA ligera) que consume la API y demuestra el flujo end-to-end.

## Artefactos entregados
- `liquidacion-service/`: servicio ASP.NET Core 8 con endpoints y lógica de negocio.
- `liquidacion-ui/`: interfaz HTML/JS para operar el módulo.
- Documentos de arquitectura y API (en esta carpeta).

## Próximos hitos
| Hito | Descripción | Estado |
| --- | --- | --- |
| Fundaciones | Análisis del módulo legado y definición de dominio | ✅ |
| Servicio base | API REST con almacenamiento en archivo y cálculos netos | 🚧 |
| UI operativa | Panel capaz de crear/gestionar liquidaciones desde navegador | ⏳ |
| Integraciones | Exportes contables/bancos + hooks event-driven | ⏳ |

---
*Documento generado el 2026-03-09 a partir del repositorio `23.01` y la nueva implementación en `C:\trabajo\nuevo-nucleus`.*

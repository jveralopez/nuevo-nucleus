# Arquitectura del nuevo módulo de Liquidación

## Vista de bloques
```mermaid
flowchart LR
  subgraph Cliente
    UI[Liquidacion UI]
    CLI[Automations/CLI]
  end
  subgraph Servicios
    API[Liquidacion Service (.NET 8)]
    Rules[Motor de reglas]
    Storage[FileStore / SQL]
    Integraciones[(Bancos/Legales)]
  end
  UI -->|REST JSON| API
  CLI -->|REST| API
  API --> Rules
  API --> Storage
  API -->|Eventos| Integraciones
```

## Capas principales
1. **API / BFF**: ASP.NET Core minimal API con endpoints REST (`/payrolls`, `/payrolls/{id}/legajos`, `/payrolls/{id}/procesar`, `/payrolls/{id}/exports`). Middleware de validación, logging y versionado.
2. **Dominio**: entidades `PayrollRun`, `LegajoEnLote`, `Recibo`. Servicios `PayrollService` y `ReceiptFactory` encapsulan la lógica de negocio.
3. **Persistencia**: EF Core sobre SQLite con `EfPayrollRepository` y `PayrollDbContext`. El repositorio rehidrata colecciones y marca nuevos items como `Added` para evitar conflictos de concurrencia al actualizar legajos/recibos. Se definió interfaz `IPayrollRepository` para desacoplar.
4. **Motor de reglas**: estrategia extensible para cálculos. Para la PoC se implementa `StandardPayRuleSet` con: remunerativo = básico + antigüedad + ajustes; deducciones = aportes (11%) + obra social (3%) + sindicato (2%).
5. **Integraciones**: pipeline `ExportService` genera archivos CSV y JSON para bancos/contabilidad simulando `Empresa_Liquidacion.InterfaceContable.XML`.
6. **UI**: SPA liviana (HTML + JS) en Portal RH que consume los endpoints y permite crear liquidaciones, agregar/quitar legajos, generar recibos y descargar exportes.

## Componentes internos
| Componente | Responsabilidad | Referencia |
| --- | --- | --- |
| `Program.cs` | Configura DI, endpoints y swagger minimal | `liquidacion-service/Program.cs` |
| `Domain/Models/PayrollRun.cs` | Define entidades y estados | `liquidacion-service/Domain/Models` |
| `Services/PayrollService.cs` | Operaciones de alto nivel (crear/actualizar/procesar) | `liquidacion-service/Services` |
| `Infrastructure/EfPayrollRepository.cs` | Persistencia con EF Core | `liquidacion-service/Infrastructure` |
| `Services/ReceiptExporter.cs` | Genera recibos + archivos | `liquidacion-service/Services` |
| `web/` | UI estática con fetch hacia la API | `liquidacion-ui/` |

## Seguridad y operaciones
- **Autenticación**: JWT emitido por `auth-service`. Lecturas autenticadas y escrituras con rol `Admin`.
- **Observabilidad**: logging por request con `X-Trace-Id`.
- **Deploy**: contenedores Docker (no generado aún) planificados para AKS/EKS. Pipelines con GitHub Actions.

## Dependencias
- .NET SDK 8.0
- Node 18+ (solo si se reemplaza la UI estática por React). Para esta entrega, la UI es HTML/JS.

---
*Basado en el análisis de `Class/NucleusRH/Base/Liquidacion/*` y `Interfaces/NucleusRH/Base/Liquidacion/*.XML`.*

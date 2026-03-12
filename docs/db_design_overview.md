# Diseño de base de datos objetivo

Este documento consolida las entidades y relaciones extraídas del relevamiento. Sirve como guía para implementar el modelo físico (SQL Server/Oracle) o definiciones EF Core (migraciones).

## Módulo Personal / Organización
- **Legajos** (`Legajos`)
  - Campos: `Id (GUID)`, `Numero`, `Nombre`, `Apellido`, `ApellidoMaterno`, `DocumentoTipoId`, `DocumentoNumero`, `CUIL`, `Sexo`, `FechaNacimiento`, `Email`, `Telefono`, `EmpresaId`, `OrgUnitId`, `PositionId`, `FechaIngreso`, `Estado`.
  - Relación con `Empresas`, `OrgUnits`, `Posiciones`, `Documentos`, `Domicilios`, `Familiares`, `LegajosLaborales`.
- **Empresas / OrgUnits / Posiciones / Centros de costo** (ver `docs/modulo-organizacion/`)
- **Documentos** (`LegajosDocumentos`), **Domicilios** (`LegajosDomicilios`), **Familiares** (`LegajosFamiliares`).

## Módulo Tiempos
- **Fichadas** (`TimeEntries`): `Id`, `LegajoId`, `FechaHora`, `Tipo`, `Fuente`, `TerminalId`.
- **Turnos / Horarios** (`WorkShifts`, `ShiftAssignments`).
- **Procesamientos de horas** (`TimeProcess`, `TimeResults`).

## Módulo Vacaciones
- **Solicitudes de Vacaciones** (`VacationRequests`): `Id`, `LegajoId`, `Periodo`, `FechaDesde`, `FechaHasta`, `Dias`, `Tipo`, `Estado`, `WorkflowInstanceId`.
- **Saldos** (`VacationBalances`): `LegajoId`, `Periodo`, `DiasDisponibles`, `DiasTomados`, `DiasBonificados`.

## Módulo Liquidación
- **PayrollRun** (`PayrollRuns`): `Id`, `Periodo`, `Tipo`, `Estado`, `Descripcion`, `CreatedAt`, `UpdatedAt`.
- **Legajo en lote** (`PayrollLegajos`): `Id`, `PayrollRunId`, `LegajoId`, `Basico`, `Antiguedad`, `Adicionales`, `Descuentos`.
- **Recibos** (`PayrollReceipts`): `Id`, `PayrollRunId`, `LegajoId`, `Remunerativo`, `Deducciones`, `Neto` + detalle (`PayrollReceiptDetails`: `Concepto`, `Importe`).

## Módulo Integraciones
- **Templates** (`IntegrationTemplates`): `Id`, `Nombre`, `Version`, `Config` JSON.
- **Jobs** (`IntegrationJobs`): `Id`, `TemplateId`, `Estado`, `OutputFiles`, `Log`.

## Módulo Tesorería
- **Solicitudes de pago** (`PaymentRequests`): `Id`, `LegajoId`, `Tipo`, `Monto`, `Moneda`, `Motivo`, `Estado`, `WorkflowInstanceId`.
- **Pagos** (`Payments`): `Id`, `RequestId`, `FechaPago`, `Metodo`, `Cuenta`, `Referencia`, `Estado`.
- **Fondos** (`Funds`), **Conciliaciones** (`Reconciliations`).

## Módulo Presupuesto
- **Presupuestos** (`Budgets`), **Versiones** (`BudgetVersions`), **Líneas** (`BudgetLines`), **Supuestos** (`BudgetAssumptions`), **Actuals** (`BudgetActuals`).

## Módulo Reclamos
- **Reclamos** (`Claims`/`Tickets`): `Id`, `Tipo`, `CategoriaId`, `OrigenId`, `Estado`, `LegajoId`, `Descripcion`, `WorkflowInstanceId`.
- **Comentarios** (`ClaimComments`), **Adjuntos** (`ClaimAttachments`).

## Módulo Sanciones
- **Casos disciplinarios** (`DisciplinaryCases`), **Etapas** (`DisciplinarySteps`), **Documentos**, **Planes**.

## Módulo Accidentabilidad / Medicina
- **Incidentes** (`Incidents`), **Investigaciones** (`Investigations`), **Actions** (`CorrectiveActions`), **Notificaciones**.
- **Exámenes médicos** (`MedicalExams`), **Licencias** (`MedicalLicenses`), **Campañas** (`MedicalCampaigns`).

## Módulo Capacitaciones
- **Cursos** (`Courses`), **Sesiones** (`Sessions`), **Inscripciones** (`Enrollments`), **Asistencias** (`Attendance`), **Contenido** (`CourseContent`), **Evaluaciones** (`CourseEvaluations`).

## Módulo Beneficios
- **Beneficios** (`Benefits`), **Inscripciones** (`BenefitEnrollments`), **Reembolsos** (`BenefitClaims`), **Wallets** (`FlexibleWallets`).

## Planes de Carrera / Sucesión
- **PlanCarrera** (`CareerPlans`), **PosicionesCriticas** (`SuccessionPositions`), **Sucesores** (`SuccessorCandidates`), **TalentPools`.

## Clima / Encuestas
- **Campañas** (`SurveyCampaigns`), **Preguntas** (`SurveyQuestions`), **Respuestas** (`SurveyResponses`), **Acciones** (`SurveyActions`).

## Analytics / Data Platform
- **Data Lake / Warehouse** (no tablas específicas en este documento, depende del stack). Sugerencia: hechos/dimensiones basados en las entidades anteriores (FactPayroll, FactTime, FactVacation, FactIncidents, FactClaims, FactBenefits, etc.).

## Configuración
- **Parámetros** (`ConfigParameters`), **Catálogos** (`CatalogEntries`), **Conexiones** (`ConfigConnections`), **Releases** (`ConfigReleases`).

## Nucleus WF
- **WorkflowDefinitions**, **WorkflowInstances**, **WorkflowTasks**, **WorkflowLogs** (ver `docs/modulo-nucleuswf/`).

## Relaciones clave
- `Legajos` es la entidad central, referenciada por la mayoría de los módulos (Vacaciones, Tiempos, Liquidación, Tesorería, Beneficios, Capacitaciones, Sanciones, Accidentes, Medicina, Carrera, etc.).
- `Organization` provee `OrgUnits`, `Positions`, `CostCenters` usados por Presupuesto, Liquidación, Tiempos, Beneficios, etc.
- Los módulos de workflow referencian `WorkflowInstances` para trazabilidad.
- Cada módulo publica eventos (Nucleus WF, Integrations Hub) para sincronizar datos entre servicios.

## Implementación
- Recomendada: SQL Server / PostgreSQL + EF Core para los servicios .NET (cada servicio con su schema/database). Fijar convenciones (naming, claves, timestamps, `rowversion`).
- Para Data Platform: Data Lake (Parquet) + Warehouse (Synapse/Databricks/BigQuery). Modelos star/dimension.
- Configuración: usar tabla central y/o JSON store + GitOps.

Con este diseño se puede iniciar la creación de migraciones e implementar las bases de datos físicas, en paralelo con el desarrollo de servicios y APIs correspondientes.

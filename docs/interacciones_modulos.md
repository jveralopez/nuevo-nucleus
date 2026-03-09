# Interacciones entre módulos y resolución propuesta

Este documento resume las interacciones clave identificadas durante el relevamiento de la versión 23.01 y cómo se resolverán en la arquitectura moderna. Sirve como guía para coordinar el desarrollo en el nuevo entorno.

## 1. Personal / Organización
- **Entradas**: catálogos de Organización (empresas, unidades, posiciones) y catálogos globales (tipos de documento, nacionalidades, etc.).
- **Salidas**: Perfil 360° del legajo, datos laborales, domicilios y familiares.
- **Interacciones**:
  - Provee datos a Tiempos, Vacaciones, Liquidación, Evaluación, Beneficios, Capacitaciones, Sanciones, Carrera/Sucesión, Control de Visitas, Medicina, Accidentabilidad.
  - Consume Organización para estructura y centros de costo.
- **Resolución**: Personal Service expone `GET /legajos` + `GET /legajos/{id}/profile`; Organization Service provee catálogos/estructuras. Eventos `LegajoCreated/Updated` alimentan otros servicios.

## 2. Tiempos ↔ Liquidación ↔ Vacaciones
- **Tiempos**: captura fichadas, turnos, licencias y genera resultados de horas (`ArchivoHoras` actualizado a pipeline).
- **Vacaciones**: solicita y aprueba licencias; envía eventos `VacationApproved/Rejected`.
- **Liquidación**: toma resultados de Tiempos y Vacaciones para calcular conceptos/remuneraciones.
- **Resolución**:
  - Eventos `HoursProcessed`, `VacationApproved` alimentan Liquidación service.
  - Liquidación expone `/payrolls/{id}/process` y publica `PayrollProcessed/Exported` para Tesorería e Integraciones.
  - Tiempos bloquea turnos vía API cuando se aprueba una vacación.

## 3. Liquidación ↔ Tesorería ↔ Integraciones ↔ Presupuesto
- Liquidación genera recibos y archivos (CSV/JSON). Eventos `PayrollProcessed` se consumen por Tesorería (para adelantos/deducciones) y Presupuesto (actualiza reales).
- Tesorería gestiona adelantos/viáticos y publica `PaymentCompleted` → Liquidación puede descontar en payroll siguiente.
- Integrations Hub reemplaza `Generico.exe` para exportar archivos bancarios y legales.
- Presupuesto consume reales (`POST /presupuestos/{id}/actuals`) para comparar vs plan.

## 4. Portal Empleado y BFF
- Portal SPA consume BFF que agrega datos de Personal, Vacaciones, Tiempos, Liquidación, Reclamos, Beneficios, Capacitaciones, Planes de Carrera, etc.
- Nucleus WF provee bandeja de tareas (`GET /tasks`).
- Seguridad (OIDC + roles) y configuración centralizada determinan qué widgets/módulos se muestran.

## 5. Reclamos / Sanciones / Accidentabilidad / Medicina
- **Reclamos**: workflow de mesa omnicanal. Puede derivar en Sanciones o Accidentabilidad.
- **Sanciones**: recibe casos desde Reclamos o Personal y actualiza Legajos.
- **Accidentabilidad**: integra con Medicina y Tiempos (ausencias, licencias) y genera notificaciones a ART.
- **Medicina**: maneja exámenes/licencias; publica eventos `MedicalLicenseApproved` para Tiempos/Liquidación.
- **Resolución**: cada módulo expone su API y publica eventos; se establecen suscripciones cruzadas (por ejemplo, Sanciones escucha `IncidentReported` o `ReclamoResolved` con resultado disciplinario).

## 6. Selección / WebCV ↔ Personal / Carrera
- Selección genera conversaciones (candidatos, pipeline). Eventos `CandidateHired` → Personal crea legajo (con datos básicos) y Vacaciones/Tiempos pueden cálcular antigüedad.
- Carrera/Sucesión consume datos de Evaluación y Capacitaciones para actualizar planes y readiness.

## 7. Capacitaciones ↔ Beneficios ↔ Carrera
- Capacitaciones publica `CourseCompleted` → Carrera actualiza planes y Evaluación registra acciones correctivas.
- Beneficios y Capacitaciones comparten proveedores e integran pagos vía Tesorería y Liquidación.

## 8. Control de Visitas ↔ Seguridad ↔ Portal
- Control de Visitas produce `VisitCheckedIn/Out` → Seguridad consume para monitoreo.
- Seguridad genera alertas y permisos especiales; Portal Empleado muestra invitaciones y autorizaciones.

## 9. Analytics / Data Platform
- Todos los servicios publican eventos y/o exponen endpoints ETL-friendly. ETL/ELT pipelines consolidan datos en Data Lake/Warehouse.
- Analytics Service ofrece datasets y dashboards; Portal Analytics muestra insights según rol.

## 10. Configuración Avanzada
- Centraliza catálogos, parámetros, feature flags y conexiones. Cada servicio consume Config API/GitOps bundles.
- Cambios de configuración pasan por approvals y releases versionados.

## 11. Beneficios ↔ Tesorería ↔ Liquidación
- Beneficios monetarios generan ajustes enviados a Liquidación/Tesorería.
- Reembolsos aprobados se pagan via Tesorería y se reflejan como beneficios en Liquidación.

## 12. Carrera/Sucesión ↔ Evaluación ↔ Capacitaciones
- Evaluación aporta potencial/desempeño; Capacitaciones ofrece acciones de desarrollo; Carrera/Sucesión mantiene readiness y planes.

## 13. Integrations Hub
- Punto central para exportes/importes (bancos, AFIP, BI, proveedores). Recibe eventos `IntegrationJob*`.
- Plantillas YAML definen fuentes (APIs/SQL), transformaciones (Liquid/Mapping) y destinos (SFTP/API/Data Lake).

## 14. Nucleus WF
- Base para todos los workflows. Definition Service gestiona definiciones; Runtime Service ejecuta instancias/tareas y publica eventos `Workflow*` consumidos por Portal/BFF.

## 15. Seguridad de datos
- Todos los módulos siguen lineamientos: OIDC + roles, auditoría, cifrado (para salud/finanzas), cumplimiento (GDPR/ISO). Configuración Avanzada y Analytics mantienen catálogo y gobernanza.

---
Con estas interacciones registradas y los blueprints generados para cada módulo, la fase de descubrimiento queda cerrada y podemos avanzar con la construcción en el nuevo entorno.

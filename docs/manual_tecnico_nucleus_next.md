# Manual Tecnico - Nucleus RH Next

## Indice General
1. [Arquitectura General](#1-arquitectura-general)
2. [Servicios y Componentes](#2-servicios-y-componentes)
3. [Base de Datos por Servicio](#3-base-de-datos-por-servicio)
4. [API Endpoints por Modulo](#4-api-endpoints-por-modulo)
5. [Modelos y Entidades](#5-modelos-y-entidades)
6. [Workflows](#6-workflows)
7. [Integraciones](#7-integraciones)
8. [Configuracion](#8-configuracion)
9. [Autenticacion y Autorizacion](#9-autenticacion-y-autorizacion)
10. [Monitoreo y Observabilidad](#10-monitoreo-y-observabilidad)
11. [Desarrollo y Mantenimiento](#11-desarrollo-y-mantenimiento)
12. [Guia de Troubleshooting](#12-guia-de-troubleshooting)

---

## 1. Arquitectura General

### 1.1 Vision Global

Nucleus RH Next es un sistema de Gestion de Recursos Humanos basado en una arquitectura de microservicios. El sistema esta construido sobre:

| Capa | Tecnologia |
|------|------------|
| Runtime | .NET 8 |
| Framework | ASP.NET Core 8.0 (Minimal APIs) |
| ORM | Entity Framework Core 8.0 |
| Base de Datos | SQLite (desarrollo) / PostgreSQL (produccion) |
| Autenticacion | JWT (Json Web Tokens) |
| Contenedores | Docker + Docker Compose |
| Observabilidad | OpenTelemetry + Jaeger + Prometheus |

### 1.2 Diagrama de Servicios

```
                                    +------------------+
                                    |   Portal RH      |
                                    |  (React :3001)  |
                                    +--------+---------+
                                             |
                                    +--------v---------+
                                    |  Portal BFF     |
                                    |  (:5090)        |
                                    +--------+---------+
                           +---------+         +---------+
                    +------v-------+   +-------v-------+ |
                    | Auth Service |   | Workflow      | |
                    | (:5001)      |   | (:5051)      | |
                    +------+-------+   +--------------+ |
                           |                            |
        +------------------+------------------+-------+
        |                  |                  |
+-------v-----+  +--------v-----+  +--------v-----+
| Organizacion|  | Personal     |  | Liquidacion  |
| (:5100)     |  | (:5200)      |  | (:5188)      |
+-------------+  +--------------+  +--------------+
        |                  |                  |
        +------------------+------------------+
                           |
              +------------v-------------+
              | Integration Hub (:5050)   |
              +------------+-------------+
                           |
              +------------v-------------+
              | Tiempos Service (:5400)  |
              +--------------------------+
```

### 1.3 Puertos de Servicios

| Servicio | Puerto | Puerto (Docker) | Descripcion |
|----------|--------|-----------------|-------------|
| auth-service | 5001 | 5001 | Autenticacion JWT |
| organizacion-service | 5100 | 5100 | Empresas, unidades, posiciones |
| personal-service | 5200 | 5200 | Legajos y documentos |
| liquidacion-service | 5188 | 5188 | Nomina y recibos |
| nucleuswf-service | 5051 | 5051 | Motor de workflows |
| integration-hub-service | 5050 | 5050 | Integraciones externas |
| portal-bff-service | 5090 | 5090 | API Gateway unificado |
| configuracion-service | 5300 | 5300 | Catalogos y parametros |
| tiempos-service | 5400 | 5400 | Control de asistencia |
| portal-rh-ui | 3001 | 3001 | UI Administracion |
| portal-empleado-ui | 3002 | 3002 | UI Autoservicio |
| Jaeger | 16686 | 16686 | Trazas |
| Prometheus | 9090 | 9090 | Metricas |

---

## 2. Servicios y Componentes

### 2.1 auth-service

**Proposito**: Manejo de autenticacion y autorizacion mediante JWT.

**Caracteristicas**:
- Generacion de tokens JWT
- Validacion de credenciales
- Gestion de usuarios y roles
- Middleware de autenticacion

**Tecnologias**:
- ASP.NET Core 8.0
- Entity Framework Core + SQLite
- System.IdentityModel.Tokens.Jwt

**Archivos Clave**:
```
auth-service/
├── Program.cs                 # Configuracion y endpoints
├── Infrastructure/
│   └── AuthDbContext.cs       # Contexto de DB
├── Domain/
│   └── Models/
│       └── User.cs            # Entidad usuario
├── Services/
│   └── AuthService.cs        # Logica de auth
└── appsettings.json          # Configuracion
```

### 2.2 organizacion-service

**Proposito**: Gestion de la estructura organizacional.

**Caracteristicas**:
- CRUD de empresas
- Gestion de unidades organizativas (jerarquia)
- Definicion de posiciones
- Centros de costo
- sindicatos y convenios colectivos

### 2.3 personal-service

**Proposito**: Gestion integral de legajos de empleados.

**Caracteristicas**:
- Alta, modificacion y baja de legajos
- Gestion de documentos
- Domicilios y datos de contacto
- Familiares y cargas familiares

### 2.4 liquidacion-service

**Proposito**: Procesamiento de nomina y generacion de recibos.

**Caracteristicas**:
- Creacion de ciclos de liquidacion
- Asignacion de legajos a liquidaciones
- Calculo automatico de conceptos
- Generacion de recibos detallados
- Exportacion de archivos

### 2.5 nucleuswf-service

**Proposito**: Motor de workflows para procesos de RRHH.

**Caracteristicas**:
- Definicion de procesos
- Ejecucion de instancias
- Transiciones de estados
- Auditoria de movimientos

### 2.6 integration-hub-service

**Proposito**: Integracion con sistemas externos.

**Caracteristicas**:
- Templates de integracion
- Ejecucion de jobs
- Triggers programables
- Logging de ejecuciones

### 2.7 tiempos-service

**Proposito**: Control de asistencia y tiempos trabajados.

**Caracteristicas**:
- Registro de fichadas
- Gestion de turnos y horarios
- Control de ausencias
- Generacion de planillas

### 2.8 portal-bff-service

**Proposito**: API Gateway que unifica el acceso a todos los servicios.

**Caracteristicas**:
- Proxy de servicios internos
- Transformacion de respuestas
- Manejo de notificaciones
- Dashboard unificado
- Health checks

### 2.9 configuracion-service

**Proposito**: Gestion centralizada de parametros y catalogos.

**Caracteristicas**:
- Catalogos configurable
- Parametros del sistema
- Versionado de configuracion

---

## 3. Base de Datos por Servicio

### 3.1 auth-service

**Archivo**: `auth.db` (SQLite)

#### Tabla: Users

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | GUID | Identificador unico |
| Username | TEXT | Nombre de usuario (unico) |
| PasswordHash | TEXT | Hash de contrasena |
| Salt | TEXT | Salt para hash |
| Role | TEXT | Rol del usuario (Admin, User, etc.) |
| Estado | TEXT | Estado (Activo/Inactivo) |
| CreatedAt | DATETIME | Fecha de creacion |
| UpdatedAt | DATETIME | Fecha de actualizacion |

**Indices**:
- Username (unico)

### 3.2 organizacion-service

**Archivo**: `organizacion.db` (SQLite)

#### Tabla: Empresas

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | GUID | Identificador |
| Nombre | TEXT | Razon social |
| CUIL | TEXT | CUIL empresa |
| Direccion | TEXT | Domicilio |
| Telefono | TEXT | Contacto |
| Email | TEXT | Correo electronico |
| Estado | TEXT | Activo/Inactivo |
| CreatedAt | DATETIME | Fecha creacion |
| UpdatedAt | DATETIME | Fecha actualizacion |

#### Tabla: OrgUnits (Unidades Organizativas)

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | GUID | Identificador |
| Nombre | TEXT | Nombre unidad |
| EmpresaId | GUID | FK a Empresa |
| PadreId | GUID? | FK a OrgUnit padre |
| Tipo | TEXT | Tipo (Gerencia, Depto, Area, etc.) |
| Codigo | TEXT | Codigo interno |
| Estado | TEXT | Activo/Inactivo |

#### Tabla: Posiciones

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | GUID | Identificador |
| Nombre | TEXT | Nombre del cargo |
| OrgUnitId | GUID | FK a OrgUnit |
| Descripcion | TEXT | Descripcion |
| SalarioMin | DECIMAL | Salario minimo |
| SalarioMax | DECIMAL | Salario maximo |
| Estado | TEXT | Activo/Inactivo |

#### Tabla: CostCenters (Centros de Costo)

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | GUID | Identificador |
| Codigo | TEXT | Codigo |
| Nombre | TEXT | Nombre |
| EmpresaId | GUID | FK a Empresa |
| Presupuesto | DECIMAL | Presupuesto asignado |

### 3.3 personal-service

**Archivo**: `personal.db` (SQLite)

#### Tabla: Legajos

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | GUID | Identificador |
| Numero | TEXT | Numero de legajo |
| Nombre | TEXT | Nombre |
| Apellido | TEXT | Apellido |
| ApellidoMaterno | TEXT | Apellido materno |
| DocumentoTipoId | GUID | FK a tipo documento |
| DocumentoNumero | TEXT | Numero documento |
| CUIL | TEXT | CUIL |
| Sexo | TEXT | Genero |
| FechaNacimiento | DATE | Fecha nacimiento |
| Email | TEXT | Correo electronico |
| Telefono | TEXT | Telefono |
| EmpresaId | GUID | FK a Empresa |
| OrgUnitId | GUID | FK a OrgUnit |
| PositionId | GUID | FK a Posicion |
| FechaIngreso | DATE | Fecha de ingreso |
| Estado | TEXT | Activo/Licencia/Baja |
| CreatedAt | DATETIME | Fecha creacion |
| UpdatedAt | DATETIME | Fecha actualizacion |

#### Tabla: LegajosDocumentos

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | GUID | Identificador |
| LegajoId | GUID | FK a Legajo |
| TipoDocumentoId | GUID | FK a tipo |
| Numero | TEXT | Numero documento |
| FechaEmision | DATE | Fecha emision |
| FechaVencimiento | DATE | Fecha vencimiento |
| ArchivoPath | TEXT | Path archivo |

#### Tabla: LegajosDomicilios

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | GUID | Identificador |
| LegajoId | GUID | FK a Legajo |
| Tipo | TEXT | Particular/Laboral |
| Calle | TEXT | Calle |
| Numero | TEXT | Numero |
| Piso | TEXT | Piso |
| Departamento | TEXT | Depto |
| Localidad | TEXT | Localidad |
| Provincia | TEXT | Provincia |
| CodigoPostal | TEXT | CP |
| Pais | TEXT | Pais |

#### Tabla: LegajosFamiliares

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | GUID | Identificador |
| LegajoId | GUID | FK a Legajo |
| Nombre | TEXT | Nombre familiar |
| Apellido | TEXT | Apellido |
| Parentesco | TEXT | Relacion |
| FechaNacimiento | DATE | Fecha nacimiento |
| DocumentoNumero | TEXT | Numero documento |
| CUIL | TEXT | CUIL |
| Discapacidad | BOOL | Tiene discapacidad |

### 3.4 liquidacion-service

**Archivo**: `liquidacion.db` (SQLite)

#### Tabla: PayrollRuns

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | GUID | Identificador |
| Periodo | TEXT | Periodo (YYYY-MM) |
| Tipo | TEXT | Mensual/Quincenal/Semanal |
| Estado | TEXT | Borrador/EnProceso/Cerrado |
| Descripcion | TEXT | Descripcion |
| FechaCierre | DATETIME? | Fecha cierre |
| TotalRemunerativo | DECIMAL | Total remunerativo |
| TotalDeducciones | DECIMAL | Total deducciones |
| TotalNeto | DECIMAL | Total neto |
| CreatedAt | DATETIME | Fecha creacion |
| UpdatedAt | DATETIME | Fecha actualizacion |

#### Tabla: PayrollLegajos

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | GUID | Identificador |
| PayrollRunId | GUID | FK a PayrollRun |
| LegajoId | GUID | FK a Legajo |
| Basico | DECIMAL | Salario basico |
| Antiguedad | DECIMAL | Antiguedad |
| Adicionales | DECIMAL | Adicionales |
| Bonificaciones | DECIMAL | Bonificaciones |
| Descuentos | DECIMAL | Descuentos |
| Estado | TEXT | Pendiente/Procesado/Error |

#### Tabla: PayrollReceipts

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | GUID | Identificador |
| PayrollRunId | GUID | FK a PayrollRun |
| LegajoId | GUID | FK a Legajo |
| Remunerativo | DECIMAL | Total remunerativo |
| Deducciones | DECIMAL | Total deducciones |
| Neto | DECIMAL | Neto a cobrar |
| FechaPago | DATE | Fecha pago |
| Estado | TEXT | Pendiente/Pagado |

#### Tabla: PayrollReceiptDetails

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | GUID | Identificador |
| ReceiptId | GUID | FK a Receipt |
| Concepto | TEXT | Nombre concepto |
| Codigo | TEXT | Codigo concepto |
| Importe | DECIMAL | Importe |
| Tipo | TEXT | Remunerativo/Deduccion |

### 3.5 nucleuswf-service

**Archivo**: `nucleuswf.db` (SQLite)

#### Tabla: WorkflowDefinitions

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | GUID | Identificador |
| Nombre | TEXT | Nombre proceso |
| Descripcion | TEXT | Descripcion |
| Version | INT | Version |
| Definicion | TEXT JSON | Definicion estados y transiciones |
| Estado | TEXT | Activo/Inactivo |
| CreatedAt | DATETIME | Fecha creacion |
| UpdatedAt | DATETIME | Fecha actualizacion |

#### Tabla: WorkflowInstances

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | GUID | Identificador |
| DefinitionId | GUID | FK a Definition |
| LegajoId | GUID? | FK a Legajo |
| Estado | TEXT | Estado actual |
| Data | TEXT JSON | Datos de la instancia |
| StartedAt | DATETIME | Inicio |
| FinishedAt | DATETIME? | Fin |
| CreatedAt | DATETIME | Creacion |

#### Tabla: WorkflowTasks

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | GUID | Identificador |
| InstanceId | GUID | FK a Instance |
| Tipo | TEXT | Tipo tarea |
| AsignadoA | TEXT | Usuario asignado |
| Estado | TEXT | Pendiente/Completada |
| Data | TEXT JSON | Datos tarea |
| CompletedAt | DATETIME? | Fecha completada |

#### Tabla: WorkflowLogs

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | GUID | Identificador |
| InstanceId | GUID | FK a Instance |
| FromEstado | TEXT | Estado anterior |
| ToEstado | TEXT | Estado nuevo |
| Accion | TEXT | Accion ejecutada |
| Usuario | TEXT | Usuario que ejecuto |
| Timestamp | DATETIME | Fecha |

### 3.6 integration-hub-service

**Archivo**: `integrationhub.db` (SQLite)

#### Tabla: IntegrationTemplates

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | GUID | Identificador |
| Nombre | TEXT | Nombre template |
| Tipo | TEXT | Tipo (AFIP, Banco, etc.) |
| Version | TEXT | Version |
| Config | TEXT JSON | Configuracion |
| CreatedAt | DATETIME | Creacion |
| UpdatedAt | DATETIME | Actualizacion |

#### Tabla: IntegrationJobs

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | GUID | Identificador |
| TemplateId | GUID | FK a Template |
| Estado | TEXT | Pendiente/Ejecutando/Completado/Error |
| Input | TEXT JSON | Datos entrada |
| Output | TEXT JSON | Datos salida |
| Log | TEXT | Log ejecucion |
| Errores | TEXT | Errores |
| StartedAt | DATETIME | Inicio |
| FinishedAt | DATETIME? | Fin |

#### Tabla: IntegrationTriggers

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | GUID | Identificador |
| Nombre | TEXT | Nombre trigger |
| EventName | TEXT | Evento que dispara |
| TemplateId | GUID | FK a Template |
| Config | TEXT JSON | Configuracion |
| Estado | TEXT | Activo/Inactivo |

### 3.7 tiempos-service

**Archivo**: `tiempos.db` (SQLite)

#### Tabla: Turnos

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | GUID | Identificador |
| Nombre | TEXT | Nombre turno |
| HoraEntrada | TIME | Hora entrada |
| HoraSalida | TIME | Hora salida |
| ToleranciaMinutos | INT | Minutos tolerancia |
| Estado | TEXT | Activo/Inactivo |

#### Tabla: Horarios

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | GUID | Identificador |
| Nombre | TEXT | Nombre horario |
| TurnoId | GUID | FK a Turno |
| Lunes | BOOL | Trabaja lunes |
| Martes | BOOL | Trabaja martes |
| Miercoles | BOOL | Trabaja miercoles |
| Jueves | BOOL | Trabaja jueves |
| Viernes | BOOL | Trabaja viernes |
| Sabado | BOOL | Trabaja sabado |
| Domingo | BOOL | Trabaja domingo |

#### Tabla: Fichadas

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | GUID | Identificador |
| LegajoId | GUID | FK a Legajo |
| FechaHora | DATETIME | Fecha y hora |
| Tipo | TEXT | Entrada/Salida |
| Fuente | TEXT | Manual/Terminal/API |
| TerminalId | TEXT | ID terminal |

#### Tabla: Ausencias

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | GUID | Identificador |
| LegajoId | GUID | FK a Legajo |
| Tipo | TEXT | Tipo ausencia |
| FechaDesde | DATE | Inicio |
| FechaHasta | DATE | Fin |
| Dias | DECIMAL | Dias solicitados |
| Motivo | TEXT | Justificacion |
| Estado | TEXT | Pendiente/Aprobada/Rechazada |
| AprobadoPor | TEXT | Usuario aprobo |
| WorkflowInstanceId | GUID? | FK a Workflow |

#### Tabla: Planillas

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | GUID | Identificador |
| Nombre | TEXT | Nombre planilla |
| Periodo | TEXT | Periodo |
| FechaDesde | DATE | Inicio periodo |
| FechaHasta | DATE | Fin periodo |
| Estado | TEXT | Abierta/Cerrada |
| TotalHoras | DECIMAL | Horas totales |
| TotalExtras | DECIMAL | Horas extras |

### 3.8 portal-bff-service

**Archivo**: `portalbff.db` (SQLite)

#### Tabla: Notificaciones

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | GUID | Identificador |
| UsuarioId | GUID | FK a Usuario |
| Titulo | TEXT | Titulo |
| Mensaje | TEXT | Mensaje |
| Tipo | TEXT | Tipo notificacion |
| Leida | BOOL | Leida o no |
| FechaCreacion | DATETIME | Creacion |
| FechaLectura | DATETIME? | Lectura |

---

## 4. API Endpoints por Modulo

### 4.1 Autenticacion

```
POST   /login                    # Login usuario
GET    /me                       # Info usuario actual
GET    /health                  # Health check
POST   /users                   # Crear usuario (Admin)
GET    /users                   # Listar usuarios
GET    /users/{id}              # Ver usuario
PUT    /users/{id}              # Actualizar usuario
DELETE /users/{id}              # Eliminar usuario
```

### 4.2 Organizacion

```
GET    /api/rh/v1/organizacion/empresas
POST   /api/rh/v1/organizacion/empresas
PUT    /api/rh/v1/organizacion/empresas/{id}
DELETE /api/rh/v1/organizacion/empresas/{id}

GET    /api/rh/v1/organizacion/unidades
POST   /api/rh/v1/organizacion/unidades
PUT    /api/rh/v1/organizacion/unidades/{id}
DELETE /api/rh/v1/organizacion/unidades/{id}

GET    /api/rh/v1/organizacion/posiciones
POST   /api/rh/v1/organizacion/posiciones
PUT    /api/rh/v1/organizacion/posiciones/{id}
DELETE /api/rh/v1/organizacion/posiciones/{id}
POST   /api/rh/v1/organizacion/posiciones/{id}/asignar
```

### 4.3 Personal

```
GET    /api/rh/v1/personal/legajos
POST   /api/rh/v1/personal/legajos
PUT    /api/rh/v1/personal/legajos/{id}
DELETE /api/rh/v1/personal/legajos/{id}
GET    /api/rh/v1/personal/legajos/{id}/documentos
GET    /api/rh/v1/personal/legajos/{id}/domicilios
```

### 4.4 Liquidacion

```
GET    /api/rh/v1/liquidacion/payrolls
POST   /api/rh/v1/liquidacion/payrolls
PATCH  /api/rh/v1/liquidacion/payrolls/{id}
POST   /api/rh/v1/liquidacion/payrolls/{id}/legajos
POST   /api/rh/v1/liquidacion/payrolls/{id}/procesar
GET    /api/rh/v1/liquidacion/payrolls/{id}/recibos
GET    /api/rh/v1/liquidacion/payrolls/{id}/exports
GET    /api/rh/v1/liquidacion/exports/{fileName}
```

### 4.5 Tiempos

```
GET    /api/rh/v1/tiempos/turnos
POST   /api/rh/v1/tiempos/turnos
GET    /api/rh/v1/tiempos/horarios
POST   /api/rh/v1/tiempos/horarios
GET    /api/rh/v1/tiempos/fichadas
POST   /api/rh/v1/tiempos/fichadas
GET    /api/rh/v1/tiempos/ausencias
POST   /api/rh/v1/tiempos/ausencias
GET    /api/rh/v1/tiempos/planillas
POST   /api/rh/v1/tiempos/planillas
```

### 4.6 Workflows

```
GET    /api/rh/v1/wf/definitions
POST   /api/rh/v1/wf/definitions
GET    /api/rh/v1/wf/instances
POST   /api/rh/v1/wf/instances
POST   /api/rh/v1/wf/instances/{id}/transitions
GET    /api/rh/v1/wf/instances/{id}
```

### 4.7 Integraciones

```
GET    /api/rh/v1/integraciones/templates
GET    /api/rh/v1/integraciones/jobs
GET    /api/rh/v1/integraciones/jobs/{id}
POST   /api/rh/v1/integraciones/jobs
POST   /api/rh/v1/integraciones/jobs/{id}/retry
GET    /api/rh/v1/integraciones/eventos
POST   /api/rh/v1/integraciones/triggers/{nombre}/execute
```

### 4.8 Seleccion

```
GET    /api/rh/v1/seleccion/candidates
POST   /api/rh/v1/seleccion/candidates
GET    /api/rh/v1/seleccion/candidates/{id}
PUT    /api/rh/v1/seleccion/candidates/{id}/estado
GET    /api/rh/v1/seleccion/avisos
POST   /api/rh/v1/seleccion/avisos
```

### 4.9 Evaluacion

```
GET    /api/rh/v1/evaluacion/evaluaciones
POST   /api/rh/v1/evaluacion/evaluaciones
GET    /api/rh/v1/evaluacion/evaluaciones/{id}
POST   /api/rh/v1/evaluacion/evaluaciones/{id}/responder
GET    /api/rh/v1/evaluacion/metas
```

### 4.10 Capacitacion

```
GET    /api/rh/v1/capacitacion/cursos
GET    /api/rh/v1/capacitacion/cursos/{id}
GET    /api/rh/v1/capacitacion/inscripciones
POST   /api/rh/v1/capacitacion/inscripciones
```

### 4.11 Clima Laboral

```
GET    /api/rh/v1/clima/encuestas
POST   /api/rh/v1/clima/encuestas
GET    /api/rh/v1/clima/encuestas/{id}
POST   /api/rh/v1/clima/encuestas/{id}/responder
GET    /api/rh/v1/clima/resultados
```

### 4.12 Carrera

```
GET    /api/rh/v1/carrera/planes
POST   /api/rh/v1/carrera/planes
GET    /api/rh/carrera/succession
```

### 4.13 Seguridad

```
GET    /api/rh/v1/auditoria/logs
GET    /api/rh/v1/auditoria/logs/{id}
GET    /api/rh/v1/seguridad/roles
POST   /api/rh/v1/seguridad/roles
GET    /api/rh/v1/seguridad/roles/{id}
PUT    /api/rh/v1/seguridad/roles/{id}
GET    /api/rh/v1/seguridad/usuarios
POST   /api/rh/v1/seguridad/usuarios
POST   /api/rh/v1/seguridad/usuarios/{id}/reset-password
GET    /api/rh/v1/seguridad/incidentes
POST   /api/rh/v1/seguridad/incidentes
GET    /api/rh/v1/seguridad/compliance
```

### 4.14 Sistema

```
GET    /api/rh/v1/sistema/health
GET    /api/rh/v1/sistema/metricas
GET    /api/rh/v1/sistema/integraciones
POST   /api/rh/v1/sistema/integraciones/{nombre}/sync
GET    /api/rh/v1/sistema/info
GET    /api/rh/v1/sistema/configuracion
PUT    /api/rh/v1/sistema/configuracion/{clave}
GET    /api/rh/v1/sistema/backup
POST   /api/rh/v1/sistema/backup
POST   /api/rh/v1/sistema/backup/{id}/restore
GET    /api/rh/v1/sistema/mantenimiento
POST   /api/rh/v1/sistema/mantenimiento
```

### 4.15 Configuracion

```
GET    /api/rh/v1/configuracion/catalogos/{tipo}
POST   /api/rh/v1/configuracion/catalogos
GET    /api/rh/v1/configuracion/parametros
```

### 4.16 Dashboard

```
GET    /api/rh/v1/dashboard/resumen
GET    /api/rh/v1/analytics/rrhh
POST   /api/rh/v1/analytics/rrhh/{id}/generar
GET    /api/rh/v1/analytics/indicadores
GET    /api/rh/v1/analytics/areas/{area}
GET    /api/rh/v1/analytics/tendencias
```

---

## 5. Modelos y Entidades

### 5.1 Entidades Core

#### Legajo (Personal)
```csharp
public class Legajo
{
    public Guid Id { get; set; }
    public string Numero { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string DocumentoTipoId { get; set; }
    public string DocumentoNumero { get; set; }
    public string CUIL { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid OrgUnitId { get; set; }
    public Guid PositionId { get; set; }
    public DateTime FechaIngreso { get; set; }
    public string Estado { get; set; } // Activo, Inactivo, Licencia, Baja
}
```

#### PayrollRun (Liquidacion)
```csharp
public class PayrollRun
{
    public Guid Id { get; set; }
    public string Periodo { get; set; } // YYYY-MM
    public string Tipo { get; set; } // Mensual, Quincenal
    public string Estado { get; set; } // Borrador, EnProceso, Cerrado
    public decimal TotalRemunerativo { get; set; }
    public decimal TotalDeducciones { get; set; }
    public decimal TotalNeto { get; set; }
    public DateTime? FechaCierre { get; set; }
}
```

#### WorkflowDefinition
```csharp
public class WorkflowDefinition
{
    public Guid Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public int Version { get; set; }
    public string Definicion { get; set; } // JSON: { estados: [], transiciones: [] }
    public string Estado { get; set; } // Activo, Inactivo
}
```

### 5.2 Enums Comunes

```csharp
public enum EstadoLegajo
{
    Activo,
    Inactivo,
    Licencia,
    Baja
}

public enum EstadoPayroll
{
    Borrador,
    EnProceso,
    Cerrado,
    Cancelado
}

public enum TipoFichada
{
    Entrada,
    Salida
}

public enum TipoAusencia
{
    Vacaciones,
    LicenciaMedica,
    LicenciaFamiliar,
    Permiso,
    Ausencia
}
```

---

## 6. Workflows

### 6.1 Estructura de Definicion

Los workflows se definen en JSON con la siguiente estructura:

```json
{
  "estados": [
    { "id": "borrador", "nombre": "Borrador", "tipo": "inicial" },
    { "id": "pendiente_aprobacion", "nombre": "Pendiente Aprobacion", "tipo": "intermedio" },
    { "id": "aprobado", "nombre": "Aprobado", "tipo": "final" },
    { "id": "rechazado", "nombre": "Rechazado", "tipo": "final" }
  ],
  "transiciones": [
    { "desde": "borrador", "hacia": "pendiente_aprobacion", "accion": "enviar", "roles": ["empleado"] },
    { "desde": "pendiente_aprobacion", "hacia": "aprobado", "accion": "aprobar", "roles": ["jefe", "rrhh"] },
    { "desde": "pendiente_aprobacion", "hacia": "rechazado", "accion": "rechazar", "roles": ["jefe", "rrhh"] }
  ]
}
```

### 6.2 Workflows Incluidos

| Nombre | Descripcion | Estados |
|--------|-------------|---------|
| Vacaciones | Solicitud de vacaciones | Borrador -> Pendiente -> Aprobado/Rechazado |
| Datos Personales | Modificacion de datos | Borrador -> Pendiente -> Aprobado/Rechazado |
| Reclamo | Gestion de reclamos | Nuevo -> EnAnalisis -> Resuelto -> Cerrado |
| Anticipo | Solicitud de anticipo | Borrador -> Pendiente -> Aprobado/Rechazado |

### 6.3 API de Workflows

```csharp
// Crear instancia
POST /api/rh/v1/wf/instances
{
  "definitionId": "vacaciones",
  "legajoId": "guid",
  "data": { "fechaDesde": "...", "dias": 5 }
}

// Transicionar
POST /api/rh/v1/wf/instances/{id}/transitions
{
  "action": "aprobar",
  "comentario": "Aprobado"
}

// Consultar
GET /api/rh/v1/wf/instances/{id}
```

---

## 7. Integraciones

### 7.1 Integration Hub

El Integration Hub permite ejecutar integraciones con sistemas externos de forma asincronica.

#### Arquitectura

```
IntegrationHub
    |
    +-- Templates (definiciones de integracion)
    +-- Jobs (ejecuciones)
    +-- Triggers (disparadores)
    +-- Connections (destinos)
```

#### Tipos de Integracion Soportados

| Tipo | Sistema | Protocolo |
|------|---------|-----------|
| AFIP | Administracion Federal de Ingresos Publicos | REST/SOAP |
| Banco | Entidades bancarias | SFTP, REST |
| SSN | Sistema Nacional de Salud | REST |
| Proveedor | Proveedores de servicios | REST |

#### Ejemplo de Template

```json
{
  "nombre": "Exportar Liquidacion a Banco",
  "tipo": "Banco",
  "version": "1.0",
  "config": {
    "endpoint": "https://banco.example.com/api/liquidaciones",
    "metodo": "POST",
    "formato": "JSON",
    "headers": {
      "Authorization": "Bearer {token}"
    }
  }
}
```

#### Ejecucion de Job

```csharp
// Crear job
POST /api/rh/v1/integraciones/jobs
{
  "templateId": "guid",
  "input": {
    "payrollRunId": "guid",
    "cuentas": [...]
  }
}

// Reintentar
POST /api/rh/v1/integraciones/jobs/{id}/retry
```

### 7.2 Triggers

Los triggers permiten ejecutar integraciones automaticamente segun eventos:

```json
{
  "nombre": "Notificar Liquidacion",
  "eventName": "liquidacion.cerrada",
  "templateId": "guid",
  "config": {
    "async": true
  },
  "estado": "Activo"
}
```

---

## 8. Configuracion

### 8.1 Archivos de Configuracion

Cada servicio tiene su propio `appsettings.json`:

```json
{
  "Auth": {
    "Issuer": "nucleus-auth",
    "Audience": "nucleus-api",
    "SigningKey": "clave_secreta_32_caracteres!!"
  },
  "Database": {
    "Provider": "sqlite",
    "ConnectionStrings": {
      "DefaultConnection": "Data Source=archivodb.db"
    }
  },
  "OpenTelemetry": {
    "Enabled": true,
    "ServiceName": "nombre-servicio"
  },
  "Cors": {
    "Origins": ["http://localhost:3001", "http://localhost:3002"]
  }
}
```

### 8.2 Variables de Entorno

| Variable | Descripcion | Default |
|----------|-------------|---------|
| ASPNETCORE_ENVIRONMENT | Desarrollo/Produccion | Development |
| ASPNETCORE_URLS | URLs servicio | http://0.0.0.0:puerto |
| AUTH_ISSUER | Emisor JWT | nucleus-auth |
| AUTH_AUDIENCE | Audiencia JWT | nucleus-api |
| AUTH_SIGNING_KEY | Clave firma JWT | - |
| OTEL_ENABLED | Habilitar OTEL | false |
| OTEL_EXPORTER_OTLP_ENDPOINT | Endpoint collector | - |

### 8.3 Docker Compose

```yaml
services:
  auth-service:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - AUTH_ISSUER=${AUTH_ISSUER}
      - AUTH_SIGNING_KEY=${AUTH_SIGNING_KEY}
      - ConnectionStrings__AuthDb=${AUTH_DB_CONNECTION}
      - OTEL_ENABLED=${OTEL_ENABLED}
    ports:
      - "5001:5001"
```

---

## 9. Autenticacion y Autorizacion

### 9.1 Flujo JWT

1. **Login**: POST /login con credenciales
2. **Respuesta**: { token: "eyJ...", expiresIn: 86400 }
3. **Uso**: Header Authorization: Bearer {token}
4. **Validacion**: Middleware valida token en cada request

### 9.2 Claims y Roles

```csharp
// Claims del token
{
  "sub": "user-id",
  "name": "Juan Perez",
  "role": "Admin",
  "exp": 1234567890
}
```

### 9.3 Roles Soportados

| Rol | Permisos |
|-----|----------|
| Admin | Total |
| RRHH | Gestion RRHH |
| Jefe | Aprobaciones equipo |
| Empleado | Consulta propia |

### 9.4 Configuracion de Auth

```csharp
// Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Auth:Issuer"],
            ValidAudience = builder.Configuration["Auth:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Auth:SigningKey"])
            )
        };
    });
```

---

## 10. Monitoreo y Observabilidad

### 10.1 OpenTelemetry

Cada servicio esta instrumentado con OpenTelemetry:

```csharp
// Configuracion
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource(serviceName)
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(otelEndpoint);
        }));
```

### 10.2 Jaeger

- **URL**: http://localhost:16686
- **Puerto**: 4317 (gRPC), 4318 (HTTP)

### 10.3 Prometheus

- **URL**: http://localhost:9090
- **Puerto**: 8889 (metricas)

### 10.4 Health Checks

Cada servicio expone endpoint de salud:

```
GET /health
```

Respuesta:
```json
{
  "status": "Healthy",
  "timestamp": "2026-03-14T10:00:00Z",
  "checks": {
    "database": "Healthy",
    "external_services": "Healthy"
  }
}
```

---

## 11. Desarrollo y Mantenimiento

### 11.1 Estructura de Proyecto

```
servicio/
├── Domain/
│   └── Models/           # Entidades
├── Infrastructure/
│   └── DbContext.cs     # Contexto EF
├── Services/             # Logica de negocio
├── Requests/             # DTOs request
├── Responses/            # DTOs response
├── Program.cs           # Endpoints
└── appsettings.json     # Configuracion
```

### 11.2 Comandos Utiles

```bash
# Restaurar dependencias
dotnet restore

# Compilar
dotnet build

# Ejecutar tests
dotnet test

# Crear migracion
dotnet ef migrations add NombreMigracion

# Aplicar migraciones
dotnet ef database update

# Ejecutar servicio
dotnet run --project servicio.csproj
```

### 11.3 Agregar Nuevo Servicio

1. Crear proyecto .NET 8
2. Agregar paquetes NuGet necesarios
3. Crear DbContext y entidades
4. Configurar Program.cs con endpoints
5. Agregar al docker-compose.yml
6. Agregar al portal-bff-service

### 11.4 Agregar Nuevo Endpoint

```csharp
// En Program.cs del servicio
app.MapGet("/api/recurso", async (HttpContext context) =>
{
    // Logica
    return Results.Ok(new { data = "..." });
});
```

### 11.5 Base de Datos

**Para SQLite (Desarrollo)**:
```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=servicio.db"
}
```

**Para PostgreSQL (Produccion)**:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=host;Database=nucleus_servicio;User=usr;Password=pwd"
}
```

---

## 12. Guia de Troubleshooting

### 12.1 Problemas Comunes

| Problema | Causa | Solucion |
|----------|-------|----------|
| 401 Unauthorized | Token expirado o mal formado | Regenerar token con /login |
| 403 Forbidden | Rol insuficiente | Verificar permisos |
| CORS Error | Origin no permitido | Configurar CORS_ORIGINS |
| Connection Refused | Servicio no levantado | Verificar docker compose |
| Database Locked | SQLite concurrencia | Usar PostgreSQL en produccion |
| No trazas Jaeger | OTEL deshabilitado | Verificar OTEL_ENABLED=true |

### 12.2 Logs

```bash
# Ver logs de un servicio
docker compose logs auth-service

# Logs en tiempo real
docker compose logs -f auth-service

# Ultimas 100 lineas
docker compose logs --tail=100 auth-service
```

### 12.3 Validar Salud

```bash
# Health checks
curl http://localhost:5001/health
curl http://localhost:5100/health
curl http://localhost:5090/health
```

### 12.4 Base de Datos

```bash
# Conectar a SQLite
sqlite3 archivo.db

# Ver tablas
sqlite> .tables

# Ver estructura
sqlite> .schema tabla

# Consultas
sqlite> SELECT * FROM users LIMIT 5;
```

---

## Anexo: Referencias

- **Documentacion**: `docs/`
- **API Docs**: Swagger en cada servicio (desarrollo)
- **Runbooks**: `docs/operacion/runbooks.md`
- **Casos de Uso**: `docs/casos_de_uso/`
- **Casos de Prueba**: `docs/casos_de_prueba/`
- **Arquitectura**: `docs/modulo-*/arquitectura.md`

---

*Documento generado automaticamente - Nucleus RH Next*
*Ultima actualizacion: 2026-03-14*

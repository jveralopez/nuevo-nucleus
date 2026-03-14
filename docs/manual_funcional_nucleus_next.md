# Manual Funcional - Nucleus RH Next

## Indice
1. [Introduccion](#1-introduccion)
2. [Arquitectura del Sistema](#2-arquitectura-del-sistema)
3. [Roles y Permisos](#3-roles-y-permisos)
4. [Configuracion Inicial](#4-configuracion-inicial)
5. [Modulos del Sistema](#5-modulos-del-sistema)
6. [Portales](#6-portales)
7. [APIs y Endpoints](#7-apis-y-endpoints)
8. [Integraciones](#8-integraciones)
9. [Workflows](#9-workflows)
10. [Configuracion de Variables de Entorno](#10-configuracion-de-variables-de-entorno)
11. [Monitoreo y Observabilidad](#11-monitoreo-y-observabilidad)
12. [Guia de Operacion](#12-guia-de-operacion)

---

## 1. Introduccion

Nucleus RH Next es un sistema de Gestion de Recursos Humanos desarrollado sobre una arquitectura de microservicios moderna. El sistema permite gestionar todos los procesos de RRHH de una organizacion, desde la contratacion hasta la liquidacion de haberes, pasando por control de asistencia, evaluaciones de desempeño, capacitación y más.

### 1.1 Características Principales
- Arquitectura de microservicios escalable
- Autenticación JWT
- Portal RH para administradores
- Portal Empleado paraautoservicio
- Workflow engine integrado
- Integraciones con sistemas externos
- Observabilidad completa

---

## 2. Arquitectura del Sistema

### 2.1 Componentes

| Servicio | Puerto | Descripcion |
|----------|--------|-------------|
| auth-service | 5001 | Autenticacion y autorizacion JWT |
| organizacion-service | 5100 | Gestion de empresas, unidades, posiciones |
| personal-service | 5200 | Gestion de legajos y documentos |
| liquidacion-service | 5188 | Nomina, payroll, recibos |
| nucleuswf-service | 5051 | Motor de workflows |
| integration-hub-service | 5050 | Integraciones externas |
| portal-bff-service | 5090 | API Gateway unificado |
| configuracion-service | 5300 | Catalogos y parametros |
| tiempos-service | 5400 | Control de asistencia |

### 2.2 Tecnologías
- **Backend**: .NET 8, ASP.NET Core
- **Base de datos**: SQLite (desarrollo) / PostgreSQL (produccion)
- **Frontend**: React + TypeScript
- **Contenedores**: Docker, Docker Compose
- **Monitoreo**: OpenTelemetry, Jaeger, Prometheus

---

## 3. Roles y Permisos

### 3.1 Roles del Sistema

| Rol | Descripcion | Permisos |
|-----|-------------|----------|
| **Admin** | Administrador del sistema | Acceso total |
| **RRHH** | Gestor de RRHH | Gestion de legajos, liquidaciones, reportes |
| **Jefe** | Responsable de area | Aprobacion de solicitudes de su equipo |
| **Empleado** | Usuario basico | Consulta de recibos, solicitudes propias |

### 3.2 Credenciales Demo
```
Usuario: admin
Password: admin123

Usuario: rrhh
Password: (configurable)

Usuario: empleado
Password: (configurable)
```

---

## 4. Configuracion Inicial

### 4.1 Variables de Entorno (.env)

El archivo `.env` en la raíz del proyecto contiene la configuración principal:

```bash
# Autenticacion
AUTH_ISSUER=nucleus-auth
AUTH_AUDIENCE=nucleus-api
AUTH_SIGNING_KEY=nucleus_super_secret_key_32_chars_long!!
AUTH_ADMIN_USER=admin
AUTH_ADMIN_PASS=admin123

# CORS
CORS_ORIGINS=http://localhost:3001,http://localhost:3002

# OpenTelemetry (opcional)
OTEL_ENABLED=true
OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
OTEL_EXPORTER_OTLP_PROTOCOL=grpc
OTEL_TRACES_SAMPLER=always_on
```

### 4.2Levantando el Sistema

#### Desarrollo Local
```bash
# Levantar todos los servicios
docker compose up -d

# O individualmente
docker compose up auth-service organizacion-service personal-service liquidacion-service
```

#### Produccion
```bash
docker compose -f docker-compose.prod.yml up -d
```

### 4.3 URLs de Acceso

| Portal | URL | Descripcion |
|--------|-----|-------------|
| Portal RH | http://localhost:3001 | Administracion de RRHH |
| Portal Empleado | http://localhost:3002 | Autoservicio del empleado |
| Jaeger | http://localhost:16686 | Monitoreo de trazas |
| Prometheus | http://localhost:9090 | Metricas |

---

## 5. Modulos del Sistema

### 5.1 Organizacion

Gestion de la estructura organizacional de la empresa.

#### Funcionalidades
- **Empresas**: Crear, editar, desactivar empresas
- **Unidades Organizativas**: Crear estructura jerárquica
- **Posiciones**: Definir cargos y asignar empleados
- **Centros de Costo**: Seguimiento presupuestario
- **Sindicatos y Convenios**: Gestión de relaciones laborales

#### Endpoints Principales
```
GET    /api/rh/v1/organizacion/empresas
POST   /api/rh/v1/organizacion/empresas
PUT    /api/rh/v1/organizacion/empresas/{id}
GET    /api/rh/v1/organizacion/unidades
POST   /api/rh/v1/organizacion/unidades
GET    /api/rh/v1/organizacion/posiciones
POST   /api/rh/v1/organizacion/posiciones/{id}/asignar
```

### 5.2 Personal

Gestion integral de legajos de empleados.

#### Funcionalidades
- **Legajos**: Alta, modificacion, baja de empleados
- **Datos Personales**: Documentos, domicilio, familia
- **Informacion Laboral**: Posicion, unidad, fecha de ingreso
- **Estados**: Activo, inactivo, licencia

#### Endpoints Principales
```
GET    /api/rh/v1/personal/legajos
POST   /api/rh/v1/personal/legajos
PUT    /api/rh/v1/personal/legajos/{id}
GET    /api/rh/v1/personal/legajos/{id}/documentos
GET    /api/rh/v1/personal/legajos/{id}/domicilios
```

### 5.3 Liquidacion

Gestion de nomina y generación de recibos de sueldos.

#### Funcionalidades
- **Payroll**: Ciclos de liquidacion mensuales/quincenales
- **Legajos en Lote**: Asignar empleados a una liquidacion
- **Procesamiento**: Calculo automatico de conceptos
- **Recibos**: Generacion de recibos con detalle
- **Exportes**: Descarga en JSON/CSV/PDF

#### Endpoints Principales
```
GET    /api/rh/v1/liquidacion/payrolls
POST   /api/rh/v1/liquidacion/payrolls
PATCH  /api/rh/v1/liquidacion/payrolls/{id}
POST   /api/rh/v1/liquidacion/payrolls/{id}/legajos
POST   /api/rh/v1/liquidacion/payrolls/{id}/procesar
GET    /api/rh/v1/liquidacion/payrolls/{id}/recibos
GET    /api/rh/v1/liquidacion/payrolls/{id}/exports
```

#### Configuracion para Liquidaciones

Para configurar liquidaciones, se utilizan los siguientes parametros:

```bash
# Configuracion de Liquidacion (appsettings.json)
{
  "Liquidacion": {
    "PeriodoDefault": 30,
    "DiasPresentacion": 4,
    "Categoria": "Empleado",
    "Convenio": "UOM"
  }
}
```

### 5.4 Tiempos (Control de Asistencia)

Gestion del control horario de empleados.

#### Funcionalidades
- **Turnos**: Definir horarios de entrada/salida
- **Horarios**: Asignar turnos semanales
- **Fichadas**: Registro de entrada/salida
- **Ausencias**: Gestion de licencias y permisos
- **Planillas**: Consolidado de horas por periodo

#### Endpoints Principales
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

### 5.5 Vacaciones

Gestion de solicitudes de vacaciones.

#### Flujo
1. Empleado solicita vacaciones desde Portal Empleado
2. Sistema crea instancia de workflow
3. Jefe recibe notificacion de aprobacion
4. RRHH aprueba/rechaza
5. Sistema actualiza saldo de vacaciones

#### Endpoints
```
GET    /api/rh/v1/vacaciones/solicitudes
POST   /api/rh/v1/vacaciones/solicitudes
GET    /api/rh/v1/vacaciones/saldos
```

### 5.6 Seleccion

Gestion del proceso de reclutamiento.

#### Funcionalidades
- **Avisos**: Publicacion de posiciones vacantes
- **Candidatos**: Registro de postulantes
- **Entrevistas**: Seguimiento del proceso
- **Estados**: En evaluacion, entrevista, rechazado, contratado

#### Endpoints
```
GET    /api/rh/v1/seleccion/candidates
POST   /api/rh/v1/seleccion/candidates
GET    /api/rh/v1/seleccion/avisos
POST   /api/rh/v1/seleccion/avisos
```

### 5.7 Evaluacion

Sistema de evaluacion de desempeño.

#### Funcionalidades
- **Ciclos**: Periodos de evaluacion (trimestral/anual)
- **Metas**: Objetivos del empleado
- **Respuestas**: Autoevaluacion y evaluacion del jefe
- **Calibracion**: Ajuste de resultados

#### Endpoints
```
GET    /api/rh/v1/evaluacion/evaluaciones
POST   /api/rh/v1/evaluacion/evaluaciones
GET    /api/rh/v1/evaluacion/metas
```

### 5.8 Capacitacion

Gestion de formación de empleados.

#### Funcionalidades
- **Cursos**: Catalogo de capaciaciones
- **Inscripciones**: Registro de empleados
- **Asistencias**: Control de asistencia a cursos
- **Certificados**: Emision de certificaciones

#### Endpoints
```
GET    /api/rh/v1/capacitacion/cursos
GET    /api/rh/v1/capacitacion/cursos/{id}
GET    /api/rh/v1/capacitacion/inscripciones
POST   /api/rh/v1/capacitacion/inscripciones
```

### 5.9 Clima Laboral

Encuestas de satisfaccion y clima organizacional.

#### Funcionalidades
- **Encuestas**: Creacion de encuestas
- **Preguntas**: Definicion de preguntas
- **Respuestas**: Recopilacion de respuestas
- **Resultados**: Analisis de resultados

#### Endpoints
```
GET    /api/rh/v1/clima/encuestas
POST   /api/rh/v1/clima/encuestas
GET    /api/rh/v1/clima/encuestas/{id}
POST   /api/rh/v1/clima/encuestas/{id}/responder
GET    /api/rh/v1/clima/resultados
```

### 5.10 Carrera y Sucesion

Planificacion de carrera y sucesion de posiciones.

#### Funcionalidades
- **Planes de Carrera**: Definicion de paths profesionales
- **Posiciones Criticas**: Identificacion de posiciones clave
- **Sucesores**: Identificacion de candidatos para succession

#### Endpoints
```
GET    /api/rh/v1/carrera/planes
POST   /api/rh/v1/carrera/planes
GET    /api/rh/carrera/succession
```

### 5.11 Configuracion

Gestion de parametros y catalogos del sistema.

#### Endpoints
```
GET    /api/rh/v1/configuracion/catalogos/{tipo}
POST   /api/rh/v1/configuracion/catalogos
GET    /api/rh/v1/configuracion/parametros
```

---

## 6. Portales

### 6.1 Portal RH

Panel de administracion para el area de RRHH.

#### URL
http://localhost:3001

#### Funcionalidades
1. **Organizacion**: CRUD de empresas, unidades, posiciones
2. **Personal**: Gestion de legajos
3. **Liquidacion**: Crear y procesar nominas
4. **Reportes**: Vistas de datos
5. **Integraciones**: Gestion de jobs y triggers
6. **Aprobaciones**: Revisar solicitudes pendientes

#### Configuracion Inicial
Al ingresar, configurar las URLs de los servicios:
```
Auth API: http://localhost:5001
Organizacion API: http://localhost:5100
Personal API: http://localhost:5200
Liquidacion API: http://localhost:5188
Portal BFF: http://localhost:5090
Workflow API: http://localhost:5051
Integration Hub: http://localhost:5050
Configuracion API: http://localhost:5300
Tiempos API: http://localhost:5400
```

### 6.2 Portal Empleado

Autoservicio para empleados.

#### URL
http://localhost:3002

#### Funcionalidades
1. **Dashboard**: Vista general de informacion
2. **Mis Datos**: Consulta de informacion personal
3. **Recibos**: Historial de recibos de sueldo
4. **Vacaciones**: Solicitud y seguimiento
5. **Notificaciones**: Alertas y avisos
6. **Workflows**: Seguimiento de solicitudes

#### Login
- Usuario:Credenciales configuradas en auth-service
- Token JWT expires en 24 horas

---

## 7. APIs y Endpoints

### 7.1 Autenticacion

```
POST /api/auth/login
Body: { "username": "admin", "password": "admin123" }
Response: { "token": "eyJ...", "expiresIn": 86400 }
```

### 7.2 Notificaciones

```
GET  /api/portal/v1/notificaciones
GET  /api/portal/v1/notificaciones?unreadOnly=true&limit=50
POST /api/portal/v1/notificaciones/{id}/read
POST /api/portal/v1/notificaciones/read-all
```

### 7.3 Dashboard

```
GET /api/rh/v1/dashboard/resumen
```

Respuesta:
```json
{
  "empleados": { "total": 180, "activos": 165 },
  "liquidacion": { "ultimoPeriodo": "2026-03", "totalNeto": 32000000 },
  "tiempos": { "asistenciaPromedio": "98.5%" }
}
```

### 7.4 Salud del Sistema

```
GET /api/rh/v1/sistema/health
GET /api/rh/v1/sistema/metricas
GET /api/rh/v1/sistema/info
```

---

## 8. Integraciones

### 8.1 Integration Hub

Sistema de integracion con servicios externos.

#### Componentes
- **Templates**: Definiciones de integracion
- **Jobs**: Ejecuciones de integracion
- **Triggers**: Eventos que disparan integraciones
- **Conexiones**: Configuracion de destinos

#### Endpoints
```
GET    /api/rh/v1/integraciones/templates
GET    /api/rh/v1/integraciones/jobs
GET    /api/rh/v1/integraciones/jobs/{id}
POST   /api/rh/v1/integraciones/jobs
POST   /api/rh/v1/integraciones/jobs/{id}/retry
GET    /api/rh/v1/integraciones/eventos
POST   /api/rh/v1/integraciones/triggers/{nombre}/execute
```

### 8.2 Integraciones Externas Soportadas

| Sistema | Tipo | Descripcion |
|---------|------|-------------|
| AFIP | API Rest | Declaraciones juradas, aportes |
| Bancos | SFTP | Pagos de haberes |
| SSN | API Rest | Sistema Nacional de Salud |
| Proveedores | API Rest | Suministros y servicios |

---

## 9. Workflows

### 9.1 Nucleus WF

Motor de workflows para procesos de RRHH.

#### Definiciones Demo Incluidas
- Solicitud de Vacaciones
- Modificacion de Datos Personales
- Reclamos
- Solicitud de Anticipo

#### Endpoints
```
GET    /api/rh/v1/wf/definitions
POST   /api/rh/v1/wf/definitions
GET    /api/rh/v1/wf/instances
POST   /api/rh/v1/wf/instances
POST   /api/rh/v1/wf/instances/{id}/transitions
```

#### Flujo Tipico
1. **Iniciar**: POST /instances con definitionId
2. **Transicionar**: POST /instances/{id}/transitions con action
3. **Consultar**: GET /instances/{id}

### 9.2 Ejemplo: Solicitud de Vacaciones

```javascript
// 1. Iniciar solicitud
POST /api/rh/v1/wf/instances
{
  "definitionId": "vacaciones",
  "legajoId": "leg-001",
  "data": {
    "fechaDesde": "2026-04-15",
    "fechaHasta": "2026-04-22",
    "dias": 6
  }
}

// 2. Aprobar (desde Portal RH)
POST /api/rh/v1/wf/instances/{id}/transitions
{
  "action": "aprobar"
}

// 3. Consultar estado
GET /api/rh/v1/wf/instances/{id}
```

---

## 10. Configuracion de Variables de Entorno

### 10.1 Autenticacion

| Variable | Descripcion | Ejemplo |
|----------|-------------|---------|
| AUTH_ISSUER | Emisor del token JWT | nucleus-auth |
| AUTH_AUDIENCE | Audiencia del token | nucleus-api |
| AUTH_SIGNING_KEY | Clave de firma (min 32 chars) | clave_secreta_32_caracteres!! |
| AUTH_ADMIN_USER | Usuario admin por defecto | admin |
| AUTH_ADMIN_PASS | Password admin por defecto | admin123 |

### 10.2 Base de Datos

| Variable | Descripcion | Ejemplo |
|----------|-------------|---------|
| DATABASE_PROVIDER | Proveedor (sqlite/postgres) | sqlite |
| ConnectionStrings__AuthDb | Connection string auth | Data Source=auth.db |

### 10.3 Liquidacion

```json
{
  "Liquidacion": {
    "PeriodoDefault": 30,
    "DiasPresentacion": 4,
    "Categoria": "Empleado",
    "Convenio": "UOM",
    "ImpuestoGanancias": true,
    "AporteJubilatorio": 0.11,
    "ObraSocial": 0.03
  }
}
```

### 10.4 CORS

| Variable | Descripcion | Ejemplo |
|----------|-------------|---------|
| CORS_ORIGINS | Orígenes permitidos | http://localhost:3001,http://localhost:3002 |

### 10.5 OpenTelemetry

| Variable | Descripcion | Ejemplo |
|----------|-------------|---------|
| OTEL_ENABLED | Habilitar trazas | true |
| OTEL_EXPORTER_OTLP_ENDPOINT | Endpoint del collector | http://otel-collector:4317 |
| OTEL_EXPORTER_OTLP_PROTOCOL | Protocolo (grpc/http) | grpc |
| OTEL_TRACES_SAMPLER | Sampler (always_on/always_off) | always_on |

---

## 11. Monitoreo y Observabilidad

### 11.1 Jaeger

Acceso: http://localhost:16686

#### Ver Trazas
1. Abrir Jaeger en el navegador
2. Seleccionar servicio del dropdown
3. Click en "Find Traces"
4. Explorar trazas en el panel derecho

#### Solucionar Problemas
- **No aparecen servicios**: Ejecutar requests a los endpoints primero
- **Solo aparece jaeger-all-in-one**: Verificar OTEL_ENABLED=true en servicios
- **Sin trazas**: Revisar logs del otel-collector

### 11.2 Prometheus

Acceso: http://localhost:9090

#### Metricas Disponibles
- request_duration_seconds
- requests_total
- process_cpu_seconds_total

---

## 12. Guia de Operacion

### 12.1 Tareas Diarias

1. **Verificar Salud del Sistema**
   - Acceder a Jaeger y Prometheus
   - Verificar que todos los servicios respondan

2. **Revisar Notificaciones**
   - Login en Portal RH
   - Revisar panel de notificaciones

3. **Aprobar Solicitudes**
   - Revisar workflows pendientes
   - Aprobar/rechazar segun corresponda

### 12.2 Tareas Semanales

1. **Generar Planilla de Tiempos**
   - Consolidar horas de la semana
   - Exportar a liquidacion

2. **Revisar Integraciones**
   - Verificar jobs fallidos
   - Reintentar si es necesario

3. **Reportes**
   - Generar reportes de asistencia
   - Revisar metricas de RRHH

### 12.3 Tareas Mensuales

1. **Liquidacion de Haberes**
   - Crear nuevo payroll
   - Agregar legajos
   - Procesar liquidacion
   - Generar recibos
   - Exportar para bancos

2. **Cierre de Mes**
   - Verificar asistencia del mes
   - Generar reportes mensuales

### 12.4 Troubleshooting Common

| Problema | Solucion |
|----------|----------|
| 401 Unauthorized | Verificar token con prefijo "Bearer" |
| CORS Error | Verificar CORS_ORIGINS en .env |
| No hay datos | Ejecutar POST /api/rh/v1/test/seed |
| Servicios no levantan | docker compose logs [servicio] |
| Trazas no aparecen | Verificar OTEL_ENABLED=true |

---

## Anexo:seed Data

Para pruebas, existe un script de seed data en `seed-data.js` que contiene datos de prueba para todos los modulos.

```bash
# Ejecutar seed
curl -X POST http://localhost:5090/api/rh/v1/test/seed \
  -H "Authorization: Bearer [TOKEN]"
```

---

## Contacto y Soporte

- **Documentacion**: Directorio `docs/`
- **API Docs**: Swagger en cada servicio (desarrollo)
- **Runbooks**: `docs/operacion/runbooks.md`
- **Monitoreo**: Jaeger (puerto 16686), Prometheus (puerto 9090)

---

*Documento generado automaticamente - Nucleus RH Next*
*Ultima actualizacion: 2026-03-14*

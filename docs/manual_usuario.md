# Manual de usuario · Nucleus RH Next

## Alcance y estado
Este manual resume las operaciones disponibles hoy (MVP) y las operaciones planificadas (blueprints) por modulo. Lo que figura como "Blueprint" no esta implementado funcionalmente en este repo.

## Roles
- RRHH Admin: gestiona organizacion, legajos, liquidaciones, integraciones y aprobaciones.
- Empleado: consulta recibos y realiza solicitudes desde Portal Empleado.

## Modulos operativos (MVP)

### Portal RH (UI)
Operaciones disponibles:
- Empresas: crear, editar.
- Unidades organizativas: crear.
- Posiciones: crear.
- Legajos: asignar a posiciones.
- Sindicatos: crear, editar.
- Convenios: crear, editar.
- Liquidaciones: crear, editar, procesar, exportar y ver detalle.
- Exportes: descargar JSON/CSV.
- Integraciones: iniciar jobs, ver detalle y reintentar; ver eventos; ejecutar y administrar triggers.
- Vacaciones: aprobar o rechazar instancias.
- BFF: operar via `/api/rh/v1/*`.

### Portal Empleado (UI)
Operaciones disponibles:
- Login con JWT.
- Ver liquidaciones y detalle de recibos.
- Render de recibo imprimible.
- Descargar recibos en JSON/CSV.
- Notificaciones (ver, limpiar, marcar leidas, resumen, filtrar no leidas).
- Descargar exportes via BFF.
- Home con widgets via BFF.
- Workflows (MVP): iniciar instancias, transicionar estados, aprobar/rechazar tareas.
- Solicitudes MVP: vacaciones, datos personales, reclamos.

### Liquidacion (service + UI)
Operaciones disponibles:
- Crear ciclos de nomina y administrar legajos.
- Procesar recibos y generar exportes.
- Consultar recibos y exportes por liquidacion.

### Integraciones (Integration Hub)
Operaciones disponibles:
- Scheduler con reintentos.
- Templates, jobs, conexiones y eventos almacenados en SQLite.
- Triggers configurables con ejecucion manual.
- Rate limiting global.

### Nucleus WF (MVP via Portal)
Operaciones disponibles:
- Cargar definiciones demo.
- Crear instancias.
- Ejecutar transiciones en instancias.

### Vacaciones (MVP via WF)
Operaciones disponibles:
- Iniciar solicitud desde Portal Empleado.
- Aprobar/rechazar desde Portal RH.

### Configuracion Avanzada
Operaciones disponibles:
- Catalogos: crear, listar por tipo, editar y eliminar.
- Parametros: listar, consultar por clave y upsert.

### Organizacion
Operaciones disponibles:
- Empresas: crear, editar y desactivar.
- Unidades: crear, editar, desactivar y ver arbol.
- Posiciones: crear, editar, desactivar, asignar/desasignar legajos.
- Centros de costo: crear, editar y desactivar.
- Sindicatos y convenios: crear, editar y desactivar.
- Organigramas: versionar estructura, listar versiones y consultar una version.

### Personal
Operaciones disponibles:
- Legajos: crear, editar y desactivar.
- Familiares y licencias: actualizar por legajo.
- Domicilios y documentos: actualizar por legajo.

## Modulos blueprint (planificados)

### Organizacion
Operaciones previstas:
- Organigramas avanzados (editor visual, comparacion entre versiones).
- Integracion con Legajos, Presupuesto y Liquidacion.

### Personal
Operaciones previstas:
- Solicitudes de cambio con aprobacion y auditoria.
- Integracion avanzada con catalogos y eventos.

### Tiempos Trabajados
Operaciones previstas:
- Ingesta y gestion de fichadas.
- ABM de turnos/horarios y planillas.
- Procesamiento de horas y exportes a Liquidacion.

### Tesoreria
Operaciones previstas:
- Gestion de adelantos, pagos, conciliaciones y fondos.
- Workflows de aprobacion y ciclos de pago.

### Seleccion / WebCV
Operaciones previstas:
- Gestion de avisos, candidatos, postulaciones y entrevistas.
- Portal de postulantes y recruiter UI.

### Seguridad
Operaciones previstas:
- Registro de accesos, rondas, incidentes y alertas.
- Integraciones con control de acceso fisico.

### Sanciones
Operaciones previstas:
- Gestion de sanciones, actas, acuerdos e historial.
- Workflow disciplinario.

### Reclamos
Operaciones previstas:
- CRUD de reclamos, comentarios, adjuntos y SLA.
- Workflow de clasificacion, resolucion y confirmacion.

### Presupuesto
Operaciones previstas:
- Presupuestos de headcount y costos, escenarios y comparaciones.
- Integraciones con Liquidacion y Organizacion.

### Medicina Laboral
Operaciones previstas:
- Examenes, aptos, licencias y campañas de salud.
- Integracion con Tiempos y Reclamos.

### Evaluacion
Operaciones previstas:
- Ciclos de performance, objetivos, competencias y calibraciones.

### Control de Visitas
Operaciones previstas:
- Preregistro, autorizaciones, check-in/out y credenciales.

### Configuracion Avanzada
Operaciones previstas:
- Feature flags y versionado de configuracion.

### Clima / Encuestas
Operaciones previstas:
- Campanas de encuestas, respuestas, segmentacion y acciones.

### Carrera y Sucesion
Operaciones previstas:
- Planes de carrera, mapas de sucesion y pools de talento.

### Capacitacion
Operaciones previstas:
- Cursos, inscripciones, sesiones, asistencias y certificados.

### Beneficios
Operaciones previstas:
- Catalogo de beneficios, elegibilidad e inscripciones.

### Analytics
Operaciones previstas:
- Consolidacion de datos y dashboards centralizados.

### Accidentabilidad
Operaciones previstas:
- Registro de incidentes, investigaciones y acciones correctivas.

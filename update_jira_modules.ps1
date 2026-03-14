$headers = @{
    Authorization='Basic am9uYXRhbm0udmVyYWxvcGV6QGdtYWlsLmNvbTpBVEFUVDN4RmZHRjBXb1dwakRTVXk0WElYRV9TVk1idEV1eUp4ekh2ZkJxbFlYU1F1b0ppaVJtMjhDSEdrc2xVcUswa3BQLUVZeU5oMHZqVVVVYVNKSkwyeHBxc285OHcwVEZFVXdYbWhFem9MSWNjREM2cHZWdzlsQld3NXRRekYweFMwWk9hZnVLcHBOSU5rdmp6RlBCTnlJbEp1OFBxWXFYVVlxMi1fZm1HLTNaY0hHUXhsLVE9QzkxQjI2QTE='
    'Content-Type'='application/json'
}

# Module epics with detailed descriptions
$modules = @{
    "KAN-17" = @{
        summary = "Modulo: Accidentabilidad"
        description = "## Módulo de Accidentabilidad

### Descripción
Sistema de registro y gestión de incidentes laborales e investigaciones de accidentes.

### Objetivos
- Registro de incidentes y accidentes laborales
- Investigaciones de causa raíz
- Estadísticas de accidentabilidad
- Cumplimiento normativo

### Valor al Producto
- Cumplimiento legal y normativo
- Reducción de incidentes mediante análisis
- Historial completo de accidents"
    }
    "KAN-18" = @{
        summary = "Modulo: Analytics"
        description = "## Módulo de Analytics

### Descripción
Dashboard y reportes analíticos para visualización de KPIs de RRHH.

### Objetivos
- Dashboards interactivos
- Indicadores de RRHH
- Reportes customizables
- Exportación de datos

### Valor al Producto
- Toma de decisiones basada en datos
- Visibilidad del estado de RRHH
- Reportes ejecutivos"
    }
    "KAN-19" = @{
        summary = "Modulo: Beneficios"
        description = "## Módulo de Beneficios

### Descripción
Gestión de beneficios para empleados como seguros, días extras, etc.

### Objetivos
- Catálogo de beneficios
- Inscripción de empleados
- Gestión de vida laboral

### Valor al Producto
- Atracción y retención de talento
- Beneficios claros para empleados
- Administración centralizada"
    }
    "KAN-20" = @{
        summary = "Modulo: Capacitacion"
        description = "## Módulo de Capacitación

### Descripción
Sistema de gestión de cursos, capacitaciones y desarrollo profesional.

### Objetivos
- Catálogo de cursos
- Inscripción a capacitaciones
- Seguimiento de progreso
- Certificaciones

### Valor al Producto
- Desarrollo continuo de empleados
- Mejora de competencias
- Registro de capacitaciones"
    }
    "KAN-21" = @{
        summary = "Modulo: Carrera Sucesion"
        description = "## Módulo de Carrera y Sucesión

### Descripción
Gestión de sucesión y planificación de carrera de empleados.

### Objetivos
- Identificación de talento
- Planes de sucesión
- Evaluación de potencial
- Carrera profesional

### Valor al Producto
- Retención de key players
- Desarrollo de liderazgo
- Continuidad del negocio"
    }
    "KAN-22" = @{
        summary = "Modulo: Clima"
        description = "## Módulo de Clima Laboral

### Descripción
Sistema de encuestas y medición de clima organizacional.

### Objetivos
- Creación de encuestas
- Medición de satisfacción
- Análisis de resultados
- Planes de acción

### Valor al Producto
- Mejora del ambiente laboral
- Feedback continuo
- Identificación de problemas"
    }
    "KAN-23" = @{
        summary = "Modulo: Configuracion Avanzada"
        description = "## Módulo de Configuración Avanzada

### Descripción
Configuraciones globales del sistema Nucleus RH.

### Objetivos
- Parámetros del sistema
- Reglas de negocio
- Workflows personalizables
- Integraciones

### Valor al Producto
- Flexibilidad del sistema
- Adaptación a necesidades
- Configuración centralizada"
    }
    "KAN-24" = @{
        summary = "Modulo: Control Visitas"
        description = "## Módulo de Control de Visitas

### Descripción
Sistema de registro y control de visitantes a las instalaciones.

### Objetivos
- Registro de visitantes
- Control de acceso
- Historial de visitas
- Reportes

### Valor al Producto
- Seguridad de instalaciones
- Compliance
- Registro de visitas"
    }
    "KAN-25" = @{
        summary = "Modulo: Evaluacion"
        description = "## Módulo de Evaluación

### Descripción
Sistema de evaluación de desempeño de empleados.

### Objetivos
- Evaluaciones periódicas
- Metas y objetivos
- Feedback 360
- Planes de mejora

### Valor al Producto
- Desarrollo de empleados
- Alineación con objetivos
- Mejora continua"
    }
    "KAN-26" = @{
        summary = "Modulo: Integraciones"
        description = "## Módulo de Integraciones

### Descripción
Centro de integración con sistemas externos y APIs.

### Objetivos
- Integración con sistemas legacy
- APIs externas
- Sincronización de datos
- Webhooks

### Valor al Producto
- Datos actualizados
- Automatización
- Conectividad"
    }
    "KAN-27" = @{
        summary = "Modulo: Liquidacion"
        description = "## Módulo de Liquidación

### Descripción
Sistema de liquidación de sueldos y generación de recibos.

### Objetivos
- Cálculo de liquidación mensual
- Generación de recibos
- Exportación de información
- Integración contable

### Valor al Producto
- Automatización de liquidaciones
- Recibo digital para empleados
- Cálculo correcto de haberes"
    }
    "KAN-28" = @{
        summary = "Modulo: Medicina"
        description = "## Módulo de Medicina Laboral

### Descripción
Sistema de salud ocupacional y controles médicos.

### Objetivos
- Exámenes ocupacionales
- Gestión de licencias
- Alertas de vencimiento
- Estadísticas de salud

### Valor al Producto
- Compliance legal
- Salud de empleados
- Controlling médico"
    }
    "KAN-29" = @{
        summary = "Modulo: Nucleuswf"
        description = "## Módulo de Workflow (NucleusWF)

### Descripción
Motor de workflows para aprobaciones y procesos de RRHH.

### Objetivos
- Definición de workflows
- Aprobaciones automáticas
- Notificaciones
- Historial de aprobaciones

### Valor al Producto
- Automatización de procesos
- Trazabilidad
- Eficiencia operativa"
    }
    "KAN-30" = @{
        summary = "Modulo: Organizacion"
        description = "## Módulo de Organización

### Descripción
Gestión de la estructura organizacional de la empresa.

### Objetivos
- Empresas y sucursales
- Unidades organizativas
- Posiciones y jerarquías
- Organigrama

### Valor al Producto
- Estructura clara de RRHH
- Control organizacional
- Reporting estructural"
    }
    "KAN-31" = @{
        summary = "Modulo: Personal"
        description = "## Módulo de Personal

### Descripción
Gestión de legajos y datos personales de empleados.

### Objetivos
- Altas, bajas, modificaciones
- Datos personales
- Documentación
- Historial laboral

### Valor al Producto
- Legajo digital completo
- Datos actualizados
- Historia del empleado"
    }
    "KAN-32" = @{
        summary = "Modulo: Portal Empleado"
        description = "## Módulo de Portal Empleado

### Descripción
Interfaz web para que los empleados accedan a su información.

### Objetivos
- Consulta de liquidaciones
- Solicitud de vacaciones
- Notificaciones
- Datos personales

### Valor al Producto
- Autoservicio de empleado
- Reducción de consultas a RRHH
- Experiencia de usuario moderna"
    }
    "KAN-33" = @{
        summary = "Modulo: Presupuesto"
        description = "## Módulo de Presupuesto

### Descripción
Gestión de presupuesto de RRHH y headcount.

### Objetivos
- Headcount planificado vs real
- Costos de personal
- Proyecciones
- Controlling

### Valor al Producto
- Control de costos
- Planificación de RRHH
- Reporting financiero"
    }
    "KAN-34" = @{
        summary = "Modulo: Reclamos"
        description = "## Módulo de Reclamos

### Descripción
Sistema de gestión de reclamos de empleados.

### Objetivos
- Registro de reclamos
- Seguimiento
- Resolución
- Estadísticas

### Valor al Producto
- Atención al empleado
- Mejora continua
- Trazabilidad"
    }
    "KAN-35" = @{
        summary = "Modulo: Sanciones"
        description = "## Módulo de Sanciones

### Descripción
Gestión de sanciones disciplinarias.

### Objetivos
- Registro de sanciones
- Historial
- Notificaciones
- Compliance

### Valor al Producto
- Control disciplinario
- Cumplimiento legal
- Registro histórico"
    }
    "KAN-36" = @{
        summary = "Modulo: Seguridad"
        description = "## Módulo de Seguridad

### Descripción
Sistema de seguridad laboral y prevención de riesgos.

### Objetivos
- Registro de mediciones
- Control de EPIs
- Capacitaciones de seguridad
- Inspecciones

### Valor al Producto
- Prevención de riesgos
- Cumplimiento normativo
- Ambiente seguro"
    }
    "KAN-37" = @{
        summary = "Modulo: Seleccion"
        description = "## Módulo de Selección

### Descripción
Sistema de gestión de reclutamiento y selección de personal.

### Objetivos
- Gestión de búsquedas
- Recepción de CVs
- Seguimiento de candidatos
- Entrevistas

### Valor al Producto
- Proceso de selección eficiente
- Mejor candidato para el puesto
- Reducción de tiempo de cobertura"
    }
    "KAN-38" = @{
        summary = "Modulo: Tesoreria"
        description = "## Módulo de Tesorería

### Descripción
Gestión de pagos, adelantos y conciliaciones.

### Objetivos
- Gestión de adelantos
- Control de pagos
- Conciliaciones
- Reporting

### Valor al Producto
- Control de efectivo
- Anticipos a empleados
- Reconciliation"
    }
    "KAN-39" = @{
        summary = "Modulo: Tiempos"
        description = "## Módulo de Tiempos

### Descripción
Control de asistencia, horarios y turnos.

### Objetivos
- Registro de ingresos
- Gestión de turnos
- Cálculo de horas
- Fichadas

### Valor al Producto
- Control de asistencia
- Horas trabajadas exactas
- Base para liquidación"
    }
    "KAN-40" = @{
        summary = "Modulo: Vacaciones"
        description = "## Módulo de Vacaciones

### Descripción
Gestión completa de vacaciones y licencias.

### Objetivos
- Solicitud de vacaciones
- Workflow de aprobación
- Cálculo de saldo
- Historial

### Valor al Producto
- Empleado gestiona sus vacaciones
- Aprobación automatizada
- Control de saldo"
    }
}

foreach ($key in $modules.Keys) {
    $mod = $modules[$key]
    $body = '{
        "fields": {
            "summary": "' + $mod.summary + '",
            "description": {
                "type": "doc",
                "version": 1,
                "content": [{
                    "type": "paragraph",
                    "content": [{"type": "text", "text": "' + $mod.description.Replace("`n", "").Replace('"', '\"') + '"}]
                }]
            }
        }
    }'
    
    try {
        $null = Invoke-RestMethod -Uri "https://jonatanmveralopez.atlassian.net/rest/api/3/issue/$key" -Headers $headers -Method Put -Body $body
        Write-Output "Updated: $key - $($mod.summary)"
    } catch {
        Write-Output "Failed to update $key : $($_.Exception.Message)"
    }
}

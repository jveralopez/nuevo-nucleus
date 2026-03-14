$headers = @{
    Authorization='Basic am9uYXRhbm0udmVyYWxvcGV6QGdtYWlsLmNvbTpBVEFUVDN4RmZHRjBXb1dwakRTVXk0WElYRV9TVk1idEV1eUp4ekh2ZkJxbFlYU1F1b0ppaVJtMjhDSEdrc2xVcUswa3BQLUVZeU5oMHZqVVVVYVNKSkwyeHBxc285OHcwVEZFVXdYbWhFem9MSWNjREM2cHZWdzlsQld3NXRRekYweFMwWk9hZnVLcHBOSU5rdmp6RlBCTnlJbEp1OFBxWXFYVVlxMi1fZm1HLTNaY0hHUXhsLVE9QzkxQjI2QTE='
    'Content-Type'='application/json'
}

# Sprint descriptions with objectives and product value
$sprints = @{
    "KAN-41" = @{
        summary = "Sprint 3"
        description = "## Sprint 3: Foundation Setup

### Objetivos
- Configurar infraestructura base de servicios
- Establecer pipeline CI/CD
- Configurar logging y monitoreo

### Valor al Producto
- Base sólida para desarrollo continuo
- Visibilidad de errores en producción
- Despliegues automatizados y confiables"
    }
    "KAN-42" = @{
        summary = "Sprint 4"
        description = "## Sprint 4: Core Modules Integration

### Objetivos
- Integrar módulo de Liquidación
- Conectar Portal BFF con servicios core
- Implementar autenticación centralizada

### Valor al Producto
- Portal empleado puede ver liquidaciones
- Sistema de autenticación unificado
- Primera funcionalidad visible para usuarios"
    }
    "KAN-43" = @{
        summary = "Sprint 5"
        description = "## Sprint 5: Organization & Personal Modules

### Objetivos
- Implementar gestión de organizaciones
- Desarrollar módulo de Legajos
- Integrar con NucleusWF

### Valor al Producto
- RRHH puede gestionar estructura organizacional
- Control completo de legajos de empleados
- Workflows automatizados de aprobación"
    }
    "KAN-44" = @{
        summary = "Sprint 6"
        description = "## Sprint 6: Time Management

### Objetivos
- Implementar módulo de Tiempos
- Gestión de turnos y horarios
- Sistema de fichadas

### Valor al Producto
- Control de asistencia automatizado
- Registro de horarios por empleado
- Base para cálculo de horas trabajadas"
    }
    "KAN-45" = @{
        summary = "Sprint 7+"
        description = "## Sprint 7+: Advanced Modules

### Objetivos
- Desarrollar módulo de Vacaciones
- Implementar审批 workflow
- Agregar notificaciones

### Valor al Producto
- Empleados pueden solicitar vacaciones
- Aprobaciones automatizadas
- Portal empleado más completo"
    }
    "KAN-88" = @{
        summary = "Sprint 8"
        description = "## Sprint 8: Vacation Module Complete

### Objetivos
- Completar módulo de Vacaciones
- Simulador de saldo
- Integración con Liquidación

### Valor al Producto
- Gestión completa de vacaciones
- Empleados ven su saldo disponible
- Cálculo automático de días"
    }
    "KAN-89" = @{
        summary = "Sprint 9"
        description = "## Sprint 9: Treasury & Budget

### Objetivos
- Implementar módulo de Tesorería
- Gestión de adelantos y pagos
- Módulo de Presupuesto

### Valor al Producto
- Control de pagos y adelantos
- Gestión de headcount y costos
- Vista financiera de RRHH"
    }
    "KAN-90" = @{
        summary = "Sprint 10"
        description = "## Sprint 10: Additional Modules

### Objetivos
- Módulo de Beneficios
- Sistema de Accidentabilidad
- Control de Visitas
- Seguridad laboral

### Valor al Producto
- Catálogo de beneficios para empleados
- Registro de incidentes
- Control de visitantes
- Cumplimiento normativo"
    }
    "KAN-110" = @{
        summary = "Sprint 11"
        description = "## Sprint 11: HR Advanced Modules

### Objetivos
- Completar módulo de Selección
- Implementar Evaluación de desempeño
- Desarrollo de Carrera y Sucesión

### Valor al Producto
- Gestión de candidatos y búsquedas
- Evaluaciones periódicas de empleados
- Plan de sucesión y desarrollo profesional"
    }
    "KAN-111" = @{
        summary = "Sprint 12"
        description = "## Sprint 12: Training & Climate

### Objetivos
- Implementar módulo de Capacitación
- Sistema de Clima laboral
- Encuestas de satisfacción

### Valor al Producto
- Catálogo de cursos y capacitaciones
- Medición de clima organizacional
- Mejora continua basada en feedback"
    }
    "KAN-112" = @{
        summary = "Sprint 13"
        description = "## Sprint 13: Final Integration & Testing

### Objetivos
- Testing E2E completo
- Documentación final
- Optimización de performance

### Valor al Producto
- Sistema robusto y sin errores
- Documentación completa para usuarios
- Producto listo para producción"
    }
}

foreach ($key in $sprints.Keys) {
    $sprint = $sprints[$key]
    $body = '{
        "fields": {
            "summary": "' + $sprint.summary + '",
            "description": {
                "type": "doc",
                "version": 1,
                "content": [{
                    "type": "paragraph",
                    "content": [{"type": "text", "text": "' + $sprint.description.Replace("`n", "").Replace('"', '\"') + '"}]
                }]
            }
        }
    }'
    
    try {
        $null = Invoke-RestMethod -Uri "https://jonatanmveralopez.atlassian.net/rest/api/3/issue/$key" -Headers $headers -Method Put -Body $body
        Write-Output "Updated: $key - $($sprint.summary)"
    } catch {
        Write-Output "Failed to update $key : $($_.Exception.Message)"
    }
}

$headers = @{
    Authorization='Basic am9uYXRhbm0udmVyYWxvcGV6QGdtYWlsLmNvbTpBVEFUVDN4RmZHRjBXb1dwakRTVXk0WElYRV9TVk1idEV1eUp4ekh2ZkJxbFlYU1F1b0ppaVJtMjhDSEdrc2xVcUswa3BQLUVZeU5oMHZqVVVVYVNKSkwyeHBxc285OHcwVEZFVXdYbWhFem9MSWNjREM2cHZWdzlsQld3NXRRekYweFMwWk9hZnVLcHBOSU5rdmp6RlBCTnlJbEp1OFBxWXFYVVlxMi1fZm1HLTNaY0hHUXhsLVE9QzkxQjI2QTE='
    'Content-Type'='application/json'
}

# Task descriptions with objectives
$tasks = @{
    "KAN-7" = @{ desc = "Test/Validacion: Generar informe de pruebas inicial del sistema" }
    "KAN-8" = @{ desc = "Test/Validacion: Verificar que Portal RH y Portal Empleado cargan correctamente" }
    "KAN-9" = @{ desc = "Test/Validacion: Verificar checklist de validacion de servicios" }
    "KAN-10" = @{ desc = "Test/Validacion: Verificar estado de CI/CD pipeline" }
    "KAN-11" = @{ desc = "Test/Validacion: Validar modulo de Tiempos - Turnos, horarios, fichadas" }
    "KAN-12" = @{ desc = "Test/Validacion: Verificar migraciones de Entity Framework" }
    "KAN-13" = @{ desc = "Test/Validacion: Verificar funcionamiento del BFF" }
    "KAN-14" = @{ desc = "Test/Validacion: Actualizacion de datos de prueba" }
    "KAN-15" = @{ desc = "Test/Validacion: Smoke test completo del sistema" }
    "KAN-16" = @{ desc = "Test/Validacion: Revision de PR #1 - Binarios y archivos grandes" }
    "KAN-46" = @{ desc = "Test/Validacion: Validar datos de CSV Ganancias Art94 y Art30" }
    "KAN-47" = @{ desc = "Test/Validacion: Checklist de validacion de servicios - Health checks" }
    "KAN-48" = @{ desc = "CI/CD: Pipeline que compila y pasa tests automaticamente" }
    "KAN-49" = @{ desc = "Pruebas Integrales: Builds, tests unitarios y E2E manuales" }
    "KAN-50" = @{ desc = "Revision PR #1: Gestionar binarios y archivos grandes en git" }
    "KAN-51" = @{ desc = "Smoke Test: Verificar que Docker Compose levanta todos los servicios" }
    "KAN-52" = @{ desc = "Validacion BFF: Verificar que los endpoints proxy funcionan correctamente" }
    "KAN-53" = @{ desc = "Validacion migraciones EF: Verificar organizacion-service migraciones" }
    "KAN-54" = @{ desc = "Validacion portales: Portal RH y Empleado cargan correctamente" }
    "KAN-55" = @{ desc = "Validacion modulo Tiempos: Turnos, horarios, fichadas" }
    "KAN-56" = @{ desc = "Medicina Laboral: Reportes legales para cumplimiento normativo" }
    "KAN-57" = @{ desc = "Medicina Laboral: Notificaciones en Portal Empleado para examenes" }
    "KAN-58" = @{ desc = "Medicina Laboral: KPIs de salud y compliance - Dashboard" }
    "KAN-59" = @{ desc = "Medicina Laboral: Alertas automaticas de vencimiento de examenes" }
    "KAN-60" = @{ desc = "Vacaciones: Backend service para gestion de saldo y solicitudes" }
    "KAN-61" = @{ desc = "Vacaciones: Integracion con modulo de Legajos" }
    "KAN-62" = @{ desc = "Vacaciones: Simulador de saldo de vacaciones" }
    "KAN-63" = @{ desc = "Vacaciones: Workflow en NucleusWF para aprobaciones" }
    "KAN-64" = @{ desc = "Vacaciones: Bandeja de aprobaciones para aprobadores" }
    "KAN-65" = @{ desc = "Vacaciones: Integracion con Tiempos y Liquidacion" }
    "KAN-66" = @{ desc = "Vacaciones: UI Portal Empleado - Solicitud y calendario" }
    "KAN-67" = @{ desc = "Vacaciones: UI para aprobador - Bandeja de entradas" }
    "KAN-68" = @{ desc = "BUG: Configuracion de OTEL y Jaeger para trazabilidad" }
    "KAN-69" = @{ desc = "BUG: Corregir endpoint 404 en modulo de Tiempos" }
    "KAN-70" = @{ desc = "BUG: Verificar y ejecutar migraciones pendientes" }
    "KAN-71" = @{ desc = "BUG: Corregir rutas del Integration Hub" }
    "KAN-72" = @{ desc = "Mejora: Documentar estructura de migraciones de Entity Framework" }
    "KAN-73" = @{ desc = "Mejora: Consolidador de configuracion de OTEL" }
    "KAN-74" = @{ desc = "Mejora: Pipeline de testing E2E automatizado" }
    "KAN-75" = @{ desc = "Mejora: Dashboard de salud de servicios (health checks)" }
    "KAN-76" = @{ desc = "Mejora: Sistema de versionado de workflows" }
    "KAN-77" = @{ desc = "E2E: Verificacion completa del sistema end-to-end" }
    "KAN-78" = @{ desc = "Seleccion: Analisis de modulo WebCV legacy" }
    "KAN-79" = @{ desc = "Seleccion: Definir API de candidates y avisos" }
    "KAN-80" = @{ desc = "Evaluacion: Disenar modelo de datos para evaluaciones" }
    "KAN-81" = @{ desc = "Evaluacion: Definir workflow de evaluacion de desempeño" }
    "KAN-82" = @{ desc = "Capacitacion: Analisis de modulos de capacitacion existentes" }
    "KAN-83" = @{ desc = "Capacitacion: Disenar modelo de cursos y capacitaciones" }
    "KAN-84" = @{ desc = "Carrera: Analisis de Succession Planning" }
    "KAN-85" = @{ desc = "Carrera: Disenar modelo de talentos" }
    "KAN-86" = @{ desc = "Clima: Analisis de encuestas de clima laboral" }
    "KAN-87" = @{ desc = "Clima: Disenar modelo de campaigns de clima" }
    "KAN-91" = @{ desc = "Vacaciones: Backend service - saldo, solicitudes, approve/reject" }
    "KAN-92" = @{ desc = "Vacaciones: Integracion con Legajos - obtener datos de empleado" }
    "KAN-93" = @{ desc = "Vacaciones: Simulador de saldo con calculo de dias habiles" }
    "KAN-94" = @{ desc = "Vacaciones: Workflow avanzado con etapas configurables" }
    "KAN-95" = @{ desc = "Vacaciones: UI Portal Empleado con calendario y historial" }
    "KAN-96" = @{ desc = "Tesoreria: Analisis de modulo legacy" }
    "KAN-97" = @{ desc = "Tesoreria: Definir modelo de adelantos y pagos" }
    "KAN-98" = @{ desc = "Tesoreria: Definir modelo de conciliaciones" }
    "KAN-99" = @{ desc = "Presupuesto: Analisis de modulo legacy" }
    "KAN-100" = @{ desc = "Presupuesto: Definir modelo de headcount y costos" }
    "KAN-101" = @{ desc = "Beneficios: Analisis de modulo legacy" }
    "KAN-102" = @{ desc = "Beneficios: Definir modelo de catalogs e inscripciones" }
    "KAN-103" = @{ desc = "Accidentabilidad: Analisis de modulo legacy" }
    "KAN-104" = @{ desc = "Accidentabilidad: Definir modelo de incidentes e investigaciones" }
    "KAN-105" = @{ desc = "Seguridad: Analisis de modulo de seguridad laboral" }
    "KAN-106" = @{ desc = "Control Visitas: Analisis de modulo de control de visitantes" }
}

foreach ($key in $tasks.Keys) {
    $task = $tasks[$key]
    $body = '{
        "fields": {
            "description": {
                "type": "doc",
                "version": 1,
                "content": [{
                    "type": "paragraph",
                    "content": [{"type": "text", "text": "' + $task.desc + '"}]
                }]
            }
        }
    }'
    
    try {
        $null = Invoke-RestMethod -Uri "https://jonatanmveralopez.atlassian.net/rest/api/3/issue/$key" -Headers $headers -Method Put -Body $body
        Write-Output "Updated: $key"
    } catch {
        Write-Output "Failed: $key - $($_.Exception.Message)"
    }
}

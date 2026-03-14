# Automatizacion de Tareas en Jira - Nucleus RH

# ==============================================================================
# CONFIGURACION
# ==============================================================================

$JIRA_EMAIL = "jonatanm.veralopez@gmail.com"
$JIRA_TOKEN = $env:JIRA_TOKEN  # Set via environment variable
$JIRA_BASE = "https://jveralopez.atlassian.net"
$PROJECT_KEY = "KAN"

# Codificar credenciales
$cred = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("$JIRA_EMAIL`:$JIRA_TOKEN"))
$headers = @{
    Authorization="Basic $cred"
    'Content-Type'='application/json'
}

# ==============================================================================
# FUNCIONES
# ==============================================================================

function New-JiraTask {
    param(
        [Parameter(Mandatory=$true)]
        [string]$Summary,
        
        [Parameter(Mandatory=$false)]
        [string]$Description = "",
        
        [Parameter(Mandatory=$false)]
        [string]$Status = "Idea",  # Idea, PorHacer, EnCurso, EnRevision, Finalizado
        
        [Parameter(Mandatory=$false)]
        [string]$ParentKey = ""
    )
    
    # Mapping de estados
    $statusMap = @{
        "Idea" = "11"
        "PorHacer" = "21"
        "EnCurso" = "31"
        "EnRevision" = "41"
        "Finalizado" = "51"
    }
    
    $issueTypeId = if ($ParentKey -ne "") { "10002" } else { "10004" }  # Subtask or Task
    
    $body = @{
        fields = @{
            project = @{ key = $PROJECT_KEY }
            summary = $Summary
            description = @{
                type = "doc"
                version = 1
                content = @(
                    @{
                        type = "paragraph"
                        content = @(
                            @{
                                type = "text"
                                text = $Description
                            }
                        )
                    }
                )
            }
            issuetype = @{ id = $issueTypeId }
        }
    }
    
    if ($ParentKey -ne "") {
        $body.fields.parent = @{ key = $ParentKey }
    }
    
    $json = $body | ConvertTo-Json -Depth 5
    
    try {
        $result = Invoke-RestMethod -Uri "$JIRA_BASE/rest/api/3/issue" -Headers $headers -Method Post -Body $json
        Write-Host "Created: $($result.key) - $Summary" -ForegroundColor Green
        
        # Cambiar estado si no es Idea
        if ($Status -ne "Idea") {
            $transitionId = $statusMap[$Status]
            if ($transitionId) {
                Start-Sleep -Milliseconds 500
                $transBody = @{ transition = @{ id = $transitionId } } | ConvertTo-Json
                $null = Invoke-RestMethod -Uri "$JIRA_BASE/rest/api/3/issue/$($result.key)/transitions" -Headers $headers -Method Post -Body $transBody
                Write-Host "  Status: $Status" -ForegroundColor Cyan
            }
        }
        
        return $result.key
    }
    catch {
        Write-Host "Error creating task: $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

function Get-JiraTasks {
    param(
        [Parameter(Mandatory=$false)]
        [string]$Status = "",
        
        [Parameter(Mandatory=$false)]
        [string]$ParentKey = ""
    )
    
    $jql = "project=$PROJECT_KEY AND type=Task"
    if ($Status -ne "") {
        $statusId = @{
            "Idea" = "10000"
            "PorHacer" = "10001"
            "EnCurso" = "10002"
            "EnRevision" = "10003"
            "Finalizado" = "10004"
        }[$Status]
        $jql += " AND status=$statusId"
    }
    if ($ParentKey -ne "") {
        $jql += " AND parent=$ParentKey"
    }
    
    $result = Invoke-RestMethod -Uri "$JIRA_BASE/rest/api/3/search/jql?jql=[$jql]&maxResults=100&fields=key,summary,status" -Headers $headers -Method Get
    
    Write-Host "Found $($result.issues.Count) tasks" -ForegroundColor Yellow
    foreach ($issue in $result.issues) {
        Write-Host "  $($issue.key) | $($issue.fields.status.name) | $($issue.fields.summary)"
    }
}

# ==============================================================================
# EJEMPLOS DE USO
# ==============================================================================

# Para agregar una nueva tarea en Idea (nueva idea/mejora):
# New-JiraTask -Summary "Nueva funcionalidad X" -Description "Descripcion..." -Status "Idea"

# Para agregar una tarea por hacer (analizada/lista para ejecutar):
# New-JiraTask -Summary "Implementar modulo X" -Description "Descripcion..." -Status "PorHacer"

# Para agregar subtarea:
# New-JiraTask -Summary "Subtarea 1" -Description "Descripcion..." -Status "PorHacer" -ParentKey "KAN-60"

# Listar tareas por estado:
# Get-JiraTasks -Status "Idea"
# Get-JiraTasks -Status "PorHacer"

Write-Host "========================================" -ForegroundColor Magenta
Write-Host " Automatizacion Jira - Nucleus RH" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta
Write-Host ""
Write-Host "Funciones disponibles:" -ForegroundColor White
Write-Host "  New-JiraTask -Summary -Description -Status -ParentKey" -ForegroundColor Cyan
Write-Host "  Get-JiraTasks -Status -ParentKey" -ForegroundColor Cyan
Write-Host ""
Write-Host "Estados: Idea, PorHacer, EnCurso, EnRevision, Finalizado" -ForegroundColor Yellow
Write-Host ""

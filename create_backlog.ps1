$headers = @{
    Authorization='Basic am9uYXRhbm0udmVyYWxvcGV6QGdtYWlsLmNvbTpBVEFUVDN4RmZHRjBXb1dwakRTVXk0WElYRV9TVk1idEV1eUp4ekh2ZkJxbFlYU1F1b0ppaVJtMjhDSEdrc2xVcUswa3BQLUVZeU5oMHZqVVVVYVNKSkwyeHBxc285OHcwVEZFVXdYbWhFem9MSWNjREM2cHZWdzlsQld3NXRRekYweFMwWk9hZnVLcHBOSU5rdmp6RlBCTnlJbEp1OFBxWXFYVVlxMi1fZm1HLTNaY0hHUXhsLVE9QzkxQjI2QTE='
    'Content-Type'='application/json'
}

# Create Backlog Epic
$body = @{
    fields = @{
        project = @{ key = "KAN" }
        summary = "Backlog"
        description = @{ 
            type = "doc"
            version = 1
            content = @(
                @{
                    type = "paragraph"
                    content = @(
                        @{
                            type = "text"
                            text = "Tareas pendientes por ejecutar en proximos sprints"
                        }
                    )
                }
            )
        }
        issuetype = @{ id = "10000" }  # Epic
    }
} | ConvertTo-Json -Depth 5

$result = Invoke-RestMethod -Uri "https://jonatanmveralopez.atlassian.net/rest/api/3/issue" -Headers $headers -Method Post -Body $body
Write-Output "Created Backlog Epic: $($result.key)"

# Create Nueva Ley Laboral in Idea status
$body2 = @{
    fields = @{
        project = @{ key = "KAN" }
        summary = "Nueva Ley Laboral - Investigar impacto"
        description = @{ 
            type = "doc"
            version = 1
            content = @(
                @{
                    type = "paragraph"
                    content = @(
                        @{
                            type = "text"
                            text = "Investigar impacto de nueva ley laboral en el sistema de RRHH. Por analizar: cambios en liquidacion, licencias, contratos."
                        }
                    )
                }
            )
        }
        issuetype = @{ id = "10004" }  # Task
    }
} | ConvertTo-Json -Depth 5

$result2 = Invoke-RestMethod -Uri "https://jonatanmveralopez.atlassian.net/rest/api/3/issue" -Headers $headers -Method Post -Body $body2
Write-Output "Created Idea task: $($result2.key)"

# Transition to Idea (status 10000)
Start-Sleep -Milliseconds 500
$transBody = @{
    transition = @{ id = "11" }
} | ConvertTo-Json

$null = Invoke-RestMethod -Uri "https://jonatanmveralopez.atlassian.net/rest/api/3/issue/$($result2.key)/transitions" -Headers $headers -Method Post -Body $transBody
Write-Output "Moved $($result2.key) to Idea status"

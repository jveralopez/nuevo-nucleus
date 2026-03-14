$headers = @{
    Authorization='Basic am9uYXRhbm0udmVyYWxvcGV6QGdtYWlsLmNvbTpBVEFUVDN4RmZHRjBXb1dwakRTVXk0WElYRV9TVk1idEV1eUp4ekh2ZkJxbFlYU1F1b0ppaVJtMjhDSEdrc2xVcUswa3BQLUVZeU5oMHZqVVVVYVNKSkwyeHBxc285OHcwVEZFVXdYbWhFem9MSWNjREM2cHZWdzlsQld3NXRRekYweFMwWk9hZnVLcHBOSU5rdmp6RlBCTnlJbEp1OFBxWXFYVVlxMi1fZm1HLTNaY0hHUXhsLVE9QzkxQjI2QTE='
    'Content-Type'='application/json'
}

$sprints = @(
    @{ name = "Sprint 11"; description = "Desarrollo Sprint 11 - Módulos pendientes" },
    @{ name = "Sprint 12"; description = "Desarrollo Sprint 12 - Integraciones y mejoras" },
    @{ name = "Sprint 13"; description = "Desarrollo Sprint 13 - Testing y documentación final" }
)

foreach ($sprint in $sprints) {
    $body = '{
        "fields": {
            "project": {"key": "KAN"},
            "summary": "' + $sprint.name + '",
            "description": {
                "type": "doc",
                "version": 1,
                "content": [{
                    "type": "paragraph",
                    "content": [{"type": "text", "text": "' + $sprint.description + '"}]
                }]
            },
            "issuetype": {"id": "10001"}
        }
    }'
    
    try {
        $result = Invoke-RestMethod -Uri "https://jonatanmveralopez.atlassian.net/rest/api/3/issue" -Headers $headers -Method Post -Body $body
        Write-Output "Created: $($result.key) - $($sprint.name)"
    } catch {
        Write-Output "Failed to create $($sprint.name): $($_.Exception.Message)"
    }
}

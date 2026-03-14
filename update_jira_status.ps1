$headers = @{
    Authorization='Basic am9uYXRhbm0udmVyYWxvcGV6QGdtYWlsLmNvbTpBVEFUVDN4RmZHRjBXb1dwakRTVXk0WElYRV9TVk1idEV1eUp4ekh2ZkJxbFlYU1F1b0ppaVJtMjhDSEdrc2xVcUswa3BQLUVZeU5oMHZqVVVVYVNKSkwyeHBxc285OHcwVEZFVXdYbWhFem9MSWNjREM2cHZWdzlsQld3NXRRekYweFMwWk9hZnVLcHBOSU5rdmp6RlBCTnlJbEp1OFBxWXFYVVlxMi1fZm1HLTNaY0hHUXhsLVE9QzkxQjI2QTE='
    'Content-Type'='application/json'
}

# Get all issues
$allIssues = @()
$startAt = 0
$maxResults = 100

do {
    $result = Invoke-RestMethod -Uri "https://jonatanmveralopez.atlassian.net/rest/api/3/search/jql?jql=project%3DKAN&startAt=$startAt&maxResults=$maxResults&fields=key,status" -Headers $headers -Method Get
    $allIssues += $result.issues
    $startAt += $maxResults
} while ($result.total -gt $startAt)

Write-Output "Found $($allIssues.Count) issues"

$successCount = 0
$failCount = 0
$skippedCount = 0

foreach ($issue in $allIssues) {
    $key = $issue.key
    $status = $issue.fields.status.name
    $statusId = $issue.fields.status.id
    
    # Skip if already Finalizado
    if ($statusId -eq "10004") {
        Write-Output "[SKIP] $key already Finalizado"
        $skippedCount++
        continue
    }
    
    # Transition to Finalizado (id: 51)
    $body = @{
        transition = @{ id = "51" }
    } | ConvertTo-Json
    
    try {
        $null = Invoke-RestMethod -Uri "https://jonatanmveralopez.atlassian.net/rest/api/3/issue/$key/transitions" -Headers $headers -Method Post -Body $body
        Write-Output "[OK] $key -> Finalizado"
        $successCount++
    } catch {
        Write-Output "[FAIL] $key : $($_.Exception.Message)"
        $failCount++
    }
    
    Start-Sleep -Milliseconds 200
}

Write-Output ""
Write-Output "=== SUMMARY ==="
Write-Output "Success: $successCount"
Write-Output "Failed: $failCount"
Write-Output "Skipped: $skippedCount"

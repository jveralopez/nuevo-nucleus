$headers = @{
    Authorization='Basic am9uYXRhbm0udmVyYWxvcGV6QGdtYWlsLmNvbTpBVEFUVDN4RmZHRjBXb1dwakRTVXk0WElYRV9TVk1idEV1eUp4ekh2ZkJxbFlYU1F1b0ppaVJtMjhDSEdrc2xVcUswa3BQLUVZeU5oMHZqVVVVYVNKSkwyeHBxc285OHcwVEZFVXdYbWhFem9MSWNjREM2cHZWdzlsQld3NXRRekYweFMwWk9hZnVLcHBOSU5rdmp6RlBCTnlJbEp1OFBxWXFYVVlxMi1fZm1HLTNaY0hHUXhsLVE9QzkxQjI2QTE='
    'Content-Type'='application/json'
}

$allIssues = @()
$startAt = 0
$maxResults = 100

do {
    $result = Invoke-RestMethod -Uri "https://jonatanmveralopez.atlassian.net/rest/api/3/search/jql?jql=project%3DKAN&startAt=$startAt&maxResults=$maxResults&fields=key,summary,status,issuetype" -Headers $headers -Method Get
    $allIssues += $result.issues
    $startAt += $maxResults
} while ($result.total -gt $startAt)

Write-Output "Total issues: $($allIssues.Count)"
Write-Output ""
$allIssues | ForEach-Object { 
    Write-Output "$($_.key) | $($_.fields.status.name) | $($_.fields.issuetype.name) | $($_.fields.summary)"
}

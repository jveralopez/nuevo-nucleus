$headers = @{
    Authorization='Basic am9uYXRhbm0udmVyYWxvcGV6QGdtYWlsLmNvbTpBVEFUVDN4RmZHRjBXb1dwakRTVXk0WElYRV9TVk1idEV1eUp4ekh2ZkJxbFlYU1F1b0ppaVJtMjhDSEdrc2xVcUswa3BQLUVZeU5oMHZqVVVVYVNKSkwyeHBxc285OHcwVEZFVXdYbWhFem9MSWNjREM2cHZWdzlsQld3NXRRekYweFMwWk9hZnVLcHBOSU5rdmp6RlBCTnlJbEp1OFBxWXFYVVlxMi1fZm1HLTNaY0hHUXhsLVE9QzkxQjI2QTE='
    'Content-Type'='application/json'
}

$result = Invoke-RestMethod -Uri "https://jonatanmveralopez.atlassian.net/rest/api/3/search/jql?jql=project%3DKAN%20AND%20type%3DTarea&maxResults=100&fields=key,summary" -Headers $headers -Method Get
$result.issues | ForEach-Object { Write-Output $_.key }

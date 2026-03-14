$email = "jonatanm.veralopez@gmail.com"
$token = $env:JIRA_TOKEN  # Set via environment variable
$cred = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("$email`:$token"))
Write-Output $cred

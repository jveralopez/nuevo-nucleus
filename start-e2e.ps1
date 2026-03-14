$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "Starting docker compose stack..."
& docker compose -f "$root\docker-compose.prod.yml" up -d

Write-Host "Starting local UI servers..."
$rh = Start-Process -FilePath python -ArgumentList "-m http.server 3001" -WorkingDirectory "$root\portal-rh-ui" -PassThru -WindowStyle Hidden
$empleado = Start-Process -FilePath python -ArgumentList "-m http.server 3002" -WorkingDirectory "$root\portal-empleado-ui" -PassThru -WindowStyle Hidden

try {
  if (!(Test-Path "$root\node_modules")) {
    Write-Host "Installing npm dependencies..."
    Push-Location $root
    & npm install
    Pop-Location
  }

  Write-Host "Installing Playwright browsers..."
  Push-Location $root
  & npx playwright install

  Write-Host "Running E2E tests..."
  & npm run test:e2e
  Pop-Location
}
finally {
  Write-Host "Stopping local UI servers..."
  if ($rh -and !$rh.HasExited) { Stop-Process -Id $rh.Id -Force }
  if ($empleado -and !$empleado.HasExited) { Stop-Process -Id $empleado.Id -Force }
}

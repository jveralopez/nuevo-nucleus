@echo off
setlocal

set ROOT=%~dp0
set ASPNETCORE_ENVIRONMENT=Development

echo Starting RRHH stack...
start "auth-service" cmd /k "cd /d %ROOT%auth-service && "C:\Program Files\dotnet\dotnet.exe" run"
start "organizacion-service" cmd /k "cd /d %ROOT%organizacion-service && "C:\Program Files\dotnet\dotnet.exe" run"
start "personal-service" cmd /k "cd /d %ROOT%personal-service && "C:\Program Files\dotnet\dotnet.exe" run"
start "nucleuswf-service" cmd /k "cd /d %ROOT%nucleuswf-service && "C:\Program Files\dotnet\dotnet.exe" run"
start "liquidacion-service" cmd /k "cd /d %ROOT%liquidacion-service && "C:\Program Files\dotnet\dotnet.exe" run"
start "integration-hub-service" cmd /k "cd /d %ROOT%integration-hub-service && "C:\Program Files\dotnet\dotnet.exe" run"
start "portal-bff-service" cmd /k "cd /d %ROOT%portal-bff-service && "C:\Program Files\dotnet\dotnet.exe" run"
start "configuracion-service" cmd /k "cd /d %ROOT%configuracion-service && "C:\Program Files\dotnet\dotnet.exe" run"

echo Waiting for auth-service...
powershell -NoProfile -Command "for ($i=0; $i -lt 30; $i++) { try { $resp = Invoke-WebRequest -UseBasicParsing http://localhost:5001/health; if ($resp.StatusCode -eq 200) { exit 0 } } catch {} Start-Sleep -Seconds 1 } exit 1"
if errorlevel 1 (
  echo auth-service not ready. Please check the window logs.
  goto :done
)

echo Creating demo admin user and token...
powershell -NoProfile -Command "$login = Invoke-WebRequest -UseBasicParsing http://localhost:5001/login -Method POST -ContentType 'application/json' -Body '{\"username\":\"admin\",\"password\":\"admin123\"}'; $token = ($login.Content | ConvertFrom-Json).token; $body = '{\"username\":\"demo_admin\",\"password\":\"demo123\",\"role\":\"Admin\",\"estado\":\"Activo\"}'; try { Invoke-WebRequest -UseBasicParsing http://localhost:5001/users -Method POST -ContentType 'application/json' -Headers @{Authorization = "Bearer $token"} -Body $body | Out-Null } catch { }; $token | Out-File -Encoding ascii "%ROOT%rrhh_token.txt""

:done
echo RRHH stack up. Token guardado en rrhh_token.txt
for /f "usebackq delims=" %%t in ("%ROOT%rrhh_token.txt") do set TOKEN=%%t
start "" "%ROOT%portal-rh-ui\index.html?token=%TOKEN%"
endlocal

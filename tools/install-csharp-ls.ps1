$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$env:PATH = (Join-Path $env:USERPROFILE ".dotnet") + ";" + (Join-Path $env:USERPROFILE ".dotnet\tools") + ";" + $env:PATH

$dst = Join-Path $env:TEMP "csharp-ls-src"
if (Test-Path $dst) { Remove-Item $dst -Recurse -Force }

$zip = Join-Path $env:TEMP "csharp-ls.zip"
Invoke-WebRequest https://github.com/razzmatazz/csharp-language-server/archive/refs/tags/0.22.0.zip -OutFile $zip
Expand-Archive -Path $zip -DestinationPath $env:TEMP

$src = Join-Path $env:TEMP "csharp-language-server-0.22.0"
Rename-Item $src $dst

$changelogPath = Join-Path $dst "src\CSharpLanguageServer\CHANGELOG.md"
@"
# Changelog

## [Unreleased]
"@ | Set-Content -Path $changelogPath -Encoding UTF8

Set-Location $dst
dotnet build -c Release

$outDir = Join-Path $dst "out"
if (Test-Path $outDir) { Remove-Item $outDir -Recurse -Force }
New-Item -ItemType Directory -Path $outDir | Out-Null
Copy-Item -Recurse -Force (Join-Path $dst "src\CSharpLanguageServer\bin\Release\net10.0\*") $outDir

$toolDir = Join-Path $env:USERPROFILE ".dotnet\tools"
$cmdPath = Join-Path $toolDir "csharp-ls.cmd"
$dllPath = Join-Path $outDir "CSharpLanguageServer.dll"
$dotnetExe = Join-Path $env:USERPROFILE ".dotnet\dotnet.exe"

$cmdContent = "@echo off`r`n" + '"' + $dotnetExe + '" "' + $dllPath + '" %*' + "`r`n"
Set-Content -Path $cmdPath -Value $cmdContent -Encoding ASCII

Write-Output "csharp-ls instalado en $cmdPath"

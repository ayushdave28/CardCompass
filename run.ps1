$env:PATH = "$env:USERPROFILE\.dotnet;" + $env:PATH
Set-Location -Path "$PSScriptRoot\CardCompass.Web"

Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "  Starting CardCompass Indian Credit Card Platform" -ForegroundColor Cyan
Write-Host "  Opening on: http://localhost:5079" -ForegroundColor Green
Write-Host "===================================================" -ForegroundColor Cyan

dotnet run --launch-profile "http"

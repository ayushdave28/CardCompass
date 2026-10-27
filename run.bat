@echo off
set "PATH=%USERPROFILE%\.dotnet;%PATH%"
cd /d "%~dp0CardCompass.Web"
echo ===================================================
echo   Starting CardCompass Indian Credit Card Platform
echo   Opening on: http://localhost:5079
echo ===================================================
dotnet run --launch-profile "http"
pause

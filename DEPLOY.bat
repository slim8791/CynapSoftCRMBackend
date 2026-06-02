@echo off
REM CynapCRM Gateway - Monster ASP Deployment Script
REM This script builds and publishes the gateway for Monster ASP

echo.
echo ================================================
echo  CynapCRM Gateway - Monster ASP Deployment
echo ================================================
echo.

REM Check if dotnet is installed
where dotnet >nul 2>nul
if %errorlevel% neq 0 (
    echo ERROR: dotnet CLI not found. Please install .NET SDK.
    exit /b 1
)

echo [1/4] Cleaning previous builds...
rmdir /s /q bin 2>nul
rmdir /s /q obj 2>nul
rmdir /s /q publish 2>nul
echo       ✓ Clean complete

echo.
echo [2/4] Building in Release mode...
dotnet build -c Release
if %errorlevel% neq 0 (
    echo ERROR: Build failed!
    exit /b 1
)
echo       ✓ Build successful

echo.
echo [3/4] Publishing for Monster ASP...
dotnet publish -c Release -o ./publish
if %errorlevel% neq 0 (
    echo ERROR: Publish failed!
    exit /b 1
)
echo       ✓ Publish successful

echo.
echo [4/4] Generating deployment information...

REM Count files
for /f %%A in ('dir /b publish ^| find /c /v ""') do set filecount=%%A
for /f %%A in ('powershell -Command "(Get-ChildItem -Path publish -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB"') do set totalsize=%%A

echo       ✓ Files: %filecount%
echo       ✓ Size: %totalsize% MB

echo.
echo ================================================
echo  ✅ DEPLOYMENT PACKAGE READY
echo ================================================
echo.
echo Location: .\publish\
echo.
echo Next Steps:
echo   1. Connect to Monster ASP via FTP
echo   2. Upload all files from .\publish\ directory
echo   3. Configure .NET 10 runtime on Monster ASP
echo   4. Test the gateway at your Monster ASP URL
echo.
echo Endpoints configured:
echo   - Auth:      http://cynapharmauth.runasp.net
echo   - Products:  http://cynapharmproducts.runasp.net
echo   - Docs:      http://cynapharmdocs.runasp.net
echo   - Fields:    http://cynapharmfields.runasp.net
echo   - Inventory: http://cynapharminventories.runasp.net
echo   - Orders:    http://cynapharmorders.tryasp.net ⚠️
echo.
echo Routes: 69 total
echo Token Required: All protected routes (Bearer token)
echo CORS: Enabled for all origins (update for production)
echo.
pause

#!/bin/bash
# Quick Configuration Helper for Samsung A55 Deployment

Write-Host "=== CYNAPHARM MOBILE - Samsung A55 Configuration ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Get PC IP
Write-Host "Step 1: Finding your PC IP address..." -ForegroundColor Yellow
$ipInfo = ipconfig | Select-String "IPv4 Address" | Select-String "192.168|10\."
if ($ipInfo) {
    Write-Host $ipInfo
    $pcIp = Read-Host "Enter your PC IPv4 Address (e.g., 192.168.1.45)"
} else {
    Write-Host "No IPv4 address found. Running full ipconfig..." -ForegroundColor Red
    ipconfig
    $pcIp = Read-Host "Enter your PC IPv4 Address manually"
}

# Step 2: Validate IP format
$ipPattern = '^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$'
if (-not ($pcIp -match $ipPattern)) {
    Write-Host "Invalid IP format! Expected format: 192.168.1.45" -ForegroundColor Red
    exit 1
}

Write-Host "IP Address: $pcIp" -ForegroundColor Green

# Step 3: Check if file exists
$mauiProgramPath = ".\Cynapharm-Mobile\MauiProgram.cs"
if (-not (Test-Path $mauiProgramPath)) {
    Write-Host "Error: MauiProgram.cs not found at $mauiProgramPath" -ForegroundColor Red
    exit 1
}

# Step 4: Update the IP in MauiProgram.cs
Write-Host ""
Write-Host "Step 2: Updating MauiProgram.cs with IP $pcIp..." -ForegroundColor Yellow

$content = Get-Content $mauiProgramPath -Raw
$newContent = $content -replace 'var baseUrl = "https://192\.168\.\d+\.\d+:7777/";', "var baseUrl = ""https://$pcIp:7777/"";"
$newContent | Set-Content $mauiProgramPath

Write-Host "✅ MauiProgram.cs updated successfully!" -ForegroundColor Green

# Step 5: Check API is running
Write-Host ""
Write-Host "Step 3: Checking if API Gateway is running..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest "https://localhost:7777/api/health" -SkipCertificateCheck -ErrorAction Stop
    Write-Host "✅ API is running on https://localhost:7777" -ForegroundColor Green
} catch {
    Write-Host "⚠️  API Gateway is not responding!" -ForegroundColor Red
    Write-Host "Start it with: cd CynapCRM.Gateway && dotnet run --launch-profile https" -ForegroundColor Yellow
}

# Step 6: Check device connection
Write-Host ""
Write-Host "Step 4: Checking device connection..." -ForegroundColor Yellow
$devices = adb devices 2>$null | Select-Object -Skip 1
if ($devices -match "device") {
    Write-Host "✅ Device found!" -ForegroundColor Green
    $devices
} else {
    Write-Host "⚠️  No devices found!" -ForegroundColor Red
    Write-Host "Reconnect your Samsung A55 and enable USB Debugging" -ForegroundColor Yellow
}

# Step 7: Offer to rebuild
Write-Host ""
Write-Host "Step 5: Ready to rebuild?" -ForegroundColor Yellow
$rebuild = Read-Host "Rebuild project now? (y/n)"
if ($rebuild -eq "y") {
    Write-Host "Cleaning and rebuilding..." -ForegroundColor Yellow
    dotnet clean -f net10.0-android
    dotnet build -f net10.0-android -c Debug
    Write-Host "✅ Build complete! Redeploy from Visual Studio (F5)" -ForegroundColor Green
}

Write-Host ""
Write-Host "=== Configuration Complete ===" -ForegroundColor Cyan
Write-Host "Summary:" -ForegroundColor Green
Write-Host "  PC IP: $pcIp"
Write-Host "  API URL: https://$pcIp:7777/"
Write-Host ""
Write-Host "Next: Connect your device and press F5 in Visual Studio" -ForegroundColor Yellow


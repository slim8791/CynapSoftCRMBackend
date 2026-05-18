# CynapCRM Gateway - Monster ASP Deployment Script (PowerShell)
# This script builds and publishes the gateway for Monster ASP

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host " CynapCRM Gateway - Monster ASP Deployment" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Check if dotnet is installed
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK found: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ ERROR: dotnet CLI not found. Please install .NET SDK." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "[1/4] Cleaning previous builds..." -ForegroundColor Yellow
Remove-Item -Path "bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "obj" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "publish" -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "      ✓ Clean complete" -ForegroundColor Green

Write-Host ""
Write-Host "[2/4] Building in Release mode..." -ForegroundColor Yellow
dotnet build -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ ERROR: Build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "      ✓ Build successful" -ForegroundColor Green

Write-Host ""
Write-Host "[3/4] Publishing for Monster ASP..." -ForegroundColor Yellow
dotnet publish -c Release -o ./publish
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ ERROR: Publish failed!" -ForegroundColor Red
    exit 1
}
Write-Host "      ✓ Publish successful" -ForegroundColor Green

Write-Host ""
Write-Host "[4/4] Generating deployment information..." -ForegroundColor Yellow

$publishDir = Get-Item "publish"
$fileCount = (Get-ChildItem -Path "publish" -Recurse | Measure-Object).Count
$totalSize = "{0:N2}" -f ((Get-ChildItem -Path "publish" -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB)

Write-Host "      ✓ Files: $fileCount" -ForegroundColor Green
Write-Host "      ✓ Size: $totalSize MB" -ForegroundColor Green

Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host " ✅ DEPLOYMENT PACKAGE READY" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""
Write-Host "Location: .\publish\" -ForegroundColor Cyan
Write-Host ""
Write-Host "📋 Next Steps:" -ForegroundColor Cyan
Write-Host "   1. Connect to Monster ASP via FTP" -ForegroundColor White
Write-Host "   2. Upload all files from .\publish\ directory" -ForegroundColor White
Write-Host "   3. Configure .NET 10 runtime on Monster ASP" -ForegroundColor White
Write-Host "   4. Test the gateway at your Monster ASP URL" -ForegroundColor White
Write-Host ""
Write-Host "🔗 Endpoints Configured:" -ForegroundColor Cyan
Write-Host "   - Auth:      http://cynapharmauth.runasp.net" -ForegroundColor Gray
Write-Host "   - Products:  http://cynapharmproducts.runasp.net" -ForegroundColor Gray
Write-Host "   - Docs:      http://cynapharmdocs.runasp.net" -ForegroundColor Gray
Write-Host "   - Fields:    http://cynapharmfields.runasp.net" -ForegroundColor Gray
Write-Host "   - Inventory: http://cynapharminventories.runasp.net" -ForegroundColor Gray
Write-Host "   - Orders:    http://cynapharmorders.tryasp.net" -ForegroundColor Yellow
Write-Host "                                          (⚠️  Different domain)" -ForegroundColor Yellow
Write-Host ""
Write-Host "📊 Configuration Summary:" -ForegroundColor Cyan
Write-Host "   - Total Routes: 69" -ForegroundColor Gray
Write-Host "   - Protocol: HTTP (port 80)" -ForegroundColor Gray
Write-Host "   - HTTPS: Handled by Monster ASP" -ForegroundColor Gray
Write-Host "   - Authentication: Bearer Token (JWT)" -ForegroundColor Gray
Write-Host "   - CORS: All origins (update for production)" -ForegroundColor Yellow
Write-Host ""
Write-Host "🧪 Quick Test Command:" -ForegroundColor Cyan
Write-Host "   curl https://your-gateway-url.com/" -ForegroundColor Gray
Write-Host ""

# Offer to open publish directory
$response = Read-Host "Open publish directory? (Y/n)"
if ($response -ne "n") {
    Invoke-Item ".\publish"
}

Write-Host ""
Write-Host "Happy deploying! 🚀" -ForegroundColor Cyan
Write-Host ""

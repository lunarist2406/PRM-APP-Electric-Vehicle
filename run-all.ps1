# Run All Services

Write-Host "🚀 Starting all services..." -ForegroundColor Green

# Start UserService
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd Services\UserService; dotnet run; pause"
Start-Sleep -Seconds 2

# Start VehicleService
Start-Process dotnet -ArgumentList "run" -WorkingDirectory "Services\VehicleService"
Start-Sleep -Seconds 2

# Start SubscriptionService
Start-Process dotnet -ArgumentList "run" -WorkingDirectory "Services\SubscriptionService"
Start-Sleep -Seconds 2

# Start ApiGateway
Start-Process dotnet -ArgumentList "run" -WorkingDirectory "ApiGateway"

Write-Host "✅ All services started!" -ForegroundColor Green
Write-Host "📍 Swagger: http://localhost:5230" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")


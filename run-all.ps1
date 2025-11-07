# Run All Services

Write-Host "Cleaning up ports..." -ForegroundColor Yellow
& ".\kill-ports.ps1"
Start-Sleep -Seconds 2

Write-Host "Starting all services..." -ForegroundColor Green

# Start UserService
Write-Host "Starting UserService..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd Services\UserService; dotnet run; pause"
Start-Sleep -Seconds 3

# Start StationService (port 5002) - MUST START FIRST for other services
Write-Host "Starting StationService..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd Services\StationService; dotnet run; pause"
Start-Sleep -Seconds 3

# Start VehicleService
Write-Host "Starting VehicleService..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd Services\VehicleService; dotnet run; pause"
Start-Sleep -Seconds 2

# Start SubscriptionService
Write-Host "Starting SubscriptionService..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd Services\SubscriptionService; dotnet run; pause"
Start-Sleep -Seconds 2

# Start BookingService
Write-Host "Starting BookingService..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd Services\BookingService; dotnet run; pause"
Start-Sleep -Seconds 2

# Start PaymentService
Write-Host "Starting PaymentService..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd Services\PaymentService; dotnet run; pause"
Start-Sleep -Seconds 2

# Start ChargingPointService
Write-Host "Starting ChargingPointService..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd Services\ChargingPointService; dotnet run; pause"
Start-Sleep -Seconds 2

# Start CompanyService
Write-Host "Starting CompanyService..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd Services\CompanyService; dotnet run; pause"
Start-Sleep -Seconds 2

# Start AIService
Write-Host "Starting AIService..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd Services\AIService; dotnet run; pause"
Start-Sleep -Seconds 2

# Start ApiGateway
Write-Host "Starting ApiGateway..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd ApiGateway; dotnet run; pause"

Write-Host ""
Write-Host "All services started!" -ForegroundColor Green
Write-Host "ApiGateway Swagger: http://localhost:5230" -ForegroundColor Cyan
Write-Host ""
Write-Host "Note: Make sure to set environment variables:" -ForegroundColor Yellow
Write-Host "   - STATION_API_URL=http://localhost:5002" -ForegroundColor Yellow
Write-Host "   - VEHICLE_SERVICE_URL=http://localhost:5003" -ForegroundColor Yellow
Write-Host ""
Write-Host "Press Enter to exit..."
Read-Host


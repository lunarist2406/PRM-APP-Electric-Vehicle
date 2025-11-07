# Kill processes using common service ports
Write-Host "Cleaning up ports..." -ForegroundColor Yellow

$ports = @(5000, 5001, 5002, 5003, 5004, 5005, 5006, 5007, 5008, 5009, 5010, 5011, 5012, 5230)

foreach ($port in $ports) {
    $connections = netstat -ano | findstr ":$port" | findstr "LISTENING"
    if ($connections) {
        $lines = $connections -split "`n"
        foreach ($line in $lines) {
            if ($line -match '\s+(\d+)$') {
                $pid = $matches[1]
                Write-Host "Killing process $pid on port $port..." -ForegroundColor Red
                taskkill /PID $pid /F 2>$null
            }
        }
    }
}

Write-Host "Done!" -ForegroundColor Green


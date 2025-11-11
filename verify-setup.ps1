# Script de Verificación - Control Combustible Docker Setup
# Ejecutar después de docker-compose up -d

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "  Control Combustible - Verificación Docker" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

# Función para verificar un endpoint
function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Url
    )
    
    try {
        $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
        Write-Host "✓ $Name" -ForegroundColor Green -NoNewline
        Write-Host " - Status: $($response.StatusCode)" -ForegroundColor Gray
        return $true
    } catch {
        Write-Host "✗ $Name" -ForegroundColor Red -NoNewline
        Write-Host " - Error: $($_.Exception.Message)" -ForegroundColor Gray
        return $false
    }
}

# 1. Verificar Docker
Write-Host "1. Verificando Docker..." -ForegroundColor Yellow
$dockerInfo = docker info 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✓ Docker está corriendo" -ForegroundColor Green
} else {
    Write-Host "   ✗ Docker no está corriendo" -ForegroundColor Red
    exit 1
}
Write-Host ""

# 2. Verificar contenedores
Write-Host "2. Estado de los contenedores:" -ForegroundColor Yellow
$containers = docker-compose ps --format json | ConvertFrom-Json
if ($containers) {
    $running = 0
    $total = 0
    foreach ($container in $containers) {
        $total++
        if ($container.State -eq "running") {
            $running++
            Write-Host "   ✓" -ForegroundColor Green -NoNewline
        } else {
            Write-Host "   ✗" -ForegroundColor Red -NoNewline
        }
        Write-Host " $($container.Service) - $($container.State)"
    }
    Write-Host "   Total: $running/$total contenedores corriendo" -ForegroundColor Cyan
} else {
    Write-Host "   ✗ No se encontraron contenedores" -ForegroundColor Red
}
Write-Host ""

# 3. Verificar bases de datos
Write-Host "3. Verificando bases de datos:" -ForegroundColor Yellow
$databases = @(
    @{Name="MySQL Auth"; Container="mysql_auth"; Database="AuthDB"},
    @{Name="MySQL Vehicles"; Container="mysql_vehicles"; Database="VehiclesDB"},
    @{Name="MySQL Drivers"; Container="mysql_drivers"; Database="DriversDB"},
    @{Name="MySQL Routes"; Container="mysql_routes"; Database="RoutesDB"},
    @{Name="MySQL Fuel"; Container="mysql_fuel"; Database="FuelDB"}
)

$dbSuccess = 0
foreach ($db in $databases) {
    $result = docker exec $db.Container mysql -uroot -pdavidgiler21 -e "SELECT 1" 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✓ $($db.Name) ($($db.Database))" -ForegroundColor Green
        $dbSuccess++
    } else {
        Write-Host "   ✗ $($db.Name) ($($db.Database))" -ForegroundColor Red
    }
}
Write-Host "   Total: $dbSuccess/$($databases.Count) bases de datos disponibles" -ForegroundColor Cyan
Write-Host ""

# 4. Verificar servicios HTTP/Health
Write-Host "4. Verificando servicios (Health Checks):" -ForegroundColor Yellow
$services = @(
    @{Name="API Gateway"; Url="http://localhost:5000"},
    @{Name="MS Autenticacion"; Url="http://localhost:5001"},
    @{Name="MS Vehiculos"; Url="http://localhost:5135"},
    @{Name="MS Choferes"; Url="http://localhost:5133"},
    @{Name="MS Rutas"; Url="http://localhost:5174"},
    @{Name="MS Combustible"; Url="http://localhost:5136"}
)

$serviceSuccess = 0
foreach ($service in $services) {
    Write-Host "   " -NoNewline
    $healthUrl = "$($service.Url)/health"
    if (Test-Endpoint -Name $service.Name -Url $healthUrl) {
        $serviceSuccess++
    }
}
Write-Host "   Total: $serviceSuccess/$($services.Count) servicios respondiendo" -ForegroundColor Cyan
Write-Host ""

# 5. Verificar conectividad de red interna
Write-Host "5. Verificando red interna (ping entre contenedores):" -ForegroundColor Yellow
$pingTests = @(
    @{From="apigateway"; To="autenticacion"},
    @{From="apigateway"; To="mysql_auth"},
    @{From="ms_autenticacion"; To="mysql_auth"}
)

$pingSuccess = 0
foreach ($test in $pingTests) {
    $result = docker exec $test.From ping -c 1 $test.To 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✓ $($test.From) → $($test.To)" -ForegroundColor Green
        $pingSuccess++
    } else {
        Write-Host "   ✗ $($test.From) → $($test.To)" -ForegroundColor Red
    }
}
Write-Host "   Total: $pingSuccess/$($pingTests.Count) conexiones exitosas" -ForegroundColor Cyan
Write-Host ""

# 6. Verificar uso de recursos
Write-Host "6. Uso de recursos:" -ForegroundColor Yellow
$stats = docker stats --no-stream --format "table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}" | Select-Object -Skip 1
Write-Host $stats
Write-Host ""

# Resumen final
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "  RESUMEN" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

$totalTests = $databases.Count + $services.Count
$totalSuccess = $dbSuccess + $serviceSuccess

if ($totalSuccess -eq $totalTests) {
    Write-Host "✓ Todo está funcionando correctamente!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Accede a los servicios en:" -ForegroundColor Cyan
    Write-Host "  - API Gateway:    http://localhost:5000" -ForegroundColor White
    Write-Host "  - Autenticacion:  http://localhost:5001" -ForegroundColor White
    Write-Host "  - Vehiculos:      http://localhost:5135" -ForegroundColor White
    Write-Host "  - Choferes:       http://localhost:5133" -ForegroundColor White
    Write-Host "  - Rutas:          http://localhost:5174" -ForegroundColor White
    Write-Host "  - Combustible:    http://localhost:5136" -ForegroundColor White
} else {
    Write-Host "⚠ Hay problemas con algunos servicios" -ForegroundColor Yellow
    Write-Host "Servicios OK: $totalSuccess/$totalTests" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Sugerencias:" -ForegroundColor Cyan
    Write-Host "  1. Ver logs: docker-compose logs -f" -ForegroundColor White
    Write-Host "  2. Reiniciar servicios: docker-compose restart" -ForegroundColor White
    Write-Host "  3. Reconstruir: docker-compose build --no-cache" -ForegroundColor White
}

Write-Host ""
Write-Host "Para ver logs en tiempo real: docker-compose logs -f" -ForegroundColor Gray
Write-Host "Para detener servicios: docker-compose down" -ForegroundColor Gray
Write-Host ""

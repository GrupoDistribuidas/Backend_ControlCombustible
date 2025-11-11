# Script para agregar el paquete de Health Checks a todos los microservicios
# Ejecutar desde la raíz del proyecto Backend_ControlCombustible

Write-Host "Agregando paquetes de Health Checks a los microservicios..." -ForegroundColor Cyan

$microservicios = @(
    "MS.Autenticacion",
    "MS.Vehiculos",
    "MS.Choferes",
    "MS.Rutas",
    "MS.Combustible",
    "ApiGateway"
)

foreach ($ms in $microservicios) {
    Write-Host "`nAgregando paquetes a $ms..." -ForegroundColor Yellow
    
    Push-Location $ms
    
    # Agregar el paquete de Health Checks para MySQL (excepto ApiGateway)
    if ($ms -ne "ApiGateway") {
        Write-Host "  - AspNetCore.HealthChecks.MySql" -ForegroundColor Gray
        dotnet add package AspNetCore.HealthChecks.MySql --version 8.0.1
    }
    
    # Agregar el paquete base de Health Checks (para todos)
    Write-Host "  - Microsoft.Extensions.Diagnostics.HealthChecks" -ForegroundColor Gray
    dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks --version 8.0.0
    
    Pop-Location
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ Paquetes agregados exitosamente a $ms" -ForegroundColor Green
    } else {
        Write-Host "  ✗ Error al agregar paquetes a $ms" -ForegroundColor Red
    }
}

Write-Host "`n✓ Proceso completado!" -ForegroundColor Green
Write-Host "Ahora puedes construir y ejecutar con Docker:" -ForegroundColor Cyan
Write-Host "  docker-compose build" -ForegroundColor White
Write-Host "  docker-compose up -d" -ForegroundColor White

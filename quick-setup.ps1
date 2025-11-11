# Script de Inicio Rapido - Probar Configuracion Actualizada
# Este script instala los paquetes necesarios y verifica que todo compile

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "  Configuracion Rapida - Control Combustible" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Este script hara lo siguiente:" -ForegroundColor Yellow
Write-Host "1. Instalar paquetes NuGet de Health Checks" -ForegroundColor White
Write-Host "2. Restaurar dependencias" -ForegroundColor White
Write-Host "3. Compilar todos los proyectos" -ForegroundColor White
Write-Host "4. Verificar que no hay errores" -ForegroundColor White
Write-Host ""

$confirm = Read-Host "Continuar? (s/n)"
if ($confirm -ne "s" -and $confirm -ne "S") {
    Write-Host "Operacion cancelada" -ForegroundColor Yellow
    exit 0
}

Write-Host "`n=============================================" -ForegroundColor Cyan
Write-Host "PASO 1: Instalando paquetes Health Checks" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

$microservicios = @(
    "MS.Autenticacion",
    "MS.Vehiculos",
    "MS.Choferes",
    "MS.Rutas",
    "MS.Combustible",
    "ApiGateway"
)

$errors = 0

foreach ($ms in $microservicios) {
    Write-Host "`n Procesando $ms..." -ForegroundColor Yellow
    
    Push-Location $ms
    
    try {
        if ($ms -ne "ApiGateway") {
            Write-Host "   Instalando AspNetCore.HealthChecks.MySql..." -ForegroundColor Gray
            dotnet add package AspNetCore.HealthChecks.MySql --version 8.0.1 2>&1 | Out-Null
            
            if ($LASTEXITCODE -ne 0) {
                Write-Host "   Nota: El paquete puede ya estar instalado" -ForegroundColor Yellow
            }
        }
        
        Write-Host "   Instalando Microsoft.Extensions.Diagnostics.HealthChecks..." -ForegroundColor Gray
        dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks --version 8.0.0 2>&1 | Out-Null
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "   Nota: El paquete puede ya estar instalado" -ForegroundColor Yellow
        }
        
        Write-Host "   Paquetes instalados en $ms" -ForegroundColor Green
    }
    catch {
        Write-Host "   Error instalando paquetes en $ms" -ForegroundColor Red
        $errors++
    }
    
    Pop-Location
}

Write-Host "`n=============================================" -ForegroundColor Cyan
Write-Host "PASO 2: Restaurando dependencias" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

Write-Host "`nRestaurando solucion..." -ForegroundColor Yellow
dotnet restore ControlCombustible.sln

if ($LASTEXITCODE -eq 0) {
    Write-Host "Dependencias restauradas" -ForegroundColor Green
} else {
    Write-Host "Error al restaurar dependencias" -ForegroundColor Red
    $errors++
}

Write-Host "`n=============================================" -ForegroundColor Cyan
Write-Host "PASO 3: Compilando proyectos" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

Write-Host "`nCompilando solucion..." -ForegroundColor Yellow
dotnet build ControlCombustible.sln --no-restore

if ($LASTEXITCODE -eq 0) {
    Write-Host "Compilacion exitosa" -ForegroundColor Green
} else {
    Write-Host "Error en la compilacion" -ForegroundColor Red
    $errors++
}

Write-Host "`n=============================================" -ForegroundColor Cyan
Write-Host "RESUMEN" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

if ($errors -eq 0) {
    Write-Host "`nTodo configurado correctamente!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Proximos pasos:" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "1. Construir imagenes Docker:" -ForegroundColor White
    Write-Host "   docker-compose build" -ForegroundColor Gray
    Write-Host ""
    Write-Host "2. Iniciar servicios:" -ForegroundColor White
    Write-Host "   docker-compose up -d" -ForegroundColor Gray
    Write-Host ""
    Write-Host "3. Verificar que todo funcione:" -ForegroundColor White
    Write-Host "   .\verify-setup.ps1" -ForegroundColor Gray
    Write-Host ""
    Write-Host "O usa el script interactivo:" -ForegroundColor White
    Write-Host "   .\docker-start.ps1" -ForegroundColor Gray
    Write-Host ""
} else {
    Write-Host "`nSe encontraron $errors error(es)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Revisa los mensajes de error arriba." -ForegroundColor Yellow
    Write-Host "Es posible que necesites:" -ForegroundColor White
    Write-Host "  - Verificar tu conexion a internet" -ForegroundColor Gray
    Write-Host "  - Limpiar cache de NuGet: dotnet nuget locals all --clear" -ForegroundColor Gray
    Write-Host "  - Restaurar manualmente: dotnet restore" -ForegroundColor Gray
    Write-Host ""
}

Write-Host "Para mas informacion, revisa:" -ForegroundColor Cyan
Write-Host "  - CHANGES-SUMMARY.md    (Resumen de cambios)" -ForegroundColor White
Write-Host "  - README-DOCKER.md      (Guia completa de Docker)" -ForegroundColor White
Write-Host "  - MIGRATION-GUIDE.md    (Guia de migracion)" -ForegroundColor White
Write-Host ""

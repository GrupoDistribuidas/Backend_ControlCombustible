# Script de inicio para Docker - Control Combustible Backend
# Este script facilita el despliegue del sistema completo

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Control Combustible - Docker Setup" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Verificar si Docker está instalado
Write-Host "Verificando Docker..." -ForegroundColor Yellow
$dockerInstalled = Get-Command docker -ErrorAction SilentlyContinue
if (-not $dockerInstalled) {
    Write-Host "ERROR: Docker no está instalado o no está en el PATH" -ForegroundColor Red
    Write-Host "Por favor, instala Docker Desktop desde: https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
    exit 1
}
Write-Host "✓ Docker está instalado" -ForegroundColor Green

# Verificar si Docker está corriendo
Write-Host "Verificando si Docker está corriendo..." -ForegroundColor Yellow
$dockerRunning = docker info 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Docker no está corriendo" -ForegroundColor Red
    Write-Host "Por favor, inicia Docker Desktop" -ForegroundColor Yellow
    exit 1
}
Write-Host "✓ Docker está corriendo" -ForegroundColor Green
Write-Host ""

# Menú de opciones
Write-Host "Selecciona una opción:" -ForegroundColor Cyan
Write-Host "1. Construir e iniciar todos los servicios (primera vez)" -ForegroundColor White
Write-Host "2. Iniciar servicios existentes" -ForegroundColor White
Write-Host "3. Detener todos los servicios" -ForegroundColor White
Write-Host "4. Reconstruir servicios (sin caché)" -ForegroundColor White
Write-Host "5. Ver logs de todos los servicios" -ForegroundColor White
Write-Host "6. Ver estado de los servicios" -ForegroundColor White
Write-Host "7. Limpiar todo (incluyendo volúmenes)" -ForegroundColor White
Write-Host "8. Salir" -ForegroundColor White
Write-Host ""

$option = Read-Host "Ingresa el número de la opción"

switch ($option) {
    "1" {
        Write-Host "`nConstruyendo e iniciando servicios..." -ForegroundColor Yellow
        docker-compose build
        if ($LASTEXITCODE -eq 0) {
            docker-compose up -d
            Write-Host "`n✓ Servicios iniciados correctamente" -ForegroundColor Green
            Write-Host "`nAccede al API Gateway en: http://localhost:5000" -ForegroundColor Cyan
            Write-Host "Para ver los logs: docker-compose logs -f" -ForegroundColor Yellow
        } else {
            Write-Host "`n✗ Error al construir los servicios" -ForegroundColor Red
        }
    }
    "2" {
        Write-Host "`nIniciando servicios..." -ForegroundColor Yellow
        docker-compose up -d
        Write-Host "`n✓ Servicios iniciados" -ForegroundColor Green
    }
    "3" {
        Write-Host "`nDeteniendo servicios..." -ForegroundColor Yellow
        docker-compose down
        Write-Host "`n✓ Servicios detenidos" -ForegroundColor Green
    }
    "4" {
        Write-Host "`nReconstruyendo servicios sin caché..." -ForegroundColor Yellow
        docker-compose build --no-cache
        if ($LASTEXITCODE -eq 0) {
            docker-compose up -d
            Write-Host "`n✓ Servicios reconstruidos e iniciados" -ForegroundColor Green
        }
    }
    "5" {
        Write-Host "`nMostrando logs (Ctrl+C para salir)..." -ForegroundColor Yellow
        docker-compose logs -f
    }
    "6" {
        Write-Host "`nEstado de los servicios:" -ForegroundColor Yellow
        docker-compose ps
        Write-Host "`n"
        Write-Host "Uso de recursos:" -ForegroundColor Yellow
        docker stats --no-stream
    }
    "7" {
        Write-Host "`n⚠️  ADVERTENCIA: Esto eliminará todos los datos de las bases de datos" -ForegroundColor Red
        $confirm = Read-Host "¿Estás seguro? (si/no)"
        if ($confirm -eq "si" -or $confirm -eq "s") {
            Write-Host "`nLimpiando..." -ForegroundColor Yellow
            docker-compose down -v
            Write-Host "`n✓ Todo limpiado" -ForegroundColor Green
        } else {
            Write-Host "`nOperación cancelada" -ForegroundColor Yellow
        }
    }
    "8" {
        Write-Host "`nSaliendo..." -ForegroundColor Yellow
        exit 0
    }
    default {
        Write-Host "`nOpción no válida" -ForegroundColor Red
    }
}

Write-Host ""

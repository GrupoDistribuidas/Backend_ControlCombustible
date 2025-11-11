# Script para insertar datos en las bases de datos MySQL de Docker
# Uso: .\insert-data.ps1

Write-Host "=== Insertar Datos en Bases de Datos MySQL ===" -ForegroundColor Cyan

# Función para ejecutar SQL en un contenedor
function Execute-SQL {
    param(
        [string]$Container,
        [string]$Database,
        [string]$SQL
    )
    
    $command = "mysql -u combustible_user -pdavidgiler21 $Database -e `"$SQL`""
    docker exec -i $Container sh -c $command
}

# Ejemplo: Insertar en AuthDB
Write-Host "`nInsertando datos en AuthDB..." -ForegroundColor Yellow
$authSQL = @"
USE AuthDB;
-- INSERT INTO Users (Username, Email) VALUES ('test', 'test@example.com');
-- Agrega tus inserts aquí
"@

# Execute-SQL -Container "mysql_auth" -Database "AuthDB" -SQL $authSQL

# Ejemplo: Insertar en VehiclesDB
Write-Host "Insertando datos en VehiclesDB..." -ForegroundColor Yellow
$vehiclesSQL = @"
USE VehiclesDB;
-- INSERT INTO Vehicles (Plate, Model) VALUES ('ABC123', 'Toyota');
-- Agrega tus inserts aquí
"@

# Execute-SQL -Container "mysql_vehicles" -Database "VehiclesDB" -SQL $vehiclesSQL

Write-Host "`n✅ Script completado" -ForegroundColor Green
Write-Host "Descomenta las líneas de Execute-SQL y agrega tus queries SQL" -ForegroundColor Yellow

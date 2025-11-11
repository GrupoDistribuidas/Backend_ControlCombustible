-- Script de inicialización para VehiclesDB
-- Este script se ejecutará automáticamente cuando el contenedor MySQL se inicie por primera vez

USE VehiclesDB;

-- Tabla de tipos de maquinaria
CREATE TABLE IF NOT EXISTS TipoMaquinaria (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL UNIQUE,
    Descripcion VARCHAR(255) NULL,
    Estado TINYINT(1) NOT NULL DEFAULT 1,
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaModificacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tabla de vehículos
CREATE TABLE IF NOT EXISTS Vehiculos (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Placa VARCHAR(20) NOT NULL UNIQUE,
    Marca VARCHAR(50) NOT NULL,
    Modelo VARCHAR(50) NOT NULL,
    TipoMaquinariaId INT NOT NULL,
    Disponible VARCHAR(50) NOT NULL DEFAULT 'Disponible' COMMENT 'Disponible, En mantenimiento, No Disponible',
    ConsumoCombustibleKm DECIMAL(10,2) NOT NULL,
    CapacidadCombustible DECIMAL(10,2) NOT NULL,
    Estado TINYINT(1) NOT NULL DEFAULT 1,
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaModificacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_placa (Placa),
    INDEX idx_estado (Estado),
    INDEX idx_disponible (Disponible),
    FOREIGN KEY (TipoMaquinariaId) REFERENCES TipoMaquinaria(Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insertar tipos de maquinaria por defecto
INSERT INTO TipoMaquinaria (Nombre, Descripcion) VALUES 
('Maquinaria ligera', 'Vehículo ligero'),
('Maquinaria Pesada', 'Excavadoras, grúas, etc.')
ON DUPLICATE KEY UPDATE Nombre=Nombre;

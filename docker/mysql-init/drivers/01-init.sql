-- Script de inicialización para DriversDB
-- Este script se ejecutará automáticamente cuando el contenedor MySQL se inicie por primera vez

USE DriversDB;

-- Tabla de tipos de maquinaria
CREATE TABLE IF NOT EXISTS TipoMaquinaria (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL UNIQUE,
    Descripcion VARCHAR(255) NULL,
    Estado TINYINT(1) NOT NULL DEFAULT 1,
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaModificacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tabla de choferes
CREATE TABLE IF NOT EXISTS Choferes (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    PrimerNombre VARCHAR(50) NOT NULL,
    SegundoNombre VARCHAR(50) NULL,
    PrimerApellido VARCHAR(50) NOT NULL,
    SegundoApellido VARCHAR(50) NULL,
    NombreCompleto VARCHAR(200) NOT NULL,
    Identificacion VARCHAR(20) NOT NULL UNIQUE,
    FechaNacimiento DATE NOT NULL,
    Disponible TINYINT(1) NOT NULL DEFAULT 1,
    UsuarioId INT NOT NULL,
    TipoMaquinariaId INT NOT NULL,
    Estado TINYINT(1) NOT NULL DEFAULT 1,
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaModificacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_identificacion (Identificacion),
    INDEX idx_usuario (UsuarioId),
    INDEX idx_estado (Estado),
    INDEX idx_disponible (Disponible)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insertar tipos de maquinaria por defecto
INSERT INTO TipoMaquinaria (Nombre, Descripcion) VALUES 
('Maquinaria ligera', 'Vehículo ligero'),
('Maquinaria Pesada', 'Excavadoras, grúas, etc.')
ON DUPLICATE KEY UPDATE Nombre=Nombre;
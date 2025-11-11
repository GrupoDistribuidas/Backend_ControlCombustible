-- Script de inicialización para RoutesDB
-- Este script se ejecutará automáticamente cuando el contenedor MySQL se inicie por primera vez

USE RoutesDB;

-- Tabla de puntos (orígenes y destinos)
CREATE TABLE IF NOT EXISTS Puntos (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Direccion VARCHAR(255) NOT NULL,
    Provincia VARCHAR(100) NOT NULL,
    TipoPunto VARCHAR(50) NOT NULL COMMENT 'Origen, Destino, Intermedio',
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaModificacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_nombre (Nombre),
    INDEX idx_provincia (Provincia)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tabla de rutas
CREATE TABLE IF NOT EXISTS Rutas (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    PuntoInicioId INT NOT NULL,
    PuntoFinId INT NOT NULL,
    Distancia DOUBLE NOT NULL COMMENT 'Distancia en kilómetros',
    Estado TINYINT(1) NOT NULL DEFAULT 1,
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaModificacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_nombre (Nombre),
    INDEX idx_estado (Estado),
    FOREIGN KEY (PuntoInicioId) REFERENCES Puntos(Id),
    FOREIGN KEY (PuntoFinId) REFERENCES Puntos(Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Script de inicialización para FuelDB
-- Este script se ejecutará automáticamente cuando el contenedor MySQL se inicie por primera vez

USE FuelDB;

-- Tabla de estados de asignación
CREATE TABLE IF NOT EXISTS estadosasignacion (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL UNIQUE,
    Descripcion VARCHAR(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tabla de estados de registro de consumo
CREATE TABLE IF NOT EXISTS estadosregistroconsumo (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL UNIQUE,
    Descripcion VARCHAR(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tabla de asignaciones de rutas
CREATE TABLE IF NOT EXISTS asignacionesrutas (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ChoferId INT NOT NULL,
    VehiculoId INT NOT NULL,
    RutaId INT NOT NULL,
    FechaAsignacion DATETIME NOT NULL,
    CombustibleEstimado DOUBLE NOT NULL,
    EstadoId INT NOT NULL,
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaModificacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_chofer (ChoferId),
    INDEX idx_vehiculo (VehiculoId),
    INDEX idx_ruta (RutaId),
    INDEX idx_estado (EstadoId),
    FOREIGN KEY (EstadoId) REFERENCES estadosasignacion(Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tabla de registros de consumo
CREATE TABLE IF NOT EXISTS registrosconsumo (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    AsignacionRutaId INT NOT NULL,
    FechaRegistro DATETIME NOT NULL,
    CombustibleEstimado DOUBLE NOT NULL,
    CombustibleReal DOUBLE NOT NULL,
    Motivo VARCHAR(500) NOT NULL,
    EstadoId INT NOT NULL,
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaModificacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_asignacion (AsignacionRutaId),
    INDEX idx_estado (EstadoId),
    FOREIGN KEY (AsignacionRutaId) REFERENCES asignacionesrutas(Id),
    FOREIGN KEY (EstadoId) REFERENCES estadosregistroconsumo(Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insertar estados de asignación por defecto
INSERT INTO estadosasignacion (Id, Nombre, Descripcion) VALUES 
(1, 'Asignada', 'Planificada pero no iniciada'),
(2, 'En Proceso', 'Viaje en curso'),
(3, 'Completada', 'Viaje finalizado exitosamente'),
(4, 'Cancelada', 'Asignación cancelada'),
(5, 'Pausada', 'Viaje temporalmente suspendido')
ON DUPLICATE KEY UPDATE Nombre=VALUES(Nombre), Descripcion=VALUES(Descripcion);

-- Insertar estados de registro de consumo por defecto
INSERT INTO estadosregistroconsumo (Nombre, Descripcion) VALUES 
('Registrado', 'Consumo registrado'),
('Revisado', 'Consumo revisado'),
('Aprobado', 'Consumo aprobado')
ON DUPLICATE KEY UPDATE Nombre=VALUES(Nombre), Descripcion=VALUES(Descripcion);
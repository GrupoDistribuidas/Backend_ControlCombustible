-- Script de inicialización para AuthDB
-- Este script se ejecutará automáticamente cuando el contenedor MySQL se inicie por primera vez

USE AuthDB;

-- Tabla de usuarios
CREATE TABLE IF NOT EXISTS usuarios (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Email VARCHAR(255) NOT NULL UNIQUE,
    NombreUsuario VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(500) NOT NULL,
    RolId INT NOT NULL DEFAULT 2,
    Estado INT NOT NULL DEFAULT 1 COMMENT '1=Activo, 0=Inactivo',
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaModificacion DATETIME NULL,
    UltimoAcceso DATETIME NULL,
    INDEX idx_email (Email),
    INDEX idx_nombreusuario (NombreUsuario),
    INDEX idx_estado (Estado)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tabla de roles (opcional, si la necesitas)
CREATE TABLE IF NOT EXISTS roles (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL UNIQUE,
    Descripcion VARCHAR(255) NULL,
    Estado TINYINT(1) NOT NULL DEFAULT 1,
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insertar roles por defecto
INSERT INTO roles (Id, Nombre, Descripcion, Estado) VALUES 
(1, 'Operador', 'Acceso restringido solo a consumo de combustibles', 1),
(2, 'Administrador', 'Acceso a todo el sistema', 1)
ON DUPLICATE KEY UPDATE Nombre=Nombre;

INSERT INTO usuarios (Email, NombreUsuario, PasswordHash, RolId, Estado) VALUES 
('michaelcha27@gmail.com', 'michael', '$2a$12$4vECyDLdFpUOKPlBIP1gAujJXjx.eFz7g//.jdZ9I0HljjoYlYQAO', 2, 1),
('davidgiler21@gmail.com', 'david', '$2a$12$4vECyDLdFpUOKPlBIP1gAujJXjx.eFz7g//.jdZ9I0HljjoYlYQAO', 2, 1)
ON DUPLICATE KEY UPDATE NombreUsuario=NombreUsuario;

<div align="center">
  <h1>⛽ Backend Control de Combustible</h1>
  <p>
    <strong>Sistema de gestión de combustible, vehículos, choferes y rutas basado en Arquitectura de Microservicios.</strong>
  </p>
  <p>
    <img src="https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white" alt=".NET" />
    <img src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white" alt="C#" />
    <img src="https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white" alt="Docker" />
    <img src="https://img.shields.io/badge/MySQL-005C84?style=for-the-badge&logo=mysql&logoColor=white" alt="MySQL" />
    <img src="https://img.shields.io/badge/gRPC-244C5A?style=for-the-badge&logo=grpc&logoColor=white" alt="gRPC" />
    <img src="https://img.shields.io/badge/JWT-black?style=for-the-badge&logo=JSON%20web%20tokens" alt="JWT" />
  </p>
</div>

---

## 🏗️ Arquitectura y Patrones de Diseño

Este proyecto ha sido desarrollado utilizando una **Arquitectura de Microservicios** robusta y escalable. Para garantizar un alto rendimiento, mantenibilidad y un bajo acoplamiento, se implementaron los siguientes patrones de diseño:

- **Microservices Architecture:** El sistema está dividido en pequeños servicios independientes, donde cada uno cumple una función de negocio específica.
- **API Gateway Pattern:** Existe un punto de entrada único (`ApiGateway`) que recibe todas las peticiones del cliente y las enruta al microservicio correspondiente, aislando los detalles de la infraestructura interna.
- **Database-per-Service:** Cada microservicio gestiona su propia base de datos, garantizando la independencia total de los datos y evitando cuellos de botella (bases de datos en MySQL separadas para Autenticación, Vehículos, Choferes, Rutas y Combustible).
- **Inter-Service Communication (gRPC):** La comunicación interna entre los distintos microservicios se realiza utilizando **gRPC**, lo que permite llamadas remotas ultra-rápidas y con bajo consumo de red.

### Diagrama de Arquitectura

El siguiente diagrama ilustra cómo los componentes interactúan entre sí. Las peticiones externas (REST) entran por el API Gateway, y la comunicación interna entre microservicios se efectúa exclusivamente a través de gRPC.

```text
                                +-----------------------+
                                |                       |
                                |  Cliente HTTP / REST  |
                                |                       |
                                +-----------+-----------+
                                            |
                                        (REST API)
                                            |
                                            v
                                +-----------------------+
                                |                       |
                                |      API Gateway      |
                                |     (Puerto 5000)     |
                                |                       |
                                +-----------+-----------+
                                            |
      +-------------------+-----------------+-------+-----------------+-------------------+
      |                   |                         |                 |                   |
    (gRPC)              (gRPC)                    (gRPC)            (gRPC)              (gRPC)
      |                   |                         |                 |                   |
      v                   v                         v                 v                   v
+-----------+       +-----------+             +-----------+     +-----------+       +-----------+
|    MS     |       |    MS     |             |    MS     |     |    MS     |       |    MS     |
|Autenticac.| <.... | Choferes  | <.......... |Combustible|....>| Vehiculos |       |   Rutas   |
| (P: 5001) | gRPC  | (P: 5133) |    gRPC     | (P: 5136) |gRPC | (P: 5135) |       | (P: 5174) |
+-----+-----+       +-----+-----+             +-----+-----+     +-----+-----+       +-----+-----+
      |                   |                         |                 |                   |
      |                   |                         |                 |                   |
      v                   v                         v                 v                   v
+-----------+       +-----------+             +-----------+     +-----------+       +-----------+
|  AuthDB   |       | DriversDB |             |  FuelDB   |     |VehiclesDB |       | RoutesDB  |
| (MySQL)   |       |  (MySQL)  |             |  (MySQL)  |     |  (MySQL)  |       |  (MySQL)  |
| P: 33071  |       | P: 33073  |             | P: 33075  |     | P: 33072  |       | P: 33074  |
+-----------+       +-----------+             +-----------+     +-----------+       +-----------+

* Nota: Las flechas punteadas indican comunicación interna vía gRPC. 
  El MS.Combustible también se comunica con el MS.Rutas vía gRPC.
```

### Otros Aspectos Técnicos

- **Health Checks:** El sistema incorpora scripts (`add-health-checks-packages.ps1`) y comprobaciones de estado. Docker Compose gestiona el ciclo de vida asegurándose de que los servicios dependientes esperen la inicialización correcta de las bases de datos.
- **Inyección de Dependencias:** Uso exhaustivo del contenedor de dependencias nativo de ASP.NET Core (.NET) para registrar servicios, repositorios y clientes gRPC.
- **Migraciones e Inicialización de BD:** Las bases de datos MySQL se inicializan automáticamente al levantar Docker mediante volúmenes y scripts en `/docker-entrypoint-initdb.d`, asegurando que el esquema esté listo en el primer arranque.

## 📦 Estructura de Microservicios

El sistema se compone de los siguientes módulos:

1. **`ApiGateway`** (Puerto: `5000`): Enrutador principal de la aplicación.
2. **`MS.Autenticacion`** (Puerto: `5001`): Gestión de usuarios, inicio de sesión, recuperación de contraseñas vía email (SMTP) y emisión de tokens.
3. **`MS.Choferes`** (Puerto: `5133`): Administración de la información de los conductores.
4. **`MS.Vehiculos`** (Puerto: `5135`): Gestión de la flota de vehículos.
5. **`MS.Combustible`** (Puerto: `5136`): Control y registro de los abastecimientos y consumos de combustible. Consolidación con choferes, vehículos y rutas.
6. **`MS.Rutas`** (Puerto: `5174`): Planificación y gestión de las rutas asignadas.

## 🛡️ Seguridad

La seguridad es una prioridad en esta plataforma. Se aplican las siguientes medidas:

- **Autenticación mediante JWT:** Todas las peticiones al API (excepto el login público) deben incluir un `JSON Web Token` firmado de forma segura (`JWT_SECRET`). El Gateway se encarga de validar o dejar pasar la petición según la configuración.
- **Aislamiento de Red (Docker Networks):** Los microservicios y bases de datos se comunican internamente a través de una red privada virtual de Docker (`combustible_network`). Las bases de datos no se exponen directamente para acceso público, sino a través de mapeos seguros para administración.
- **Comunicación Interna Privada:** Las llamadas gRPC entre microservicios no están expuestas al exterior; únicamente el API Gateway recibe tráfico de red externo.
- **Variables de Entorno:** Las contraseñas, secretos de JWT y credenciales SMTP se administran mediante variables de entorno en el entorno de Docker, previniendo fugas de credenciales.

## 🛠️ Tecnologías Utilizadas

- **Backend:** C# con ASP.NET Core
- **Bases de Datos:** MySQL 8.0
- **Comunicación:** gRPC / REST API
- **Infraestructura y Orquestación:** Docker y Docker Compose
- **Notificaciones:** SMTP (Gmail) para envíos de correo

---

## 🚀 Uso y Despliegue con Docker

El proyecto está dockerizado para un despliegue rápido y sencillo. 

### Prerrequisitos

- Tener instalado [Docker](https://www.docker.com/get-started) y Docker Compose.
- (Opcional) PowerShell para ejecutar los scripts de automatización en Windows.

### Inicializar el Proyecto

La manera más sencilla de levantar todo el ecosistema es utilizando **Docker Compose**. Asegúrate de estar en el directorio raíz del proyecto (`Backend_ControlCombustible`) y ejecuta:

```bash
# Levantar los contenedores en segundo plano (detached mode)
docker-compose up -d
```

Este comando descargará las imágenes necesarias (si no existen), construirá los `Dockerfiles` de cada microservicio, inicializará las 5 bases de datos MySQL y ejecutará los servicios y el API Gateway.

Para detener todos los servicios:

```bash
docker-compose down
```

### 📜 Scripts de Automatización (PowerShell)

Para facilitar las tareas a los desarrolladores, se incluyen scripts auxiliares:

- `quick-setup.ps1`: Configuración inicial rápida del entorno.
- `docker-start.ps1`: Levanta y verifica el estado de los contenedores Docker.
- `verify-setup.ps1`: Revisa que la infraestructura (bases de datos y contenedores) esté operativa.
- `insert-data.ps1`: Script para sembrar o insertar datos iniciales de prueba en las bases de datos.

Puedes ejecutarlos desde una consola PowerShell:
```powershell
.\docker-start.ps1
```

## 🧪 Desarrollo Local

Si deseas ejecutar un microservicio específico fuera de Docker:
1. Abre la solución `ControlCombustible.sln` en tu IDE favorito (Visual Studio, Rider o VS Code).
2. Asegúrate de configurar las variables de entorno necesarias (ej: cadenas de conexión a tu MySQL local o contenedorizado) indicadas en los `appsettings.Development.json` o en tu entorno local.
3. Ejecuta el microservicio deseado y el `ApiGateway` si necesitas interactuar con él desde el exterior.

---
*Desarrollado para el módulo de Programación Distribuida.*

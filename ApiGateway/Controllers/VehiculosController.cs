using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MS.Vehiculos.Protos;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.ComponentModel.DataAnnotations;

namespace ApiGateway.Controllers
{
    /// <summary>
    /// Controlador para la gestión de vehículos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere JWT para todos los endpoints
    [Produces("application/json")]
    public class VehiculosController : ControllerBase
    {
        private readonly ILogger<VehiculosController> _logger;
        private readonly string _vehiculosServiceUrl;

        public VehiculosController(ILogger<VehiculosController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _vehiculosServiceUrl = configuration.GetValue<string>("Services:VehiculosService:Url") ?? "https://localhost:7056";
        }

        /// <summary>
        /// Crea un nuevo vehículo en el sistema
        /// </summary>
        /// <param name="request">Datos del vehículo a crear</param>
        /// <returns>ID del vehículo creado</returns>
        /// <response code="200">Vehículo creado exitosamente</response>
        /// <response code="400">Datos inválidos o placa ya registrada</response>
        /// <response code="401">Token JWT no válido o ausente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CrearVehiculo([FromBody] CrearVehiculoRequestDto request)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_vehiculosServiceUrl);
                var client = new VehiculosService.VehiculosServiceClient(channel);

                var grpcRequest = new CrearVehiculoRequest
                {
                    Nombre = request.Nombre,
                    Placa = request.Placa,
                    Marca = request.Marca,
                    Modelo = request.Modelo,
                    TipoMaquinariaId = request.TipoMaquinariaId,
                    Disponible = request.Disponible,
                    ConsumoCombustibleKm = request.ConsumoCombustibleKm,
                    CapacidadCombustible = request.CapacidadCombustible
                };

                var response = await client.CrearVehiculoAsync(grpcRequest);

                return Ok(new
                {
                    Success = true,
                    Message = "Vehículo creado exitosamente",
                    Data = new { Id = response.Id }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando vehículo");
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Lista todos los vehículos registrados en el sistema
        /// </summary>
        /// <returns>Lista completa de vehículos con sus detalles</returns>
        /// <response code="200">Lista de vehículos obtenida exitosamente</response>
        /// <response code="401">Token JWT no válido o ausente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<object>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ListarVehiculos()
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_vehiculosServiceUrl);
                var client = new VehiculosService.VehiculosServiceClient(channel);

                var vehiculos = new List<object>();
                
                using var call = client.ListarVehiculos(new Empty());
                
                await foreach (var vehiculo in call.ResponseStream.ReadAllAsync())
                {
                    vehiculos.Add(new
                    {
                        Id = vehiculo.Id,
                        Nombre = vehiculo.Nombre,
                        Placa = vehiculo.Placa,
                        Marca = vehiculo.Marca,
                        Modelo = vehiculo.Modelo,
                        TipoMaquinariaId = vehiculo.TipoMaquinariaId,
                        Disponible = vehiculo.Disponible,
                        ConsumoCombustibleKm = vehiculo.ConsumoCombustibleKm,
                        CapacidadCombustible = vehiculo.CapacidadCombustible
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Vehículos obtenidos exitosamente",
                    Data = vehiculos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo vehículos");
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza los datos de un vehículo existente
        /// </summary>
        /// <param name="id">ID único del vehículo a actualizar</param>
        /// <param name="request">Datos actualizados del vehículo</param>
        /// <returns>Resultado de la operación de actualización</returns>
        /// <response code="200">Vehículo actualizado exitosamente</response>
        /// <response code="400">Datos inválidos o ID no encontrado</response>
        /// <response code="401">Token JWT no válido o ausente</response>
        /// <response code="404">Vehículo no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ActualizarVehiculo(int id, [FromBody] ActualizarVehiculoRequestDto request)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_vehiculosServiceUrl);
                var client = new VehiculosService.VehiculosServiceClient(channel);

                var grpcRequest = new ActualizarVehiculoRequest
                {
                    Id = id,
                    Nombre = request.Nombre,
                    Placa = request.Placa,
                    Marca = request.Marca,
                    Modelo = request.Modelo,
                    TipoMaquinariaId = request.TipoMaquinariaId,
                    Disponible = request.Disponible,
                    ConsumoCombustibleKm = request.ConsumoCombustibleKm,
                    CapacidadCombustible = request.CapacidadCombustible
                };

                if (request.Estado.HasValue)
                {
                    grpcRequest.Estado = request.Estado.Value;
                }

                var response = await client.ActualizarVehiculoAsync(grpcRequest);

                return Ok(new
                {
                    Success = response.Affected > 0,
                    Message = response.Affected > 0 ? "Vehículo actualizado exitosamente" : "No se pudo actualizar el vehículo",
                    Data = new { AffectedRows = response.Affected }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando vehículo {Id}", id);
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza únicamente el estado de un vehículo (activo/inactivo)
        /// </summary>
        /// <param name="id">ID único del vehículo</param>
        /// <param name="request">Nuevo estado del vehículo</param>
        /// <returns>Resultado de la actualización del estado</returns>
        /// <response code="200">Estado actualizado exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="401">Token JWT no válido o ausente</response>
        /// <response code="404">Vehículo no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPatch("{id}/estado")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ActualizarEstadoVehiculo(int id, [FromBody] ActualizarEstadoRequestDto request)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_vehiculosServiceUrl);
                var client = new VehiculosService.VehiculosServiceClient(channel);

                var grpcRequest = new ActualizarEstadoRequest
                {
                    Id = id,
                    Estado = request.Estado
                };

                var response = await client.ActualizarEstadoVehiculoAsync(grpcRequest);

                return Ok(new
                {
                    Success = response.Affected > 0,
                    Message = response.Affected > 0 ? "Estado actualizado exitosamente" : "No se pudo actualizar el estado",
                    Data = new { AffectedRows = response.Affected }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando estado del vehículo {Id}", id);
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Verifica si existe un vehículo registrado con la placa especificada
        /// </summary>
        /// <param name="placa">Placa del vehículo a verificar</param>
        /// <returns>Indica si existe un vehículo con esa placa</returns>
        /// <response code="200">Consulta realizada exitosamente</response>
        /// <response code="400">Placa inválida</response>
        /// <response code="401">Token JWT no válido o ausente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("exists/{placa}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ExistsByPlaca(string placa)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_vehiculosServiceUrl);
                var client = new VehiculosService.VehiculosServiceClient(channel);

                var grpcRequest = new ExistsByPlacaRequest
                {
                    Placa = placa
                };

                var response = await client.ExistsByPlacaAsync(grpcRequest);

                return Ok(new
                {
                    Success = true,
                    Message = "Consulta realizada exitosamente",
                    Data = new { Exists = response.Exists }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando existencia de placa {Placa}", placa);
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Busca vehículos utilizando filtros avanzados
        /// </summary>
        /// <param name="estado">Estado del vehículo (activo/inactivo) - opcional</param>
        /// <param name="tipoMaquinariaId">ID del tipo de maquinaria - opcional</param>
        /// <param name="marca">Marca del vehículo - opcional</param>
        /// <param name="modelo">Modelo del vehículo - opcional</param>
        /// <param name="capacidadMin">Capacidad mínima de combustible en litros - opcional</param>
        /// <param name="capacidadMax">Capacidad máxima de combustible en litros - opcional</param>
        /// <param name="consumoMin">Consumo mínimo por kilómetro - opcional</param>
        /// <param name="consumoMax">Consumo máximo por kilómetro - opcional</param>
        /// <param name="disponible">Estado de disponibilidad del vehículo - opcional</param>
        /// <returns>Lista filtrada de vehículos que coinciden con los criterios especificados</returns>
        /// <response code="200">Búsqueda realizada exitosamente</response>
        /// <response code="400">Parámetros de búsqueda inválidos</response>
        /// <response code="401">Token JWT no válido o ausente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<object>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Search(
            [FromQuery] bool? estado = null,
            [FromQuery] int? tipoMaquinariaId = null,
            [FromQuery] string? marca = null,
            [FromQuery] string? modelo = null,
            [FromQuery] double? capacidadMin = null,
            [FromQuery] double? capacidadMax = null,
            [FromQuery] double? consumoMin = null,
            [FromQuery] double? consumoMax = null,
            [FromQuery] string? disponible = null)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_vehiculosServiceUrl);
                var client = new VehiculosService.VehiculosServiceClient(channel);

                var grpcRequest = new SearchRequest();

                if (estado.HasValue)
                    grpcRequest.Estado = estado.Value;
                if (tipoMaquinariaId.HasValue)
                    grpcRequest.TipoMaquinariaId = tipoMaquinariaId.Value;
                if (!string.IsNullOrEmpty(marca))
                    grpcRequest.Marca = marca;
                if (!string.IsNullOrEmpty(modelo))
                    grpcRequest.Modelo = modelo;
                if (capacidadMin.HasValue)
                    grpcRequest.CapacidadMin = capacidadMin.Value;
                if (capacidadMax.HasValue)
                    grpcRequest.CapacidadMax = capacidadMax.Value;
                if (consumoMin.HasValue)
                    grpcRequest.ConsumoMin = consumoMin.Value;
                if (consumoMax.HasValue)
                    grpcRequest.ConsumoMax = consumoMax.Value;
                if (!string.IsNullOrEmpty(disponible))
                    grpcRequest.Disponible = disponible;

                var vehiculos = new List<object>();
                
                using var call = client.Search(grpcRequest);
                
                await foreach (var vehiculo in call.ResponseStream.ReadAllAsync())
                {
                    vehiculos.Add(new
                    {
                        Id = vehiculo.Id,
                        Nombre = vehiculo.Nombre,
                        Placa = vehiculo.Placa,
                        Marca = vehiculo.Marca,
                        Modelo = vehiculo.Modelo,
                        TipoMaquinariaId = vehiculo.TipoMaquinariaId,
                        Disponible = vehiculo.Disponible,
                        ConsumoCombustibleKm = vehiculo.ConsumoCombustibleKm,
                        CapacidadCombustible = vehiculo.CapacidadCombustible
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Búsqueda realizada exitosamente",
                    Data = vehiculos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda de vehículos");
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Busca vehículos utilizando un término general de búsqueda
        /// </summary>
        /// <param name="term">Término de búsqueda (busca en nombre, placa, marca, modelo)</param>
        /// <returns>Lista de vehículos que coinciden con el término de búsqueda</returns>
        /// <response code="200">Búsqueda por término realizada exitosamente</response>
        /// <response code="400">Término de búsqueda inválido</response>
        /// <response code="401">Token JWT no válido o ausente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("search/{term}")]
        [ProducesResponseType(typeof(IEnumerable<object>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SearchByTerm(string term)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_vehiculosServiceUrl);
                var client = new VehiculosService.VehiculosServiceClient(channel);

                var grpcRequest = new SearchByTermRequest
                {
                    Term = term
                };

                var vehiculos = new List<object>();
                
                using var call = client.SearchByTerm(grpcRequest);
                
                await foreach (var vehiculo in call.ResponseStream.ReadAllAsync())
                {
                    vehiculos.Add(new
                    {
                        Id = vehiculo.Id,
                        Nombre = vehiculo.Nombre,
                        Placa = vehiculo.Placa,
                        Marca = vehiculo.Marca,
                        Modelo = vehiculo.Modelo,
                        TipoMaquinariaId = vehiculo.TipoMaquinariaId,
                        Disponible = vehiculo.Disponible,
                        ConsumoCombustibleKm = vehiculo.ConsumoCombustibleKm,
                        CapacidadCombustible = vehiculo.CapacidadCombustible
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Búsqueda realizada exitosamente",
                    Data = vehiculos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda por término {Term}", term);
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }
    }

    // DTOs para las requests REST
    /// <summary>
    /// Datos requeridos para crear un nuevo vehículo
    /// </summary>
    public class CrearVehiculoRequestDto
    {
        /// <summary>
        /// Nombre descriptivo del vehículo
        /// </summary>
        /// <example>Camión de Carga Pesada</example>
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Placa única del vehículo
        /// </summary>
        /// <example>ABC-1234</example>
        [Required(ErrorMessage = "La placa es requerida")]
        public string Placa { get; set; } = string.Empty;

        /// <summary>
        /// Marca del vehículo
        /// </summary>
        /// <example>Volvo</example>
        [Required(ErrorMessage = "La marca es requerida")]
        public string Marca { get; set; } = string.Empty;

        /// <summary>
        /// Modelo del vehículo
        /// </summary>
        /// <example>FH16</example>
        [Required(ErrorMessage = "El modelo es requerido")]
        public string Modelo { get; set; } = string.Empty;

        /// <summary>
        /// ID del tipo de maquinaria
        /// </summary>
        /// <example>1</example>
        [Required(ErrorMessage = "El tipo de maquinaria es requerido")]
        public int TipoMaquinariaId { get; set; }

        /// <summary>
        /// Estado de disponibilidad del vehículo
        /// </summary>
        /// <example>disponible</example>
        [Required(ErrorMessage = "El estado de disponibilidad es requerido")]
        public string Disponible { get; set; } = string.Empty;

        /// <summary>
        /// Consumo de combustible por kilómetro
        /// </summary>
        /// <example>0.35</example>
        [Required(ErrorMessage = "El consumo de combustible es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El consumo debe ser mayor a 0")]
        public double ConsumoCombustibleKm { get; set; }

        /// <summary>
        /// Capacidad del tanque de combustible en litros
        /// </summary>
        /// <example>500.0</example>
        [Required(ErrorMessage = "La capacidad de combustible es requerida")]
        [Range(1, double.MaxValue, ErrorMessage = "La capacidad debe ser mayor a 0")]
        public double CapacidadCombustible { get; set; }
    }

    /// <summary>
    /// Datos para actualizar un vehículo existente
    /// </summary>
    public class ActualizarVehiculoRequestDto
    {
        /// <summary>
        /// Nombre descriptivo del vehículo
        /// </summary>
        /// <example>Camión de Carga Pesada Actualizado</example>
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Placa única del vehículo
        /// </summary>
        /// <example>ABC-1234</example>
        [Required(ErrorMessage = "La placa es requerida")]
        public string Placa { get; set; } = string.Empty;

        /// <summary>
        /// Marca del vehículo
        /// </summary>
        /// <example>Volvo</example>
        [Required(ErrorMessage = "La marca es requerida")]
        public string Marca { get; set; } = string.Empty;

        /// <summary>
        /// Modelo del vehículo
        /// </summary>
        /// <example>FH16</example>
        [Required(ErrorMessage = "El modelo es requerido")]
        public string Modelo { get; set; } = string.Empty;

        /// <summary>
        /// ID del tipo de maquinaria
        /// </summary>
        /// <example>1</example>
        [Required(ErrorMessage = "El tipo de maquinaria es requerido")]
        public int TipoMaquinariaId { get; set; }

        /// <summary>
        /// Estado de disponibilidad del vehículo
        /// </summary>
        /// <example>disponible</example>
        [Required(ErrorMessage = "El estado de disponibilidad es requerido")]
        public string Disponible { get; set; } = string.Empty;

        /// <summary>
        /// Consumo de combustible por kilómetro
        /// </summary>
        /// <example>0.35</example>
        [Required(ErrorMessage = "El consumo de combustible es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El consumo debe ser mayor a 0")]
        public double ConsumoCombustibleKm { get; set; }

        /// <summary>
        /// Capacidad del tanque de combustible en litros
        /// </summary>
        /// <example>500.0</example>
        [Required(ErrorMessage = "La capacidad de combustible es requerida")]
        [Range(1, double.MaxValue, ErrorMessage = "La capacidad debe ser mayor a 0")]
        public double CapacidadCombustible { get; set; }

        /// <summary>
        /// Estado activo/inactivo del vehículo (opcional)
        /// </summary>
        /// <example>true</example>
        public bool? Estado { get; set; }
    }

    /// <summary>
    /// Datos para actualizar únicamente el estado de un vehículo
    /// </summary>
    public class ActualizarEstadoRequestDto
    {
        /// <summary>
        /// Nuevo estado del vehículo (true = activo, false = inactivo)
        /// </summary>
        /// <example>true</example>
        [Required(ErrorMessage = "El estado es requerido")]
        public bool Estado { get; set; }
    }
}
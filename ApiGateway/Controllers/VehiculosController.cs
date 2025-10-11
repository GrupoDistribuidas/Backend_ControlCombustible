using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MS.Vehiculos.Protos;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere JWT para todos los endpoints
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
        /// Crea un nuevo vehículo
        /// </summary>
        /// <param name="request">Datos del vehículo a crear</param>
        /// <returns>ID del vehículo creado</returns>
        [HttpPost]
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
        /// Lista todos los vehículos
        /// </summary>
        /// <returns>Lista de vehículos</returns>
        [HttpGet]
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
        /// Actualiza un vehículo existente
        /// </summary>
        /// <param name="id">ID del vehículo</param>
        /// <param name="request">Datos actualizados del vehículo</param>
        /// <returns>Resultado de la actualización</returns>
        [HttpPut("{id}")]
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
        /// Actualiza el estado de un vehículo
        /// </summary>
        /// <param name="id">ID del vehículo</param>
        /// <param name="request">Nuevo estado</param>
        /// <returns>Resultado de la actualización</returns>
        [HttpPatch("{id}/estado")]
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
        /// Verifica si existe un vehículo con la placa especificada
        /// </summary>
        /// <param name="placa">Placa a verificar</param>
        /// <returns>True si existe, false en caso contrario</returns>
        [HttpGet("exists/{placa}")]
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
        /// Busca vehículos por filtros avanzados
        /// </summary>
        /// <param name="estado">Estado del vehículo (opcional)</param>
        /// <param name="tipoMaquinariaId">ID del tipo de maquinaria (opcional)</param>
        /// <param name="marca">Marca del vehículo (opcional)</param>
        /// <param name="modelo">Modelo del vehículo (opcional)</param>
        /// <param name="capacidadMin">Capacidad mínima de combustible (opcional)</param>
        /// <param name="capacidadMax">Capacidad máxima de combustible (opcional)</param>
        /// <param name="consumoMin">Consumo mínimo por km (opcional)</param>
        /// <param name="consumoMax">Consumo máximo por km (opcional)</param>
        /// <param name="disponible">Estado de disponibilidad (opcional)</param>
        /// <returns>Lista de vehículos que coinciden con los filtros</returns>
        [HttpGet("search")]
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
        /// Busca vehículos por término general
        /// </summary>
        /// <param name="term">Término de búsqueda</param>
        /// <returns>Lista de vehículos que coinciden con el término</returns>
        [HttpGet("search/{term}")]
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
    public class CrearVehiculoRequestDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int TipoMaquinariaId { get; set; }
        public string Disponible { get; set; } = string.Empty;
        public double ConsumoCombustibleKm { get; set; }
        public double CapacidadCombustible { get; set; }
    }

    public class ActualizarVehiculoRequestDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int TipoMaquinariaId { get; set; }
        public string Disponible { get; set; } = string.Empty;
        public double ConsumoCombustibleKm { get; set; }
        public double CapacidadCombustible { get; set; }
        public bool? Estado { get; set; }
    }

    public class ActualizarEstadoRequestDto
    {
        public bool Estado { get; set; }
    }
}
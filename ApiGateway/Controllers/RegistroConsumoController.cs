using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MS.Combustible.Protos;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcStatusCode = Grpc.Core.StatusCode;
using System.ComponentModel.DataAnnotations;

namespace ApiGateway.Controllers
{
    /// <summary>
    /// Controlador para la gestión de registros de consumo de combustible
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class RegistroConsumoController : ControllerBase
    {
        private readonly ILogger<RegistroConsumoController> _logger;
        private readonly string _combustibleServiceUrl;

        public RegistroConsumoController(ILogger<RegistroConsumoController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _combustibleServiceUrl = configuration.GetValue<string>("Services:CombustibleService:Url") ?? "https://localhost:5136";
        }

        /// <summary>
        /// Crea un nuevo registro de consumo y automáticamente completa la asignación
        /// </summary>
        /// <param name="request">Datos del registro de consumo</param>
        /// <returns>ID del registro creado</returns>
        [HttpPost]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CrearRegistroConsumo([FromBody] CreateRegistroConsumoRequest request)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_combustibleServiceUrl);
                var client = new RegistroConsumoService.RegistroConsumoServiceClient(channel);

                var grpcRequest = new CrearRegistroConsumoRequest
                {
                    AsignacionRutaId = request.AsignacionRutaId,
                    FechaRegistro = Timestamp.FromDateTime(request.FechaRegistro.ToUniversalTime()),
                    CombustibleEstimado = request.CombustibleEstimado,
                    CombustibleReal = request.CombustibleReal,
                    Motivo = request.Motivo ?? string.Empty,
                    EstadoId = request.EstadoId
                };

                var response = await client.CrearRegistroConsumoAsync(grpcRequest);

                _logger.LogInformation($"Registro de consumo creado exitosamente con ID {response.Id}");

                return Ok(new
                {
                    success = true,
                    message = "Registro de consumo creado exitosamente. La asignación ha sido marcada como completada automáticamente.",
                    data = new { id = response.Id }
                });
            }
            catch (RpcException ex) when (ex.StatusCode == GrpcStatusCode.InvalidArgument)
            {
                _logger.LogWarning($"Error de validación al crear registro de consumo: {ex.Status.Detail}");
                return BadRequest(new { success = false, message = ex.Status.Detail });
            }
            catch (RpcException ex) when (ex.StatusCode == GrpcStatusCode.NotFound)
            {
                _logger.LogWarning($"Recurso no encontrado al crear registro de consumo: {ex.Status.Detail}");
                return NotFound(new { success = false, message = ex.Status.Detail });
            }
            catch (RpcException ex) when (ex.StatusCode == GrpcStatusCode.FailedPrecondition)
            {
                _logger.LogWarning($"Precondición fallida al crear registro de consumo: {ex.Status.Detail}");
                return Conflict(new { success = false, message = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al crear registro de consumo");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza un registro de consumo existente
        /// </summary>
        /// <param name="id">ID del registro</param>
        /// <param name="request">Datos actualizados</param>
        /// <returns>Resultado de la actualización</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ActualizarRegistroConsumo(int id, [FromBody] UpdateRegistroConsumoRequest request)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_combustibleServiceUrl);
                var client = new RegistroConsumoService.RegistroConsumoServiceClient(channel);

                var grpcRequest = new ActualizarRegistroConsumoRequest
                {
                    Id = id,
                    FechaRegistro = Timestamp.FromDateTime(request.FechaRegistro.ToUniversalTime()),
                    CombustibleEstimado = request.CombustibleEstimado,
                    CombustibleReal = request.CombustibleReal,
                    Motivo = request.Motivo ?? string.Empty,
                    EstadoId = request.EstadoId ?? 0
                };

                var response = await client.ActualizarRegistroConsumoAsync(grpcRequest);

                _logger.LogInformation($"Registro de consumo {id} actualizado exitosamente");

                return Ok(new
                {
                    success = true,
                    message = "Registro de consumo actualizado exitosamente",
                    data = new { affected = response.Affected }
                });
            }
            catch (RpcException ex) when (ex.StatusCode == GrpcStatusCode.InvalidArgument)
            {
                _logger.LogWarning($"Error de validación al actualizar registro de consumo {id}: {ex.Status.Detail}");
                return BadRequest(new { success = false, message = ex.Status.Detail });
            }
            catch (RpcException ex) when (ex.StatusCode == GrpcStatusCode.NotFound)
            {
                _logger.LogWarning($"Registro de consumo {id} no encontrado");
                return NotFound(new { success = false, message = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error interno al actualizar registro de consumo {id}");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene un registro de consumo por su ID
        /// </summary>
        /// <param name="id">ID del registro</param>
        /// <returns>Datos del registro</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetRegistroConsumo(int id)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_combustibleServiceUrl);
                var client = new RegistroConsumoService.RegistroConsumoServiceClient(channel);

                var request = new GetRegistroConsumoByIdRequest { Id = id };
                var response = await client.GetByIdAsync(request);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        id = response.Id,
                        asignacionRutaId = response.AsignacionRutaId,
                        fechaRegistro = response.FechaRegistro.ToDateTime(),
                        combustibleEstimado = response.CombustibleEstimado,
                        combustibleReal = response.CombustibleReal,
                        motivo = response.Motivo,
                        estadoId = response.EstadoId,
                        estadoNombre = response.EstadoNombre,
                        fechaCreacion = response.FechaCreacion.ToDateTime(),
                        fechaModificacion = response.FechaModificacion.ToDateTime()
                    }
                });
            }
            catch (RpcException ex) when (ex.StatusCode == GrpcStatusCode.NotFound)
            {
                _logger.LogWarning($"Registro de consumo {id} no encontrado");
                return NotFound(new { success = false, message = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error interno al obtener registro de consumo {id}");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene todos los registros de consumo
        /// </summary>
        /// <returns>Lista de registros</returns>
        [HttpGet]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllRegistrosConsumo()
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_combustibleServiceUrl);
                var client = new RegistroConsumoService.RegistroConsumoServiceClient(channel);

                var request = new Empty();
                var call = client.ListarTodos(request);

                var registros = new List<object>();
                await foreach (var registro in call.ResponseStream.ReadAllAsync())
                {
                    registros.Add(new
                    {
                        id = registro.Id,
                        asignacionRutaId = registro.AsignacionRutaId,
                        fechaRegistro = registro.FechaRegistro.ToDateTime(),
                        combustibleEstimado = registro.CombustibleEstimado,
                        combustibleReal = registro.CombustibleReal,
                        motivo = registro.Motivo,
                        estadoId = registro.EstadoId,
                        estadoNombre = registro.EstadoNombre,
                        fechaCreacion = registro.FechaCreacion.ToDateTime(),
                        fechaModificacion = registro.FechaModificacion.ToDateTime()
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = registros,
                    count = registros.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al obtener registros de consumo");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene registros de consumo por asignación de ruta
        /// </summary>
        /// <param name="asignacionId">ID de la asignación</param>
        /// <returns>Lista de registros</returns>
        [HttpGet("asignacion/{asignacionId}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetRegistrosByAsignacion(int asignacionId)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_combustibleServiceUrl);
                var client = new RegistroConsumoService.RegistroConsumoServiceClient(channel);

                var request = new GetRegistroConsumoByAsignacionRequest { AsignacionRutaId = asignacionId };
                var call = client.GetByAsignacionRutaId(request);

                var registros = new List<object>();
                await foreach (var registro in call.ResponseStream.ReadAllAsync())
                {
                    registros.Add(new
                    {
                        id = registro.Id,
                        asignacionRutaId = registro.AsignacionRutaId,
                        fechaRegistro = registro.FechaRegistro.ToDateTime(),
                        combustibleEstimado = registro.CombustibleEstimado,
                        combustibleReal = registro.CombustibleReal,
                        motivo = registro.Motivo,
                        estadoId = registro.EstadoId,
                        estadoNombre = registro.EstadoNombre,
                        fechaCreacion = registro.FechaCreacion.ToDateTime(),
                        fechaModificacion = registro.FechaModificacion.ToDateTime()
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = registros,
                    count = registros.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error interno al obtener registros por asignación {asignacionId}");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene registros de consumo por estado
        /// </summary>
        /// <param name="estadoId">ID del estado</param>
        /// <returns>Lista de registros</returns>
        [HttpGet("estado/{estadoId}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetRegistrosByEstado(int estadoId)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_combustibleServiceUrl);
                var client = new RegistroConsumoService.RegistroConsumoServiceClient(channel);

                var request = new GetRegistroConsumoByEstadoRequest { EstadoId = estadoId };
                var call = client.GetByEstadoId(request);

                var registros = new List<object>();
                await foreach (var registro in call.ResponseStream.ReadAllAsync())
                {
                    registros.Add(new
                    {
                        id = registro.Id,
                        asignacionRutaId = registro.AsignacionRutaId,
                        fechaRegistro = registro.FechaRegistro.ToDateTime(),
                        combustibleEstimado = registro.CombustibleEstimado,
                        combustibleReal = registro.CombustibleReal,
                        motivo = registro.Motivo,
                        estadoId = registro.EstadoId,
                        estadoNombre = registro.EstadoNombre,
                        fechaCreacion = registro.FechaCreacion.ToDateTime(),
                        fechaModificacion = registro.FechaModificacion.ToDateTime()
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = registros,
                    count = registros.Count
                });
            }
            catch (RpcException ex) when (ex.StatusCode == GrpcStatusCode.NotFound)
            {
                _logger.LogWarning($"Estado {estadoId} no encontrado");
                return NotFound(new { success = false, message = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error interno al obtener registros por estado {estadoId}");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene registros de consumo por rango de fechas
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio (formato: yyyy-MM-dd)</param>
        /// <param name="fechaFin">Fecha de fin (formato: yyyy-MM-dd)</param>
        /// <returns>Lista de registros</returns>
        [HttpGet("fechas")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetRegistrosByFechaRango([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
        {
            try
            {
                if (fechaInicio > fechaFin)
                {
                    return BadRequest(new { success = false, message = "La fecha de inicio no puede ser mayor que la fecha de fin" });
                }

                using var channel = GrpcChannel.ForAddress(_combustibleServiceUrl);
                var client = new RegistroConsumoService.RegistroConsumoServiceClient(channel);

                var request = new GetRegistroConsumoByFechaRangoRequest
                {
                    FechaInicio = Timestamp.FromDateTime(fechaInicio.ToUniversalTime()),
                    FechaFin = Timestamp.FromDateTime(fechaFin.ToUniversalTime())
                };

                var call = client.GetByFechaRango(request);

                var registros = new List<object>();
                await foreach (var registro in call.ResponseStream.ReadAllAsync())
                {
                    registros.Add(new
                    {
                        id = registro.Id,
                        asignacionRutaId = registro.AsignacionRutaId,
                        fechaRegistro = registro.FechaRegistro.ToDateTime(),
                        combustibleEstimado = registro.CombustibleEstimado,
                        combustibleReal = registro.CombustibleReal,
                        motivo = registro.Motivo,
                        estadoId = registro.EstadoId,
                        estadoNombre = registro.EstadoNombre,
                        fechaCreacion = registro.FechaCreacion.ToDateTime(),
                        fechaModificacion = registro.FechaModificacion.ToDateTime()
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = registros,
                    count = registros.Count,
                    filtros = new { fechaInicio, fechaFin }
                });
            }
            catch (RpcException ex) when (ex.StatusCode == GrpcStatusCode.InvalidArgument)
            {
                _logger.LogWarning($"Error de validación al obtener registros por rango de fechas: {ex.Status.Detail}");
                return BadRequest(new { success = false, message = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al obtener registros por rango de fechas");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Cambia el estado de un registro de consumo (método PATCH)
        /// </summary>
        /// <param name="id">ID del registro</param>
        /// <param name="request">Nuevo estado</param>
        /// <returns>Resultado del cambio</returns>
        [HttpPatch("{id}/estado")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CambiarEstadoRegistro(int id, [FromBody] CambiarEstadoRegistroRequest request)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_combustibleServiceUrl);
                var client = new RegistroConsumoService.RegistroConsumoServiceClient(channel);

                var grpcRequest = new MS.Combustible.Protos.CambiarEstadoRegistroRequest
                {
                    Id = id,
                    NuevoEstado = request.NuevoEstado
                };

                var response = await client.CambiarEstadoAsync(grpcRequest);

                _logger.LogInformation($"Estado del registro {id} cambiado a '{request.NuevoEstado}' exitosamente");

                return Ok(new
                {
                    success = true,
                    message = $"Estado del registro cambiado a '{request.NuevoEstado}' exitosamente",
                    data = new { affected = response.Affected }
                });
            }
            catch (RpcException ex) when (ex.StatusCode == GrpcStatusCode.NotFound)
            {
                _logger.LogWarning($"Registro {id} o estado '{request.NuevoEstado}' no encontrado");
                return NotFound(new { success = false, message = ex.Status.Detail });
            }
            catch (RpcException ex) when (ex.StatusCode == GrpcStatusCode.FailedPrecondition)
            {
                _logger.LogWarning($"Transición de estado inválida para registro {id}: {ex.Status.Detail}");
                return Conflict(new { success = false, message = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error interno al cambiar estado del registro {id}");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }
    }

    #region DTOs for API requests

    /// <summary>
    /// DTO para crear un nuevo registro de consumo
    /// </summary>
    public class CreateRegistroConsumoRequest
    {
        [Required]
        public int AsignacionRutaId { get; set; }

        [Required]
        public DateTime FechaRegistro { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "El combustible estimado debe ser mayor o igual a 0")]
        public double CombustibleEstimado { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "El combustible real debe ser mayor o igual a 0")]
        public double CombustibleReal { get; set; }

        public string? Motivo { get; set; }

        [Required]
        public int EstadoId { get; set; }
    }

    /// <summary>
    /// DTO para actualizar un registro de consumo
    /// </summary>
    public class UpdateRegistroConsumoRequest
    {
        [Required]
        public DateTime FechaRegistro { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "El combustible estimado debe ser mayor o igual a 0")]
        public double CombustibleEstimado { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "El combustible real debe ser mayor o igual a 0")]
        public double CombustibleReal { get; set; }

        public string? Motivo { get; set; }

        public int? EstadoId { get; set; }
    }

    /// <summary>
    /// DTO para cambiar el estado de un registro
    /// </summary>
    public class CambiarEstadoRegistroRequest
    {
        [Required]
        public string NuevoEstado { get; set; } = string.Empty;
    }

    #endregion
}
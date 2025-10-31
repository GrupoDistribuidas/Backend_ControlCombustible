using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MS.Combustible.Protos;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcStatusCode = Grpc.Core.StatusCode;

namespace ApiGateway.Controllers
{
    /// <summary>
    /// Controlador para la gestión de asignaciones de rutas
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class AsignacionesController : ControllerBase
    {
        private readonly ILogger<AsignacionesController> _logger;
        private readonly string _combustibleServiceUrl;

        public AsignacionesController(ILogger<AsignacionesController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _combustibleServiceUrl = configuration.GetValue<string>("Services:CombustibleService:Url") ?? "https://localhost:5136";
        }

        /// <summary>
        /// Crea una nueva asignación de ruta
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CrearAsignacion([FromBody] CrearAsignacionRequest request)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_combustibleServiceUrl);
                var client = new AsignacionesService.AsignacionesServiceClient(channel);

                var response = await client.CrearAsignacionAsync(request);
                return Ok(new { id = response.Id, mensaje = "Asignación creada exitosamente" });
            }
            catch (RpcException ex) when (ex.StatusCode == GrpcStatusCode.InvalidArgument)
            {
                return BadRequest(new { error = ex.Status.Detail });
            }
            catch (RpcException ex) when (ex.StatusCode == GrpcStatusCode.NotFound)
            {
                return NotFound(new { error = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear asignación");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene todas las asignaciones
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<object>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ListarAsignaciones()
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_combustibleServiceUrl);
                var client = new AsignacionesService.AsignacionesServiceClient(channel);

                var asignaciones = new List<AsignacionDto>();
                var call = client.ListarAsignaciones(new Empty());

                await foreach (var asignacion in call.ResponseStream.ReadAllAsync())
                {
                    asignaciones.Add(asignacion);
                }

                return Ok(asignaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar asignaciones");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene una asignación por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_combustibleServiceUrl);
                var client = new AsignacionesService.AsignacionesServiceClient(channel);

                var request = new GetByIdRequest { Id = id };
                var response = await client.GetByIdAsync(request);
                return Ok(response);
            }
            catch (RpcException ex) when (ex.StatusCode == GrpcStatusCode.NotFound)
            {
                return NotFound(new { error = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener asignación");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza una asignación existente
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ActualizarAsignacion(int id, [FromBody] ActualizarAsignacionRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest(new { error = "El ID de la URL no coincide con el ID del cuerpo" });
            }

            try
            {
                using var channel = GrpcChannel.ForAddress(_combustibleServiceUrl);
                var client = new AsignacionesService.AsignacionesServiceClient(channel);

                var response = await client.ActualizarAsignacionAsync(request);
                return Ok(new { affected = response.Affected, mensaje = "Asignación actualizada exitosamente" });
            }
            catch (RpcException ex) when (ex.StatusCode == GrpcStatusCode.InvalidArgument)
            {
                return BadRequest(new { error = ex.Status.Detail });
            }
            catch (RpcException ex) when (ex.StatusCode == GrpcStatusCode.NotFound)
            {
                return NotFound(new { error = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar asignación");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Cambia el estado de una asignación
        /// </summary>
        [HttpPatch("{id}/estado")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest(new { error = "El ID de la URL no coincide con el ID del cuerpo" });
            }

            try
            {
                using var channel = GrpcChannel.ForAddress(_combustibleServiceUrl);
                var client = new AsignacionesService.AsignacionesServiceClient(channel);

                var response = await client.CambiarEstadoAsync(request);
                return Ok(new { affected = response.Affected, mensaje = "Estado cambiado exitosamente" });
            }
            catch (RpcException ex) when (ex.StatusCode == GrpcStatusCode.InvalidArgument)
            {
                return BadRequest(new { error = ex.Status.Detail });
            }
            catch (RpcException ex) when (ex.StatusCode == GrpcStatusCode.NotFound)
            {
                return NotFound(new { error = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene asignaciones por chofer
        /// </summary>
        [HttpGet("chofer/{choferId}")]
        [ProducesResponseType(typeof(List<object>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetByChofer(int choferId)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_combustibleServiceUrl);
                var client = new AsignacionesService.AsignacionesServiceClient(channel);

                var request = new GetByChoferIdRequest { ChoferId = choferId };
                var asignaciones = new List<AsignacionDto>();
                var call = client.GetByChoferId(request);

                await foreach (var asignacion in call.ResponseStream.ReadAllAsync())
                {
                    asignaciones.Add(asignacion);
                }

                return Ok(asignaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener asignaciones por chofer");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene asignaciones por vehículo
        /// </summary>
        [HttpGet("vehiculo/{vehiculoId}")]
        [ProducesResponseType(typeof(List<object>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetByVehiculo(int vehiculoId)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_combustibleServiceUrl);
                var client = new AsignacionesService.AsignacionesServiceClient(channel);

                var request = new GetByVehiculoIdRequest { VehiculoId = vehiculoId };
                var asignaciones = new List<AsignacionDto>();
                var call = client.GetByVehiculoId(request);

                await foreach (var asignacion in call.ResponseStream.ReadAllAsync())
                {
                    asignaciones.Add(asignacion);
                }

                return Ok(asignaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener asignaciones por vehículo");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene asignaciones por estado
        /// </summary>
        [HttpGet("estado/{estadoNombre}")]
        [ProducesResponseType(typeof(List<object>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetByEstado(string estadoNombre)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_combustibleServiceUrl);
                var client = new AsignacionesService.AsignacionesServiceClient(channel);

                var request = new GetByEstadoRequest { EstadoNombre = estadoNombre };
                var asignaciones = new List<AsignacionDto>();
                var call = client.GetByEstado(request);

                await foreach (var asignacion in call.ResponseStream.ReadAllAsync())
                {
                    asignaciones.Add(asignacion);
                }

                return Ok(asignaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener asignaciones por estado");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }
    }
}

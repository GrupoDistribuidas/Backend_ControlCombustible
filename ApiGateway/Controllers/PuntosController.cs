using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MS.Rutas.Protos;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.ComponentModel.DataAnnotations;
using GrpcStatusCode = Grpc.Core.StatusCode;

namespace ApiGateway.Controllers
{
    /// <summary>
    /// Controlador para la gestión de puntos (ubicaciones)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class PuntosController : ControllerBase
    {
        private readonly ILogger<PuntosController> _logger;
        private readonly PuntosService.PuntosServiceClient _puntosClient;

        public PuntosController(ILogger<PuntosController> logger, PuntosService.PuntosServiceClient puntosClient)
        {
            _logger = logger;
            _puntosClient = puntosClient;
        }

        [HttpPost]
        public async Task<IActionResult> CrearPunto([FromBody] CrearPuntoRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new 
                { 
                    Success = false,
                    Message = "Datos de entrada inválidos",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            try
            {
                var grpcRequest = new CrearPuntoRequest
                {
                    Nombre = request.Nombre,
                    Direccion = request.Direccion,
                    Provincia = request.Provincia,
                    TipoPunto = request.TipoPunto
                };

                var response = await _puntosClient.CrearPuntoAsync(grpcRequest);

                return Ok(new { Success = true, Message = "Punto creado exitosamente", Data = new { Id = response.Id } });
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error gRPC al crear punto");
                
                if (ex.StatusCode == GrpcStatusCode.InvalidArgument)
                    return BadRequest(new { Success = false, Message = ex.Status.Detail });
                    
                return base.StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear punto");
                return base.StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ListarPuntos()
        {
            try
            {
                // Using injected client

                var puntos = new List<object>();
                using var call = _puntosClient.ListarPuntos(new Empty());

                await foreach (var punto in call.ResponseStream.ReadAllAsync())
                {
                    puntos.Add(new
                    {
                        Id = punto.Id,
                        Nombre = punto.Nombre,
                        Direccion = punto.Direccion,
                        Provincia = punto.Provincia,
                        TipoPunto = punto.TipoPunto
                    });
                }

                return Ok(new { Success = true, Message = "Puntos listados exitosamente", Data = puntos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar puntos");
                return base.StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPuntoById(int id)
        {
            try
            {
                // Using injected client

                var grpcRequest = new GetPuntoByIdRequest { Id = id };
                var punto = await _puntosClient.GetByIdAsync(grpcRequest);

                return Ok(new
                {
                    Success = true,
                    Message = "Punto obtenido exitosamente",
                    Data = new
                    {
                        Id = punto.Id,
                        Nombre = punto.Nombre,
                        Direccion = punto.Direccion,
                        Provincia = punto.Provincia,
                        TipoPunto = punto.TipoPunto
                    }
                });
            }
            catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
            {
                return NotFound(new { Success = false, Message = "Punto no encontrado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener punto por ID: {Id}", id);
                return base.StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarPunto(int id, [FromBody] ActualizarPuntoRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new 
                { 
                    Success = false,
                    Message = "Datos de entrada inválidos",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            try
            {
                var grpcRequest = new ActualizarPuntoRequest
                {
                    Id = id,
                    Nombre = request.Nombre,
                    Direccion = request.Direccion,
                    Provincia = request.Provincia,
                    TipoPunto = request.TipoPunto
                };

                var response = await _puntosClient.ActualizarPuntoAsync(grpcRequest);

                return Ok(new
                {
                    Success = response.Affected > 0,
                    Message = response.Affected > 0 ? "Punto actualizado exitosamente" : "No se pudo actualizar el punto",
                    Data = new { AffectedRows = response.Affected }
                });
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error gRPC al actualizar punto: {Id}", id);
                
                if (ex.StatusCode == GrpcStatusCode.InvalidArgument)
                    return BadRequest(new { Success = false, Message = ex.Status.Detail });
                    
                return base.StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar punto: {Id}", id);
                return base.StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpGet("search/{term}")]
        public async Task<IActionResult> SearchByTerm(string term)
        {
            try
            {
                // Using injected client

                var grpcRequest = new SearchPuntoByTermRequest { Term = term };

                var puntos = new List<object>();
                using var call = _puntosClient.SearchByTerm(grpcRequest);

                await foreach (var punto in call.ResponseStream.ReadAllAsync())
                {
                    puntos.Add(new
                    {
                        Id = punto.Id,
                        Nombre = punto.Nombre,
                        Direccion = punto.Direccion,
                        Provincia = punto.Provincia,
                        TipoPunto = punto.TipoPunto
                    });
                }

                return Ok(new { Success = true, Message = "Búsqueda realizada exitosamente", Data = puntos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda por término: {Term}", term);
                return base.StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }
    }

    // DTOs
    public class CrearPuntoRequestDto
    {
        [Required] public string Nombre { get; set; } = string.Empty;
        [Required] public string Direccion { get; set; } = string.Empty;
        [Required] public string Provincia { get; set; } = string.Empty;
        [Required] public string TipoPunto { get; set; } = string.Empty;
    }

    public class ActualizarPuntoRequestDto
    {
        [Required] public string Nombre { get; set; } = string.Empty;
        [Required] public string Direccion { get; set; } = string.Empty;
        [Required] public string Provincia { get; set; } = string.Empty;
        [Required] public string TipoPunto { get; set; } = string.Empty;
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MS.Rutas.Protos;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.ComponentModel.DataAnnotations;

namespace ApiGateway.Controllers
{
    /// <summary>
    /// Controlador para la gestión de rutas
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class RutasController : ControllerBase
    {
        private readonly ILogger<RutasController> _logger;
        private readonly string _rutasServiceUrl;

        public RutasController(ILogger<RutasController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _rutasServiceUrl = configuration.GetValue<string>("Services:RutasService:Url") ?? "https://localhost:5134";
        }

        /// <summary>
        /// Crea una nueva ruta en el sistema
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CrearRuta([FromBody] CrearRutaRequestDto request)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_rutasServiceUrl);
                var client = new RutasService.RutasServiceClient(channel);

                var grpcRequest = new CrearRutaRequest
                {
                    Nombre = request.Nombre,
                    PuntoInicioId = request.PuntoInicioId,
                    PuntoFinId = request.PuntoFinId,
                    Distancia = request.Distancia
                };

                var response = await client.CrearRutaAsync(grpcRequest);

                return Ok(new
                {
                    Success = true,
                    Message = "Ruta creada exitosamente",
                    Data = new { Id = response.Id }
                });
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error gRPC al crear ruta");
                
                var errorResponse = ex.StatusCode switch
                {
                    Grpc.Core.StatusCode.InvalidArgument => BadRequest(new { Message = ex.Status.Detail }),
                    _ => base.StatusCode(500, new { Message = "Error interno del servidor" })
                };
                
                return errorResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear ruta");
                return base.StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene todas las rutas activas
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListarRutas()
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_rutasServiceUrl);
                var client = new RutasService.RutasServiceClient(channel);

                var rutas = new List<object>();

                using var call = client.ListarRutas(new Empty());

                await foreach (var ruta in call.ResponseStream.ReadAllAsync())
                {
                    rutas.Add(new
                    {
                        Id = ruta.Id,
                        Nombre = ruta.Nombre,
                        PuntoInicioId = ruta.PuntoInicioId,
                        PuntoFinId = ruta.PuntoFinId,
                        Distancia = ruta.Distancia,
                        Estado = ruta.Estado
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Rutas listadas exitosamente",
                    Data = rutas
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar rutas");
                return base.StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene todas las rutas (incluyendo inactivas)
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> ListarTodasRutas()
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_rutasServiceUrl);
                var client = new RutasService.RutasServiceClient(channel);

                var rutas = new List<object>();

                using var call = client.ListarTodasRutas(new Empty());

                await foreach (var ruta in call.ResponseStream.ReadAllAsync())
                {
                    rutas.Add(new
                    {
                        Id = ruta.Id,
                        Nombre = ruta.Nombre,
                        PuntoInicioId = ruta.PuntoInicioId,
                        PuntoFinId = ruta.PuntoFinId,
                        Distancia = ruta.Distancia,
                        Estado = ruta.Estado
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Todas las rutas listadas exitosamente",
                    Data = rutas
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar todas las rutas");
                return base.StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene una ruta por su ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRutaById(int id)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_rutasServiceUrl);
                var client = new RutasService.RutasServiceClient(channel);

                var grpcRequest = new MS.Rutas.Protos.GetByIdRequest { Id = id };
                var ruta = await client.GetByIdAsync(grpcRequest);

                return Ok(new
                {
                    Success = true,
                    Message = "Ruta obtenida exitosamente",
                    Data = new
                    {
                        Id = ruta.Id,
                        Nombre = ruta.Nombre,
                        PuntoInicioId = ruta.PuntoInicioId,
                        PuntoFinId = ruta.PuntoFinId,
                        Distancia = ruta.Distancia,
                        Estado = ruta.Estado
                    }
                });
            }
            catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
            {
                return NotFound(new { Message = "Ruta no encontrada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ruta por ID: {Id}", id);
                return base.StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza los datos de una ruta existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarRuta(int id, [FromBody] ActualizarRutaRequestDto request)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_rutasServiceUrl);
                var client = new RutasService.RutasServiceClient(channel);

                var grpcRequest = new ActualizarRutaRequest
                {
                    Id = id,
                    Nombre = request.Nombre,
                    PuntoInicioId = request.PuntoInicioId,
                    PuntoFinId = request.PuntoFinId,
                    Distancia = request.Distancia,
                    Estado = request.Estado
                };

                var response = await client.ActualizarRutaAsync(grpcRequest);

                return Ok(new
                {
                    Success = response.Affected > 0,
                    Message = response.Affected > 0 ? "Ruta actualizada exitosamente" : "No se pudo actualizar la ruta",
                    Data = new { AffectedRows = response.Affected }
                });
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error gRPC al actualizar ruta: {Id}", id);
                
                var errorResponse = ex.StatusCode switch
                {
                    Grpc.Core.StatusCode.InvalidArgument => BadRequest(new { Message = ex.Status.Detail }),
                    _ => base.StatusCode(500, new { Message = "Error interno del servidor" })
                };
                
                return errorResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar ruta: {Id}", id);
                return base.StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza únicamente el estado de una ruta (activo/inactivo)
        /// </summary>
        [HttpPatch("{id}/estado")]
        public async Task<IActionResult> ActualizarEstadoRuta(int id, [FromBody] ActualizarEstadoRutaRequestDto request)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_rutasServiceUrl);
                var client = new RutasService.RutasServiceClient(channel);

                var grpcRequest = new MS.Rutas.Protos.ActualizarEstadoRequest
                {
                    Id = id,
                    Estado = request.Estado
                };

                var response = await client.ActualizarEstadoRutaAsync(grpcRequest);

                return Ok(new
                {
                    Success = response.Affected > 0,
                    Message = response.Affected > 0 ? "Estado actualizado exitosamente" : "No se pudo actualizar el estado",
                    Data = new { AffectedRows = response.Affected }
                });
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error gRPC al actualizar estado de ruta: {Id}", id);
                
                var errorResponse = ex.StatusCode switch
                {
                    Grpc.Core.StatusCode.InvalidArgument => BadRequest(new { Message = ex.Status.Detail }),
                    _ => base.StatusCode(500, new { Message = "Error interno del servidor" })
                };
                
                return errorResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado de ruta: {Id}", id);
                return base.StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Busca rutas utilizando un término general de búsqueda
        /// </summary>
        [HttpGet("search/{term}")]
        public async Task<IActionResult> SearchByTerm(string term)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_rutasServiceUrl);
                var client = new RutasService.RutasServiceClient(channel);

                var grpcRequest = new MS.Rutas.Protos.SearchByTermRequest { Term = term };

                var rutas = new List<object>();

                using var call = client.SearchByTerm(grpcRequest);

                await foreach (var ruta in call.ResponseStream.ReadAllAsync())
                {
                    rutas.Add(new
                    {
                        Id = ruta.Id,
                        Nombre = ruta.Nombre,
                        PuntoInicioId = ruta.PuntoInicioId,
                        PuntoFinId = ruta.PuntoFinId,
                        Distancia = ruta.Distancia,
                        Estado = ruta.Estado
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Búsqueda realizada exitosamente",
                    Data = rutas
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda por término: {Term}", term);
                return base.StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }
    }

    // ========== DTOs ==========

    public class CrearRutaRequestDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El punto de inicio es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del punto de inicio debe ser mayor a 0")]
        public int PuntoInicioId { get; set; }

        [Required(ErrorMessage = "El punto final es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del punto final debe ser mayor a 0")]
        public int PuntoFinId { get; set; }

        [Required(ErrorMessage = "La distancia es requerida")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La distancia debe ser mayor a 0")]
        public double Distancia { get; set; }
    }

    public class ActualizarRutaRequestDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El punto de inicio es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del punto de inicio debe ser mayor a 0")]
        public int PuntoInicioId { get; set; }

        [Required(ErrorMessage = "El punto final es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del punto final debe ser mayor a 0")]
        public int PuntoFinId { get; set; }

        [Required(ErrorMessage = "La distancia es requerida")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La distancia debe ser mayor a 0")]
        public double Distancia { get; set; }

        public bool? Estado { get; set; }
    }

    public class ActualizarEstadoRutaRequestDto
    {
        [Required(ErrorMessage = "El estado es requerido")]
        public bool Estado { get; set; }
    }
}

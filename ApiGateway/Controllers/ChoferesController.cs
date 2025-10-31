using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MS.Choferes.Protos;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcStatusCode = Grpc.Core.StatusCode;
using System.ComponentModel.DataAnnotations;

namespace ApiGateway.Controllers
{
    /// <summary>
    /// Controlador para la gestión de choferes/conductores
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class ChoferesController : ControllerBase
    {
        private readonly ILogger<ChoferesController> _logger;
        private readonly ChoferesService.ChoferesServiceClient _choferesClient;

        public ChoferesController(
            ILogger<ChoferesController> logger, 
            ChoferesService.ChoferesServiceClient choferesClient)
        {
            _logger = logger;
            _choferesClient = choferesClient;
        }

        /// <summary>
        /// Crea un nuevo chofer en el sistema
        /// </summary>
        /// <param name="request">Datos del chofer a crear</param>
        /// <returns>ID del chofer creado</returns>
        /// <response code="200">Chofer creado exitosamente</response>
        /// <response code="400">Datos inválidos, usuario no existe o ya está asignado</response>
        /// <response code="401">Token JWT no válido o ausente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CrearChofer([FromBody] CrearChoferRequestDto request)
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
                var grpcRequest = new CrearChoferRequest
                {
                    PrimerNombre = request.PrimerNombre,
                    SegundoNombre = request.SegundoNombre ?? string.Empty,
                    PrimerApellido = request.PrimerApellido,
                    SegundoApellido = request.SegundoApellido ?? string.Empty,
                    Identificacion = request.Identificacion,
                    FechaNacimiento = request.FechaNacimiento.ToString("yyyy-MM-dd"),
                    Disponible = request.Disponible,
                    UsuarioId = request.UsuarioId ?? 0,
                    TipoMaquinariaId = request.TipoMaquinariaId
                };

                var response = await _choferesClient.CrearChoferAsync(grpcRequest);

                return Ok(new
                {
                    Success = true,
                    Message = "Chofer creado exitosamente",
                    Data = new { Id = response.Id }
                });
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error gRPC al crear chofer");
                
                if (ex.StatusCode == GrpcStatusCode.InvalidArgument)
                    return BadRequest(new { Success = false, Message = ex.Status.Detail });
                    
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear chofer");
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene todos los choferes registrados en el sistema
        /// </summary>
        /// <returns>Lista completa de choferes con sus detalles</returns>
        /// <response code="200">Lista de choferes obtenida exitosamente</response>
        /// <response code="401">Token JWT no válido o ausente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<object>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ListarChoferes()
        {
            try
            {
                var choferes = new List<object>();

                using var call = _choferesClient.ListarChoferes(new Empty());

                await foreach (var chofer in call.ResponseStream.ReadAllAsync())
                {
                    choferes.Add(new
                    {
                        Id = chofer.Id,
                        PrimerNombre = chofer.PrimerNombre,
                        SegundoNombre = chofer.SegundoNombre,
                        PrimerApellido = chofer.PrimerApellido,
                        SegundoApellido = chofer.SegundoApellido,
                        NombreCompleto = chofer.NombreCompleto,
                        Identificacion = chofer.Identificacion,
                        FechaNacimiento = chofer.FechaNacimiento,
                        Disponible = chofer.Disponible,
                        UsuarioId = chofer.UsuarioId,
                        TipoMaquinariaId = chofer.TipoMaquinariaId,
                        Estado = chofer.Estado
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Choferes listados exitosamente",
                    Data = choferes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar choferes");
                return base.StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene todos los choferes (incluyendo inactivos)
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> ListarTodosChoferes()
        {
            try
            {
                var choferes = new List<object>();

                using var call = _choferesClient.ListarTodosChoferes(new Empty());

                await foreach (var chofer in call.ResponseStream.ReadAllAsync())
                {
                    choferes.Add(new
                    {
                        Id = chofer.Id,
                        PrimerNombre = chofer.PrimerNombre,
                        SegundoNombre = chofer.SegundoNombre,
                        PrimerApellido = chofer.PrimerApellido,
                        SegundoApellido = chofer.SegundoApellido,
                        NombreCompleto = chofer.NombreCompleto,
                        Identificacion = chofer.Identificacion,
                        FechaNacimiento = chofer.FechaNacimiento,
                        Disponible = chofer.Disponible,
                        UsuarioId = chofer.UsuarioId,
                        TipoMaquinariaId = chofer.TipoMaquinariaId,
                        Estado = chofer.Estado
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Todos los choferes listados exitosamente",
                    Data = choferes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar todos los choferes");
                return base.StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza los datos de un chofer existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarChofer(int id, [FromBody] ActualizarChoferRequestDto request)
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
                var grpcRequest = new ActualizarChoferRequest
                {
                    Id = id,
                    PrimerNombre = request.PrimerNombre,
                    SegundoNombre = request.SegundoNombre ?? string.Empty,
                    PrimerApellido = request.PrimerApellido,
                    SegundoApellido = request.SegundoApellido ?? string.Empty,
                    Identificacion = request.Identificacion,
                    FechaNacimiento = request.FechaNacimiento.ToString("yyyy-MM-dd"),
                    Disponible = request.Disponible,
                    UsuarioId = request.UsuarioId ?? 0,
                    TipoMaquinariaId = request.TipoMaquinariaId,
                    Estado = request.Estado
                };

                var response = await _choferesClient.ActualizarChoferAsync(grpcRequest);

                return Ok(new
                {
                    Success = response.Affected > 0,
                    Message = response.Affected > 0 ? "Chofer actualizado exitosamente" : "No se pudo actualizar el chofer",
                    Data = new { AffectedRows = response.Affected }
                });
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error gRPC al actualizar chofer: {Id}", id);
                
                var errorResponse = ex.StatusCode switch
                {
                    GrpcStatusCode.InvalidArgument => BadRequest(new { Success = false, Message = ex.Status.Detail }),
                    _ => base.StatusCode(500, new { Success = false, Message = "Error interno del servidor" })
                };
                
                return errorResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar chofer: {Id}", id);
                return base.StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpPatch("{id}/estado")]
        public async Task<IActionResult> ActualizarEstadoChofer(int id, [FromBody] ActualizarEstadoChoferRequestDto request)
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
                var grpcRequest = new ActualizarEstadoRequest
                {
                    Id = id,
                    Estado = request.Estado
                };

                var response = await _choferesClient.ActualizarEstadoChoferAsync(grpcRequest);

                return Ok(new
                {
                    Success = response.Affected > 0,
                    Message = response.Affected > 0 ? "Estado actualizado exitosamente" : "No se pudo actualizar el estado",
                    Data = new { AffectedRows = response.Affected }
                });
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error gRPC al actualizar estado del chofer: {Id}", id);
                
                var errorResponse = ex.StatusCode switch
                {
                    GrpcStatusCode.InvalidArgument => BadRequest(new { Success = false, Message = ex.Status.Detail }),
                    _ => base.StatusCode(500, new { Success = false, Message = "Error interno del servidor" })
                };
                
                return errorResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado del chofer: {Id}", id);
                return base.StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpPatch("{id}/disponibilidad")]
        public async Task<IActionResult> ActualizarDisponibilidadChofer(int id, [FromBody] ActualizarDisponibilidadChoferRequestDto request)
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
                var grpcRequest = new ActualizarDisponibilidadRequest
                {
                    Id = id,
                    Disponible = request.Disponible
                };

                var response = await _choferesClient.ActualizarDisponibilidadChoferAsync(grpcRequest);

                return Ok(new
                {
                    Success = response.Affected > 0,
                    Message = response.Affected > 0 ? "Disponibilidad actualizada exitosamente" : "No se pudo actualizar la disponibilidad",
                    Data = new { AffectedRows = response.Affected }
                });
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error gRPC al actualizar disponibilidad del chofer: {Id}", id);
                
                var errorResponse = ex.StatusCode switch
                {
                    GrpcStatusCode.InvalidArgument => BadRequest(new { Success = false, Message = ex.Status.Detail }),
                    _ => base.StatusCode(500, new { Success = false, Message = "Error interno del servidor" })
                };
                
                return errorResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar disponibilidad del chofer: {Id}", id);
                return base.StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpPatch("{choferId}/asignar-usuario")]
        public async Task<IActionResult> AsignarUsuario(int choferId, [FromBody] AsignarUsuarioRequestDto request)
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
                var grpcRequest = new AsignarUsuarioRequest
                {
                    ChoferId = choferId,
                    UsuarioId = request.UsuarioId
                };

                var response = await _choferesClient.AsignarUsuarioAsync(grpcRequest);

                return Ok(new
                {
                    Success = response.Affected > 0,
                    Message = response.Affected > 0 ? "Usuario asignado exitosamente al chofer" : "No se pudo asignar el usuario",
                    Data = new { AffectedRows = response.Affected }
                });
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error gRPC al asignar usuario al chofer: {ChoferId}", choferId);
                
                var errorResponse = ex.StatusCode switch
                {
                    GrpcStatusCode.InvalidArgument => BadRequest(new { Success = false, Message = ex.Status.Detail }),
                    GrpcStatusCode.NotFound => NotFound(new { Success = false, Message = ex.Status.Detail }),
                    _ => base.StatusCode(500, new { Success = false, Message = "Error interno del servidor" })
                };
                
                return errorResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar usuario al chofer: {ChoferId}", choferId);
                return base.StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] bool? estado = null,
            [FromQuery] int? tipoMaquinariaId = null,
            [FromQuery] bool? disponible = null,
            [FromQuery] string? fechaNacimientoDesde = null,
            [FromQuery] string? fechaNacimientoHasta = null)
        {
            try
            {
                var grpcRequest = new SearchChoferRequest();

                if (estado.HasValue)
                    grpcRequest.Estado = estado.Value;
                if (tipoMaquinariaId.HasValue)
                    grpcRequest.TipoMaquinariaId = tipoMaquinariaId.Value;
                if (disponible.HasValue)
                    grpcRequest.Disponible = disponible.Value;
                if (!string.IsNullOrWhiteSpace(fechaNacimientoDesde))
                    grpcRequest.FechaNacimientoDesde = fechaNacimientoDesde;
                if (!string.IsNullOrWhiteSpace(fechaNacimientoHasta))
                    grpcRequest.FechaNacimientoHasta = fechaNacimientoHasta;

                var choferes = new List<object>();

                using var call = _choferesClient.Search(grpcRequest);

                await foreach (var chofer in call.ResponseStream.ReadAllAsync())
                {
                    choferes.Add(new
                    {
                        Id = chofer.Id,
                        PrimerNombre = chofer.PrimerNombre,
                        SegundoNombre = chofer.SegundoNombre,
                        PrimerApellido = chofer.PrimerApellido,
                        SegundoApellido = chofer.SegundoApellido,
                        NombreCompleto = chofer.NombreCompleto,
                        Identificacion = chofer.Identificacion,
                        FechaNacimiento = chofer.FechaNacimiento,
                        Disponible = chofer.Disponible,
                        UsuarioId = chofer.UsuarioId,
                        TipoMaquinariaId = chofer.TipoMaquinariaId
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Búsqueda realizada exitosamente",
                    Data = choferes
                });
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error gRPC en búsqueda de choferes");
                
                var errorResponse = ex.StatusCode switch
                {
                    GrpcStatusCode.InvalidArgument => BadRequest(new { Success = false, Message = ex.Status.Detail }),
                    _ => base.StatusCode(500, new { Success = false, Message = "Error interno del servidor" })
                };
                
                return errorResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda de choferes");
                return base.StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpGet("search/{term}")]
        public async Task<IActionResult> SearchByTerm(string term)
        {
            try
            {
                var grpcRequest = new SearchByTermRequest { Term = term };

                var choferes = new List<object>();

                using var call = _choferesClient.SearchByTerm(grpcRequest);

                await foreach (var chofer in call.ResponseStream.ReadAllAsync())
                {
                    choferes.Add(new
                    {
                        Id = chofer.Id,
                        PrimerNombre = chofer.PrimerNombre,
                        SegundoNombre = chofer.SegundoNombre,
                        PrimerApellido = chofer.PrimerApellido,
                        SegundoApellido = chofer.SegundoApellido,
                        NombreCompleto = chofer.NombreCompleto,
                        Identificacion = chofer.Identificacion,
                        FechaNacimiento = chofer.FechaNacimiento,
                        Disponible = chofer.Disponible,
                        UsuarioId = chofer.UsuarioId,
                        TipoMaquinariaId = chofer.TipoMaquinariaId
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Búsqueda realizada exitosamente",
                    Data = choferes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda por término: {Term}", term);
                return base.StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }
    }

    // ========== DTOs ==========

    public class CrearChoferRequestDto
    {
        [Required(ErrorMessage = "El primer nombre es requerido")]
        public string PrimerNombre { get; set; } = string.Empty;

        public string? SegundoNombre { get; set; }

        [Required(ErrorMessage = "El primer apellido es requerido")]
        public string PrimerApellido { get; set; } = string.Empty;

        public string? SegundoApellido { get; set; }

        [Required(ErrorMessage = "La identificación es requerida")]
        public string Identificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de nacimiento es requerida")]
        public DateTime FechaNacimiento { get; set; }

        [Required(ErrorMessage = "La disponibilidad es requerida")]
        public bool Disponible { get; set; }

        public int? UsuarioId { get; set; }

        [Required(ErrorMessage = "El tipo de maquinaria es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de tipo de maquinaria debe ser mayor a 0")]
        public int TipoMaquinariaId { get; set; }
    }

    public class ActualizarChoferRequestDto
    {
        [Required(ErrorMessage = "El primer nombre es requerido")]
        public string PrimerNombre { get; set; } = string.Empty;

        public string? SegundoNombre { get; set; }

        [Required(ErrorMessage = "El primer apellido es requerido")]
        public string PrimerApellido { get; set; } = string.Empty;

        public string? SegundoApellido { get; set; }

        [Required(ErrorMessage = "La identificación es requerida")]
        public string Identificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de nacimiento es requerida")]
        public DateTime FechaNacimiento { get; set; }

        [Required(ErrorMessage = "La disponibilidad es requerida")]
        public bool Disponible { get; set; }

        public int? UsuarioId { get; set; }

        [Required(ErrorMessage = "El tipo de maquinaria es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de tipo de maquinaria debe ser mayor a 0")]
        public int TipoMaquinariaId { get; set; }

        public bool? Estado { get; set; }
    }

    public class ActualizarEstadoChoferRequestDto
    {
        [Required(ErrorMessage = "El estado es requerido")]
        public bool Estado { get; set; }
    }

    public class ActualizarDisponibilidadChoferRequestDto
    {
        [Required(ErrorMessage = "La disponibilidad es requerida")]
        public bool Disponible { get; set; }
    }

    public class AsignarUsuarioRequestDto
    {
        [Required(ErrorMessage = "El ID de usuario es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de usuario debe ser mayor a 0")]
        public int UsuarioId { get; set; }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MS.Choferes.Protos;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
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
        private readonly string _choferesServiceUrl;

        public ChoferesController(ILogger<ChoferesController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _choferesServiceUrl = configuration.GetValue<string>("Services:ChoferesService:Url") ?? "https://localhost:5133";
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
            try
            {
                using var channel = GrpcChannel.ForAddress(_choferesServiceUrl);
                var client = new ChoferesService.ChoferesServiceClient(channel);

                var grpcRequest = new CrearChoferRequest
                {
                    PrimerNombre = request.PrimerNombre,
                    SegundoNombre = request.SegundoNombre ?? string.Empty,
                    PrimerApellido = request.PrimerApellido,
                    SegundoApellido = request.SegundoApellido ?? string.Empty,
                    Identificacion = request.Identificacion,
                    FechaNacimiento = request.FechaNacimiento.ToString("yyyy-MM-dd"),
                    Disponible = request.Disponible,
                    UsuarioId = request.UsuarioId ?? 0, // 0 indica que no tiene usuario asignado
                    TipoMaquinariaId = request.TipoMaquinariaId
                };

                var response = await client.CrearChoferAsync(grpcRequest);

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
                
                var errorResponse = ex.StatusCode switch
                {
                    Grpc.Core.StatusCode.InvalidArgument => BadRequest(new { Message = ex.Status.Detail }),
                    _ => base.StatusCode(500, new { Message = "Error interno del servidor" })
                };
                
                return errorResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear chofer");
                return base.StatusCode(500, new { Message = "Error interno del servidor" });
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
                using var channel = GrpcChannel.ForAddress(_choferesServiceUrl);
                var client = new ChoferesService.ChoferesServiceClient(channel);

                var choferes = new List<object>();

                using var call = client.ListarChoferes(new Empty());

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
                    Message = "Choferes listados exitosamente",
                    Data = choferes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar choferes");
                return base.StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza los datos de un chofer existente
        /// </summary>
        /// <param name="id">ID único del chofer a actualizar</param>
        /// <param name="request">Datos actualizados del chofer</param>
        /// <returns>Resultado de la operación de actualización</returns>
        /// <response code="200">Chofer actualizado exitosamente</response>
        /// <response code="400">Datos inválidos, usuario no existe o ya está asignado</response>
        /// <response code="401">Token JWT no válido o ausente</response>
        /// <response code="404">Chofer no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ActualizarChofer(int id, [FromBody] ActualizarChoferRequestDto request)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_choferesServiceUrl);
                var client = new ChoferesService.ChoferesServiceClient(channel);

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
                    UsuarioId = request.UsuarioId ?? 0, // 0 indica sin usuario asignado
                    TipoMaquinariaId = request.TipoMaquinariaId,
                    Estado = request.Estado
                };

                var response = await client.ActualizarChoferAsync(grpcRequest);

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
                    Grpc.Core.StatusCode.InvalidArgument => BadRequest(new { Message = ex.Status.Detail }),
                    _ => base.StatusCode(500, new { Message = "Error interno del servidor" })
                };
                
                return errorResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar chofer: {Id}", id);
                return base.StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza únicamente el estado de un chofer (activo/inactivo)
        /// </summary>
        /// <param name="id">ID único del chofer</param>
        /// <param name="request">Nuevo estado del chofer</param>
        /// <returns>Resultado de la actualización del estado</returns>
        /// <response code="200">Estado actualizado exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="401">Token JWT no válido o ausente</response>
        /// <response code="404">Chofer no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPatch("{id}/estado")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ActualizarEstadoChofer(int id, [FromBody] ActualizarEstadoChoferRequestDto request)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_choferesServiceUrl);
                var client = new ChoferesService.ChoferesServiceClient(channel);

                var grpcRequest = new ActualizarEstadoRequest
                {
                    Id = id,
                    Estado = request.Estado
                };

                var response = await client.ActualizarEstadoChoferAsync(grpcRequest);

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
                    Grpc.Core.StatusCode.InvalidArgument => BadRequest(new { Message = ex.Status.Detail }),
                    _ => base.StatusCode(500, new { Message = "Error interno del servidor" })
                };
                
                return errorResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado del chofer: {Id}", id);
                return base.StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza únicamente la disponibilidad de un chofer
        /// </summary>
        /// <param name="id">ID único del chofer</param>
        /// <param name="request">Nueva disponibilidad del chofer</param>
        /// <returns>Resultado de la actualización de disponibilidad</returns>
        /// <response code="200">Disponibilidad actualizada exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="401">Token JWT no válido o ausente</response>
        /// <response code="404">Chofer no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPatch("{id}/disponibilidad")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ActualizarDisponibilidadChofer(int id, [FromBody] ActualizarDisponibilidadChoferRequestDto request)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_choferesServiceUrl);
                var client = new ChoferesService.ChoferesServiceClient(channel);

                var grpcRequest = new ActualizarDisponibilidadRequest
                {
                    Id = id,
                    Disponible = request.Disponible
                };

                var response = await client.ActualizarDisponibilidadChoferAsync(grpcRequest);

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
                    Grpc.Core.StatusCode.InvalidArgument => BadRequest(new { Message = ex.Status.Detail }),
                    _ => base.StatusCode(500, new { Message = "Error interno del servidor" })
                };
                
                return errorResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar disponibilidad del chofer: {Id}", id);
                return base.StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Asigna un usuario existente a un chofer
        /// </summary>
        /// <param name="choferId">ID único del chofer</param>
        /// <param name="request">ID del usuario a asignar</param>
        /// <returns>Resultado de la asignación</returns>
        /// <response code="200">Usuario asignado exitosamente</response>
        /// <response code="400">Usuario no existe o ya está asignado a otro chofer</response>
        /// <response code="401">Token JWT no válido o ausente</response>
        /// <response code="404">Chofer no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPatch("{choferId}/asignar-usuario")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> AsignarUsuario(int choferId, [FromBody] AsignarUsuarioRequestDto request)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_choferesServiceUrl);
                var client = new ChoferesService.ChoferesServiceClient(channel);

                var grpcRequest = new AsignarUsuarioRequest
                {
                    ChoferId = choferId,
                    UsuarioId = request.UsuarioId
                };

                var response = await client.AsignarUsuarioAsync(grpcRequest);

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
                    Grpc.Core.StatusCode.InvalidArgument => BadRequest(new { Message = ex.Status.Detail }),
                    Grpc.Core.StatusCode.NotFound => NotFound(new { Message = ex.Status.Detail }),
                    _ => base.StatusCode(500, new { Message = "Error interno del servidor" })
                };
                
                return errorResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar usuario al chofer: {ChoferId}", choferId);
                return base.StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Busca choferes utilizando filtros avanzados
        /// </summary>
        /// <param name="estado">Estado del chofer (activo/inactivo) - opcional</param>
        /// <param name="tipoMaquinariaId">ID del tipo de maquinaria - opcional</param>
        /// <param name="disponible">Disponibilidad del chofer - opcional</param>
        /// <param name="fechaNacimientoDesde">Fecha de nacimiento desde (yyyy-MM-dd) - opcional</param>
        /// <param name="fechaNacimientoHasta">Fecha de nacimiento hasta (yyyy-MM-dd) - opcional</param>
        /// <returns>Lista filtrada de choferes que coinciden con los criterios</returns>
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
            [FromQuery] bool? disponible = null,
            [FromQuery] string? fechaNacimientoDesde = null,
            [FromQuery] string? fechaNacimientoHasta = null)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_choferesServiceUrl);
                var client = new ChoferesService.ChoferesServiceClient(channel);

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

                using var call = client.Search(grpcRequest);

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
                    Grpc.Core.StatusCode.InvalidArgument => BadRequest(new { Message = ex.Status.Detail }),
                    _ => base.StatusCode(500, new { Message = "Error interno del servidor" })
                };
                
                return errorResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda de choferes");
                return base.StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Busca choferes utilizando un término general de búsqueda
        /// </summary>
        /// <param name="term">Término de búsqueda (busca en nombres, apellidos, identificación)</param>
        /// <returns>Lista de choferes que coinciden con el término de búsqueda</returns>
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
                using var channel = GrpcChannel.ForAddress(_choferesServiceUrl);
                var client = new ChoferesService.ChoferesServiceClient(channel);

                var grpcRequest = new SearchByTermRequest { Term = term };

                var choferes = new List<object>();

                using var call = client.SearchByTerm(grpcRequest);

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
                return base.StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }
    }

    // ========== DTOs ==========

    /// <summary>
    /// Datos requeridos para crear un nuevo chofer
    /// </summary>
    public class CrearChoferRequestDto
    {
        /// <summary>
        /// Primer nombre del chofer
        /// </summary>
        /// <example>Juan</example>
        [Required(ErrorMessage = "El primer nombre es requerido")]
        public string PrimerNombre { get; set; } = string.Empty;

        /// <summary>
        /// Segundo nombre del chofer (opcional)
        /// </summary>
        /// <example>Carlos</example>
        public string? SegundoNombre { get; set; }

        /// <summary>
        /// Primer apellido del chofer
        /// </summary>
        /// <example>Pérez</example>
        [Required(ErrorMessage = "El primer apellido es requerido")]
        public string PrimerApellido { get; set; } = string.Empty;

        /// <summary>
        /// Segundo apellido del chofer (opcional)
        /// </summary>
        /// <example>González</example>
        public string? SegundoApellido { get; set; }

        /// <summary>
        /// Número de identificación único del chofer
        /// </summary>
        /// <example>0123456789</example>
        [Required(ErrorMessage = "La identificación es requerida")]
        public string Identificacion { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de nacimiento del chofer (debe ser mayor de 18 años)
        /// </summary>
        /// <example>1990-05-15</example>
        [Required(ErrorMessage = "La fecha de nacimiento es requerida")]
        public DateTime FechaNacimiento { get; set; }

        /// <summary>
        /// Indica si el chofer está disponible para asignación
        /// </summary>
        /// <example>true</example>
        [Required(ErrorMessage = "La disponibilidad es requerida")]
        public bool Disponible { get; set; }

        /// <summary>
        /// ID del usuario asociado al chofer (opcional - se puede asignar posteriormente)
        /// </summary>
        /// <example>5</example>
        public int? UsuarioId { get; set; }

        /// <summary>
        /// ID del tipo de maquinaria que puede operar el chofer
        /// </summary>
        /// <example>1</example>
        [Required(ErrorMessage = "El tipo de maquinaria es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de tipo de maquinaria debe ser mayor a 0")]
        public int TipoMaquinariaId { get; set; }
    }

    /// <summary>
    /// Datos para actualizar un chofer existente
    /// </summary>
    public class ActualizarChoferRequestDto
    {
        /// <summary>
        /// Primer nombre del chofer
        /// </summary>
        /// <example>Juan</example>
        [Required(ErrorMessage = "El primer nombre es requerido")]
        public string PrimerNombre { get; set; } = string.Empty;

        /// <summary>
        /// Segundo nombre del chofer (opcional)
        /// </summary>
        /// <example>Carlos</example>
        public string? SegundoNombre { get; set; }

        /// <summary>
        /// Primer apellido del chofer
        /// </summary>
        /// <example>Pérez</example>
        [Required(ErrorMessage = "El primer apellido es requerido")]
        public string PrimerApellido { get; set; } = string.Empty;

        /// <summary>
        /// Segundo apellido del chofer (opcional)
        /// </summary>
        /// <example>González</example>
        public string? SegundoApellido { get; set; }

        /// <summary>
        /// Número de identificación único del chofer
        /// </summary>
        /// <example>0123456789</example>
        [Required(ErrorMessage = "La identificación es requerida")]
        public string Identificacion { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de nacimiento del chofer
        /// </summary>
        /// <example>1990-05-15</example>
        [Required(ErrorMessage = "La fecha de nacimiento es requerida")]
        public DateTime FechaNacimiento { get; set; }

        /// <summary>
        /// Indica si el chofer está disponible
        /// </summary>
        /// <example>true</example>
        [Required(ErrorMessage = "La disponibilidad es requerida")]
        public bool Disponible { get; set; }

        /// <summary>
        /// ID del usuario asociado (opcional)
        /// </summary>
        /// <example>5</example>
        public int? UsuarioId { get; set; }

        /// <summary>
        /// ID del tipo de maquinaria
        /// </summary>
        /// <example>1</example>
        [Required(ErrorMessage = "El tipo de maquinaria es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de tipo de maquinaria debe ser mayor a 0")]
        public int TipoMaquinariaId { get; set; }

        /// <summary>
        /// Estado activo/inactivo del chofer (opcional)
        /// </summary>
        /// <example>true</example>
        public bool? Estado { get; set; }
    }

    /// <summary>
    /// Datos para actualizar el estado de un chofer
    /// </summary>
    public class ActualizarEstadoChoferRequestDto
    {
        /// <summary>
        /// Nuevo estado del chofer (true = activo, false = inactivo)
        /// </summary>
        /// <example>true</example>
        [Required(ErrorMessage = "El estado es requerido")]
        public bool Estado { get; set; }
    }

    /// <summary>
    /// Datos para actualizar la disponibilidad de un chofer
    /// </summary>
    public class ActualizarDisponibilidadChoferRequestDto
    {
        /// <summary>
        /// Nueva disponibilidad del chofer (true = disponible, false = no disponible)
        /// </summary>
        /// <example>true</example>
        [Required(ErrorMessage = "La disponibilidad es requerida")]
        public bool Disponible { get; set; }
    }

    /// <summary>
    /// Datos para asignar un usuario a un chofer
    /// </summary>
    public class AsignarUsuarioRequestDto
    {
        /// <summary>
        /// ID del usuario a asignar al chofer
        /// </summary>
        /// <example>5</example>
        [Required(ErrorMessage = "El ID de usuario es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de usuario debe ser mayor a 0")]
        public int UsuarioId { get; set; }
    }
}

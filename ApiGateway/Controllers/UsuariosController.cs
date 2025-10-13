using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MS.Autenticacion.Grpc;
using Grpc.Net.Client;
using Grpc.Core;
using System.ComponentModel.DataAnnotations;

namespace ApiGateway.Controllers
{
    /// <summary>
    /// Controlador para la gestión de usuarios del sistema
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class UsuariosController : ControllerBase
    {
        private readonly ILogger<UsuariosController> _logger;
        private readonly string _authServiceUrl;

        public UsuariosController(ILogger<UsuariosController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _authServiceUrl = configuration.GetValue<string>("Services:AuthService:Url") ?? "https://localhost:5235";
        }

        /// <summary>
        /// Crea un nuevo usuario en el sistema
        /// </summary>
        /// <param name="request">Datos del usuario a crear</param>
        /// <returns>ID del usuario creado</returns>
        /// <response code="200">Usuario creado exitosamente</response>
        /// <response code="400">Datos inválidos, email o username ya registrados</response>
        /// <response code="401">Token JWT no válido o ausente</response>
        /// <response code="404">Rol especificado no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CrearUsuario([FromBody] CrearUsuarioRequestDto request)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_authServiceUrl);
                var client = new UserService.UserServiceClient(channel);

                var grpcRequest = new CrearUsuarioRequest
                {
                    Email = request.Email,
                    NombreUsuario = request.NombreUsuario,
                    Password = request.Password,
                    RolId = request.RolId
                };

                var response = await client.CrearUsuarioAsync(grpcRequest);

                return Ok(new
                {
                    Success = response.Success,
                    Message = response.Message,
                    Data = new { UsuarioId = response.UsuarioId }
                });
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error gRPC al crear usuario");
                
                var errorResponse = ex.StatusCode switch
                {
                    Grpc.Core.StatusCode.InvalidArgument => BadRequest(new { Message = ex.Status.Detail }),
                    Grpc.Core.StatusCode.AlreadyExists => BadRequest(new { Message = ex.Status.Detail }),
                    Grpc.Core.StatusCode.NotFound => NotFound(new { Message = ex.Status.Detail }),
                    _ => base.StatusCode(500, new { Message = "Error interno del servidor" })
                };
                
                return errorResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene todos los usuarios registrados en el sistema
        /// </summary>
        /// <returns>Lista completa de usuarios con sus detalles</returns>
        /// <response code="200">Lista de usuarios obtenida exitosamente</response>
        /// <response code="401">Token JWT no válido o ausente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<object>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ListarUsuarios()
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_authServiceUrl);
                var client = new UserService.UserServiceClient(channel);

                var usuarios = new List<object>();

                using var call = client.ListarUsuarios(new ListarUsuariosRequest());

                await foreach (var usuario in call.ResponseStream.ReadAllAsync())
                {
                    usuarios.Add(new
                    {
                        Id = usuario.Id,
                        Email = usuario.Email,
                        NombreUsuario = usuario.NombreUsuario,
                        RolId = usuario.RolId,
                        RolNombre = usuario.RolNombre,
                        Estado = usuario.Estado,
                        FechaCreacion = usuario.FechaCreacion,
                        FechaModificacion = usuario.FechaModificacion,
                        UltimoAcceso = usuario.UltimoAcceso
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Usuarios listados exitosamente",
                    Data = usuarios
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar usuarios");
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene los detalles de un usuario específico por su ID
        /// </summary>
        /// <param name="id">ID único del usuario</param>
        /// <returns>Detalles completos del usuario</returns>
        /// <response code="200">Usuario encontrado exitosamente</response>
        /// <response code="401">Token JWT no válido o ausente</response>
        /// <response code="404">Usuario no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ObtenerUsuarioPorId(int id)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_authServiceUrl);
                var client = new UserService.UserServiceClient(channel);

                var grpcRequest = new ObtenerUsuarioPorIdRequest { Id = id };
                var response = await client.ObtenerUsuarioPorIdAsync(grpcRequest);

                if (!response.Success)
                {
                    return NotFound(new { Message = response.Message });
                }

                return Ok(new
                {
                    Success = true,
                    Message = response.Message,
                    Data = new
                    {
                        Id = response.Usuario.Id,
                        Email = response.Usuario.Email,
                        NombreUsuario = response.Usuario.NombreUsuario,
                        RolId = response.Usuario.RolId,
                        RolNombre = response.Usuario.RolNombre,
                        Estado = response.Usuario.Estado,
                        FechaCreacion = response.Usuario.FechaCreacion,
                        FechaModificacion = response.Usuario.FechaModificacion,
                        UltimoAcceso = response.Usuario.UltimoAcceso
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por ID: {Id}", id);
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza los datos de un usuario existente
        /// </summary>
        /// <param name="id">ID único del usuario a actualizar</param>
        /// <param name="request">Datos actualizados del usuario</param>
        /// <returns>Resultado de la operación de actualización</returns>
        /// <response code="200">Usuario actualizado exitosamente</response>
        /// <response code="400">Datos inválidos, email o username ya registrados</response>
        /// <response code="401">Token JWT no válido o ausente</response>
        /// <response code="404">Usuario o rol no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ActualizarUsuario(int id, [FromBody] ActualizarUsuarioRequestDto request)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_authServiceUrl);
                var client = new UserService.UserServiceClient(channel);

                var grpcRequest = new ActualizarUsuarioRequest
                {
                    Id = id,
                    Email = request.Email,
                    NombreUsuario = request.NombreUsuario,
                    RolId = request.RolId
                };

                if (!string.IsNullOrWhiteSpace(request.Password))
                {
                    grpcRequest.Password = request.Password;
                }

                var response = await client.ActualizarUsuarioAsync(grpcRequest);

                return Ok(new
                {
                    Success = response.Success,
                    Message = response.Message,
                    Data = new { AffectedRows = response.AffectedRows }
                });
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error gRPC al actualizar usuario: {Id}", id);
                
                var errorResponse = ex.StatusCode switch
                {
                    Grpc.Core.StatusCode.InvalidArgument => BadRequest(new { Message = ex.Status.Detail }),
                    Grpc.Core.StatusCode.AlreadyExists => BadRequest(new { Message = ex.Status.Detail }),
                    Grpc.Core.StatusCode.NotFound => NotFound(new { Message = ex.Status.Detail }),
                    _ => base.StatusCode(500, new { Message = "Error interno del servidor" })
                };
                
                return errorResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario: {Id}", id);
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza únicamente el estado de un usuario (activo/inactivo)
        /// </summary>
        /// <param name="id">ID único del usuario</param>
        /// <param name="request">Nuevo estado del usuario</param>
        /// <returns>Resultado de la actualización del estado</returns>
        /// <response code="200">Estado actualizado exitosamente</response>
        /// <response code="400">Estado inválido</response>
        /// <response code="401">Token JWT no válido o ausente</response>
        /// <response code="404">Usuario no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPatch("{id}/estado")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ActualizarEstadoUsuario(int id, [FromBody] ActualizarEstadoUsuarioRequestDto request)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_authServiceUrl);
                var client = new UserService.UserServiceClient(channel);

                var grpcRequest = new ActualizarEstadoUsuarioRequest
                {
                    Id = id,
                    Estado = request.Estado
                };

                var response = await client.ActualizarEstadoUsuarioAsync(grpcRequest);

                return Ok(new
                {
                    Success = response.Success,
                    Message = response.Message,
                    Data = new { AffectedRows = response.AffectedRows }
                });
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error gRPC al actualizar estado del usuario: {Id}", id);
                
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
                _logger.LogError(ex, "Error al actualizar estado del usuario: {Id}", id);
                return base.StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Verifica si existe un usuario con el ID especificado
        /// </summary>
        /// <param name="id">ID del usuario a verificar</param>
        /// <returns>Indica si existe el usuario</returns>
        /// <response code="200">Consulta realizada exitosamente</response>
        /// <response code="401">Token JWT no válido o ausente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("{id}/existe")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ExisteUsuario(int id)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_authServiceUrl);
                var client = new UserService.UserServiceClient(channel);

                var grpcRequest = new ExisteUsuarioRequest { Id = id };
                var response = await client.ExisteUsuarioAsync(grpcRequest);

                return Ok(new
                {
                    Success = true,
                    Data = new { Existe = response.Existe }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de usuario: {Id}", id);
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Crea un nuevo usuario y lo asigna automáticamente a un chofer existente
        /// </summary>
        /// <param name="request">Datos del usuario a crear y el ID del chofer a asociar</param>
        /// <returns>ID del usuario creado y confirmación de asignación</returns>
        /// <response code="200">Usuario creado y asignado exitosamente al chofer</response>
        /// <response code="400">Datos inválidos, email/username duplicados, chofer no existe o ya tiene usuario</response>
        /// <response code="401">Token JWT no válido o ausente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("crear-y-asignar-chofer")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CrearUsuarioYAsignarChofer([FromBody] CrearUsuarioYAsignarChoferRequestDto request)
        {
            try
            {
                var choferesServiceUrl = Environment.GetEnvironmentVariable("MS_CHOFERES_GRPC_URL") ?? "https://localhost:5133";
                
                // 0️⃣ VERIFICAR QUE EL CHOFER NO TENGA USUARIO ASIGNADO
                try
                {
                    using var choferesChannelCheck = GrpcChannel.ForAddress(choferesServiceUrl);
                    var choferesClientCheck = new MS.Choferes.Protos.ChoferesService.ChoferesServiceClient(choferesChannelCheck);

                    // Obtener todos los choferes y buscar el específico
                    var choferes = new List<MS.Choferes.Protos.ChoferDto>();
                    using var call = choferesClientCheck.ListarChoferes(new Google.Protobuf.WellKnownTypes.Empty());
                    
                    await foreach (var chofer in call.ResponseStream.ReadAllAsync())
                    {
                        if (chofer.Id == request.ChoferId)
                        {
                            // Verificar si el chofer ya tiene un usuario asignado (UsuarioId != 0)
                            if (chofer.UsuarioId > 0)
                            {
                                _logger.LogWarning("Intento de asignar usuario al chofer {ChoferId} que ya tiene usuario {UsuarioId}", 
                                    request.ChoferId, chofer.UsuarioId);
                                
                                return BadRequest(new
                                {
                                    Success = false,
                                    Message = $"El chofer '{chofer.NombreCompleto}' ya tiene un usuario asignado (ID: {chofer.UsuarioId}). No se puede crear un nuevo usuario para este chofer.",
                                    Data = new
                                    {
                                        ChoferId = request.ChoferId,
                                        ChoferNombre = chofer.NombreCompleto,
                                        UsuarioIdExistente = chofer.UsuarioId
                                    }
                                });
                            }
                            break;
                        }
                    }
                }
                catch (RpcException ex)
                {
                    _logger.LogError(ex, "Error al verificar estado del chofer {ChoferId}", request.ChoferId);
                    return StatusCode(500, new { Message = "Error al verificar el estado del chofer" });
                }

                // 1️⃣ CREAR EL USUARIO (solo si el chofer no tiene usuario)
                using var channel = GrpcChannel.ForAddress(_authServiceUrl);
                var userClient = new UserService.UserServiceClient(channel);

                var crearUsuarioRequest = new CrearUsuarioRequest
                {
                    NombreUsuario = request.Username,
                    Email = request.Email,
                    Password = request.Password,
                    RolId = request.RolId
                };

                var usuarioResponse = await userClient.CrearUsuarioAsync(crearUsuarioRequest);
                var usuarioId = usuarioResponse.UsuarioId;

                // 2️⃣ ASIGNAR EL USUARIO AL CHOFER
                try
                {
                    using var choferesChannel = GrpcChannel.ForAddress(choferesServiceUrl);
                    var choferesClient = new MS.Choferes.Protos.ChoferesService.ChoferesServiceClient(choferesChannel);

                    var asignarRequest = new MS.Choferes.Protos.AsignarUsuarioRequest
                    {
                        ChoferId = request.ChoferId,
                        UsuarioId = usuarioId
                    };

                    var asignarResponse = await choferesClient.AsignarUsuarioAsync(asignarRequest);

                    return Ok(new
                    {
                        Success = true,
                        Message = "Usuario creado y asignado exitosamente al chofer",
                        Data = new
                        {
                            UsuarioId = usuarioId,
                            ChoferId = request.ChoferId,
                            UsuarioAsignado = asignarResponse.Affected > 0
                        }
                    });
                }
                catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.InvalidArgument)
                {
                    // Si falla la asignación, intentar eliminar el usuario creado
                    _logger.LogWarning("Error al asignar usuario {UsuarioId} al chofer {ChoferId}. Intentando rollback...", usuarioId, request.ChoferId);
                    
                    try
                    {
                        // Marcar el usuario como inactivo (rollback parcial)
                        var desactivarRequest = new ActualizarEstadoUsuarioRequest
                        {
                            Id = usuarioId,
                            Estado = 0
                        };
                        await userClient.ActualizarEstadoUsuarioAsync(desactivarRequest);
                    }
                    catch (Exception rollbackEx)
                    {
                        _logger.LogError(rollbackEx, "Error al hacer rollback del usuario {UsuarioId}", usuarioId);
                    }

                    return BadRequest(new
                    {
                        Success = false,
                        Message = $"Usuario creado pero no se pudo asignar al chofer: {ex.Status.Detail}",
                        Data = new
                        {
                            UsuarioId = usuarioId,
                            UsuarioDesactivado = true,
                            Error = ex.Status.Detail
                        }
                    });
                }
            }
            catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.InvalidArgument)
            {
                _logger.LogWarning("Error de validación al crear usuario y asignar chofer: {Detail}", ex.Status.Detail);
                return BadRequest(new { Message = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario y asignar chofer");
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }
    }

    // ========== DTOs ==========

    /// <summary>
    /// Datos requeridos para crear un nuevo usuario
    /// </summary>
    public class CrearUsuarioRequestDto
    {
        /// <summary>
        /// Email único del usuario
        /// </summary>
        /// <example>usuario@ejemplo.com</example>
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email no válido")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de usuario único
        /// </summary>
        /// <example>juanperez</example>
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [MinLength(3, ErrorMessage = "El nombre de usuario debe tener al menos 3 caracteres")]
        public string NombreUsuario { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña del usuario
        /// </summary>
        /// <example>Passw0rd!</example>
        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// ID del rol asignado al usuario
        /// </summary>
        /// <example>1</example>
        [Required(ErrorMessage = "El rol es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El rol debe ser mayor a 0")]
        public int RolId { get; set; }
    }

    /// <summary>
    /// Datos para actualizar un usuario existente
    /// </summary>
    public class ActualizarUsuarioRequestDto
    {
        /// <summary>
        /// Email único del usuario
        /// </summary>
        /// <example>usuario@ejemplo.com</example>
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email no válido")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de usuario único
        /// </summary>
        /// <example>juanperez</example>
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [MinLength(3, ErrorMessage = "El nombre de usuario debe tener al menos 3 caracteres")]
        public string NombreUsuario { get; set; } = string.Empty;

        /// <summary>
        /// Nueva contraseña (opcional - solo si se desea cambiar)
        /// </summary>
        /// <example>NuevoPassword123!</example>
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string? Password { get; set; }

        /// <summary>
        /// ID del rol asignado al usuario
        /// </summary>
        /// <example>1</example>
        [Required(ErrorMessage = "El rol es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El rol debe ser mayor a 0")]
        public int RolId { get; set; }
    }

    /// <summary>
    /// Datos para actualizar el estado de un usuario
    /// </summary>
    public class ActualizarEstadoUsuarioRequestDto
    {
        /// <summary>
        /// Nuevo estado del usuario (0 = inactivo, 1 = activo)
        /// </summary>
        /// <example>1</example>
        [Required(ErrorMessage = "El estado es requerido")]
        [Range(0, 1, ErrorMessage = "El estado debe ser 0 (inactivo) o 1 (activo)")]
        public int Estado { get; set; }
    }

    /// <summary>
    /// Datos para crear un usuario y asignarlo automáticamente a un chofer
    /// </summary>
    public class CrearUsuarioYAsignarChoferRequestDto
    {
        /// <summary>
        /// Nombre de usuario único
        /// </summary>
        /// <example>juan.perez</example>
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [MinLength(3, ErrorMessage = "El username debe tener al menos 3 caracteres")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Email único del usuario
        /// </summary>
        /// <example>juan.perez@example.com</example>
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña del usuario
        /// </summary>
        /// <example>Password123!</example>
        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// ID del rol asignado al usuario
        /// </summary>
        /// <example>2</example>
        [Required(ErrorMessage = "El rol es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El rol debe ser mayor a 0")]
        public int RolId { get; set; }

        /// <summary>
        /// ID del chofer al que se asignará el usuario
        /// </summary>
        /// <example>1</example>
        [Required(ErrorMessage = "El ID del chofer es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del chofer debe ser mayor a 0")]
        public int ChoferId { get; set; }
    }
}

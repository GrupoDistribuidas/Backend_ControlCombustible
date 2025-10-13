using Grpc.Core;
using MS.Autenticacion.Grpc;
using MS.Autenticacion.Domain.Entities;
using MS.Autenticacion.Domain.Interfaces;

namespace MS.Autenticacion.Application.Services
{
    public class GrpcUserService : UserService.UserServiceBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IRolRepository _rolRepository;
        private readonly ILogger<GrpcUserService> _logger;

        public GrpcUserService(
            IUserRepository userRepository,
            IRolRepository rolRepository,
            ILogger<GrpcUserService> logger)
        {
            _userRepository = userRepository;
            _rolRepository = rolRepository;
            _logger = logger;
        }

        public override async Task<CrearUsuarioResponse> CrearUsuario(CrearUsuarioRequest request, ServerCallContext context)
        {
            try
            {
                // Validar datos requeridos
                if (string.IsNullOrWhiteSpace(request.Email) || 
                    string.IsNullOrWhiteSpace(request.NombreUsuario) || 
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Email, nombre de usuario y contraseña son requeridos"));
                }

                // Validar email
                if (!IsValidEmail(request.Email))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Email no válido"));
                }

                // Verificar que el email no esté registrado
                if (await _userRepository.ExistsByEmailAsync(request.Email))
                {
                    throw new RpcException(new Status(StatusCode.AlreadyExists, "El email ya está registrado"));
                }

                // Verificar que el username no esté registrado
                if (await _userRepository.ExistsByUsernameAsync(request.NombreUsuario))
                {
                    throw new RpcException(new Status(StatusCode.AlreadyExists, "El nombre de usuario ya está registrado"));
                }

                // Verificar que el rol existe
                var rol = await _rolRepository.GetByIdAsync(request.RolId);
                if (rol == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Rol con ID {request.RolId} no encontrado"));
                }

                // Hashear la contraseña
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                // Crear usuario
                var user = new User
                {
                    Email = request.Email.Trim(),
                    NombreUsuario = request.NombreUsuario.Trim(),
                    PasswordHash = passwordHash,
                    RolId = request.RolId,
                    Estado = 1, // Activo por defecto
                    FechaCreacion = DateTime.UtcNow
                };

                var userId = await _userRepository.CreateAsync(user);

                _logger.LogInformation("Usuario creado exitosamente: {Username} con ID: {UserId}", request.NombreUsuario, userId);

                return new CrearUsuarioResponse
                {
                    Success = true,
                    Message = "Usuario creado exitosamente",
                    UsuarioId = userId
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al crear usuario"));
            }
        }

        public override async Task ListarUsuarios(ListarUsuariosRequest request, IServerStreamWriter<UsuarioDto> responseStream, ServerCallContext context)
        {
            try
            {
                var usuarios = await _userRepository.GetAllAsync();

                foreach (var usuario in usuarios)
                {
                    var rol = await _rolRepository.GetByIdAsync(usuario.RolId);
                    
                    var usuarioDto = new UsuarioDto
                    {
                        Id = usuario.Id,
                        Email = usuario.Email,
                        NombreUsuario = usuario.NombreUsuario,
                        RolId = usuario.RolId,
                        RolNombre = rol?.Nombre ?? "Sin rol",
                        Estado = usuario.Estado,
                        FechaCreacion = usuario.FechaCreacion.ToString("yyyy-MM-dd HH:mm:ss"),
                        FechaModificacion = usuario.FechaModificacion?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                        UltimoAcceso = usuario.UltimoAcceso?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""
                    };

                    await responseStream.WriteAsync(usuarioDto);
                }

                _logger.LogInformation("Usuarios listados exitosamente: {Count}", usuarios.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar usuarios");
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al listar usuarios"));
            }
        }

        public override async Task<ObtenerUsuarioPorIdResponse> ObtenerUsuarioPorId(ObtenerUsuarioPorIdRequest request, ServerCallContext context)
        {
            try
            {
                var usuario = await _userRepository.GetByIdAsync(request.Id);
                
                if (usuario == null)
                {
                    return new ObtenerUsuarioPorIdResponse
                    {
                        Success = false,
                        Message = "Usuario no encontrado"
                    };
                }

                var rol = await _rolRepository.GetByIdAsync(usuario.RolId);

                var usuarioDto = new UsuarioDto
                {
                    Id = usuario.Id,
                    Email = usuario.Email,
                    NombreUsuario = usuario.NombreUsuario,
                    RolId = usuario.RolId,
                    RolNombre = rol?.Nombre ?? "Sin rol",
                    Estado = usuario.Estado,
                    FechaCreacion = usuario.FechaCreacion.ToString("yyyy-MM-dd HH:mm:ss"),
                    FechaModificacion = usuario.FechaModificacion?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                    UltimoAcceso = usuario.UltimoAcceso?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""
                };

                return new ObtenerUsuarioPorIdResponse
                {
                    Success = true,
                    Message = "Usuario encontrado",
                    Usuario = usuarioDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por ID: {Id}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al obtener usuario"));
            }
        }

        public override async Task<ActualizarUsuarioResponse> ActualizarUsuario(ActualizarUsuarioRequest request, ServerCallContext context)
        {
            try
            {
                var usuario = await _userRepository.GetByIdAsync(request.Id);
                if (usuario == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "Usuario no encontrado"));
                }

                // Validar email
                if (!IsValidEmail(request.Email))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Email no válido"));
                }

                // Verificar que el nuevo email no esté registrado por otro usuario
                var existingUserByEmail = await _userRepository.GetByEmailAsync(request.Email);
                if (existingUserByEmail != null && existingUserByEmail.Id != request.Id)
                {
                    throw new RpcException(new Status(StatusCode.AlreadyExists, "El email ya está registrado por otro usuario"));
                }

                // Verificar que el nuevo username no esté registrado por otro usuario
                var existingUserByUsername = await _userRepository.GetByNombreUsuarioAsync(request.NombreUsuario);
                if (existingUserByUsername != null && existingUserByUsername.Id != request.Id)
                {
                    throw new RpcException(new Status(StatusCode.AlreadyExists, "El nombre de usuario ya está registrado por otro usuario"));
                }

                // Verificar que el rol existe
                var rol = await _rolRepository.GetByIdAsync(request.RolId);
                if (rol == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Rol con ID {request.RolId} no encontrado"));
                }

                usuario.Email = request.Email.Trim();
                usuario.NombreUsuario = request.NombreUsuario.Trim();
                usuario.RolId = request.RolId;

                // Si se proporciona una nueva contraseña, actualizarla
                if (!string.IsNullOrWhiteSpace(request.Password))
                {
                    usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                }

                var affectedRows = await _userRepository.UpdateAsync(usuario);

                _logger.LogInformation("Usuario actualizado exitosamente: ID {Id}", request.Id);

                return new ActualizarUsuarioResponse
                {
                    Success = affectedRows > 0,
                    Message = affectedRows > 0 ? "Usuario actualizado exitosamente" : "No se pudo actualizar el usuario",
                    AffectedRows = affectedRows
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario: {Id}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al actualizar usuario"));
            }
        }

        public override async Task<ActualizarEstadoUsuarioResponse> ActualizarEstadoUsuario(ActualizarEstadoUsuarioRequest request, ServerCallContext context)
        {
            try
            {
                if (!await _userRepository.ExistsAsync(request.Id))
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "Usuario no encontrado"));
                }

                if (request.Estado != 0 && request.Estado != 1)
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Estado debe ser 0 (inactivo) o 1 (activo)"));
                }

                var affectedRows = await _userRepository.UpdateEstadoAsync(request.Id, request.Estado);

                _logger.LogInformation("Estado de usuario actualizado: ID {Id}, Nuevo Estado: {Estado}", request.Id, request.Estado);

                return new ActualizarEstadoUsuarioResponse
                {
                    Success = affectedRows > 0,
                    Message = affectedRows > 0 ? "Estado actualizado exitosamente" : "No se pudo actualizar el estado",
                    AffectedRows = affectedRows
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado del usuario: {Id}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al actualizar estado"));
            }
        }

        public override async Task<ExisteUsuarioResponse> ExisteUsuario(ExisteUsuarioRequest request, ServerCallContext context)
        {
            try
            {
                var existe = await _userRepository.ExistsAsync(request.Id);
                
                return new ExisteUsuarioResponse
                {
                    Existe = existe
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia del usuario: {Id}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al verificar existencia del usuario"));
            }
        }

        public override async Task<ExisteUsernameResponse> ExisteUsername(ExisteUsernameRequest request, ServerCallContext context)
        {
            try
            {
                var existe = await _userRepository.ExistsByUsernameAsync(request.Username);
                
                return new ExisteUsernameResponse
                {
                    Existe = existe
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia del username: {Username}", request.Username);
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al verificar username"));
            }
        }

        public override async Task<ExisteEmailResponse> ExisteEmail(ExisteEmailRequest request, ServerCallContext context)
        {
            try
            {
                var existe = await _userRepository.ExistsByEmailAsync(request.Email);
                
                return new ExisteEmailResponse
                {
                    Existe = existe
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia del email: {Email}", request.Email);
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al verificar email"));
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}

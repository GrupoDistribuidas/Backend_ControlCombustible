using Grpc.Core;
using MS.Autenticacion.Grpc;
using MS.Autenticacion.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace MS.Autenticacion.Application.Services
{
    public class GrpcAuthService : MS.Autenticacion.Grpc.AuthService.AuthServiceBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<GrpcAuthService> _logger;

        public GrpcAuthService(IAuthService authService, ILogger<GrpcAuthService> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Intento de login para: {Username}", request.Username);

                var (success, token, message) = await _authService.LoginAsync(request.Username, request.Password);

                var response = new LoginResponse
                {
                    Success = success,
                    Message = message,
                    Token = token ?? string.Empty
                };

                if (success)
                {
                    // Calcular tiempo de expiración (1 hora desde ahora)
                    var expiresAt = DateTime.UtcNow.AddHours(1);
                    var expiresAtTimestamp = ((DateTimeOffset)expiresAt).ToUnixTimeSeconds();
                    response.ExpiresAt = expiresAtTimestamp;
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el login");
                return new LoginResponse
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Token = string.Empty
                };
            }
        }

        public override async Task<ValidateTokenResponse> ValidateToken(ValidateTokenRequest request, ServerCallContext context)
        {
            try
            {
                var (isValid, user, message, expiresAt) = await _authService.ValidateTokenAsync(request.Token);

                var response = new ValidateTokenResponse
                {
                    IsValid = isValid,
                    Message = message
                };

                if (isValid && user != null)
                {
                    response.User = new UserInfo
                    {
                        Id = user.Id,
                        Email = user.Email,
                        NombreUsuario = user.NombreUsuario,
                        RolId = user.RolId,
                        RolNombre = "", // Se llenará desde el servicio si está disponible
                        Estado = user.Estado,
                        FechaCreacion = user.FechaCreacion.ToString("yyyy-MM-dd HH:mm:ss"),
                        UltimoAcceso = user.UltimoAcceso?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty
                    };
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la validación del token");
                return new ValidateTokenResponse
                {
                    IsValid = false,
                    Message = "Error interno del servidor"
                };
            }
        }

        public override async Task<ForgotPasswordResponse> ForgotPassword(ForgotPasswordRequest request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Solicitud de recuperación de contraseña para: {UsernameOrEmail}", request.UsernameOrEmail);

                var (success, message) = await _authService.SendTemporaryPasswordAsync(request.UsernameOrEmail);

                var response = new ForgotPasswordResponse
                {
                    Success = success,
                    Message = message
                };

                if (success)
                {
                    _logger.LogInformation("Contraseña temporal enviada exitosamente para: {UsernameOrEmail}", request.UsernameOrEmail);
                }
                else
                {
                    _logger.LogWarning("Fallo en envío de contraseña temporal para: {UsernameOrEmail}", request.UsernameOrEmail);
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la recuperación de contraseña");
                return new ForgotPasswordResponse
                {
                    Success = false,
                    Message = "Error interno del servidor"
                };
            }
        }

        public override Task<GenerateTemporaryPasswordResponse> GenerateTemporaryPassword(GenerateTemporaryPasswordRequest request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Solicitud de generación de contraseña temporal");

                var temporaryPassword = _authService.GenerateTemporaryPassword();

                var response = new GenerateTemporaryPasswordResponse
                {
                    Success = true,
                    TemporaryPassword = temporaryPassword,
                    Message = "Contraseña temporal generada exitosamente"
                };

                _logger.LogInformation("Contraseña temporal generada exitosamente");

                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando contraseña temporal");
                return Task.FromResult(new GenerateTemporaryPasswordResponse
                {
                    Success = false,
                    TemporaryPassword = string.Empty,
                    Message = "Error interno del servidor"
                });
            }
        }
    }
}
using Grpc.Net.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MS.Autenticacion.Grpc;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ApiGateway.Controllers
{
    /// <summary>
    /// Controlador para operaciones de autenticación y gestión de usuarios
    /// </summary>
    [ApiController]
    [Route("auth")]
    [Produces("application/json")]
    public class AuthProxyController : ControllerBase
    {
        private readonly ILogger<AuthProxyController> _logger;
        private readonly MS.Autenticacion.Grpc.AuthService.AuthServiceClient _grpcClient;

        public AuthProxyController(ILogger<AuthProxyController> logger, MS.Autenticacion.Grpc.AuthService.AuthServiceClient grpcClient)
        {
            _logger = logger;
            _grpcClient = grpcClient;
        }

        /// <summary>
        /// Datos requeridos para iniciar sesión
        /// </summary>
        public class LoginRequestDto
        {
            /// <summary>
            /// Nombre de usuario
            /// </summary>
            /// <example>admin</example>
            [Required(ErrorMessage = "El nombre de usuario es requerido")]
            public string Username { get; set; } = string.Empty;

            /// <summary>
            /// Contraseña del usuario
            /// </summary>
            /// <example>password123</example>
            [Required(ErrorMessage = "La contraseña es requerida")]
            public string Password { get; set; } = string.Empty;
        }

        /// <summary>
        /// Respuesta del proceso de autenticación
        /// </summary>
        public class LoginResponseDto
        {
            /// <summary>
            /// Indica si el login fue exitoso
            /// </summary>
            public bool Success { get; set; }

            /// <summary>
            /// Mensaje descriptivo del resultado
            /// </summary>
            public string Message { get; set; } = string.Empty;

            /// <summary>
            /// Token JWT para autenticación
            /// </summary>
            public string Token { get; set; } = string.Empty;

            /// <summary>
            /// Timestamp de expiración del token
            /// </summary>
            public long ExpiresAt { get; set; }
        }

        /// <summary>
        /// Datos para recuperación de contraseña
        /// </summary>
        public class ForgotPasswordRequestDto
        {
            /// <summary>
            /// Nombre de usuario o email
            /// </summary>
            /// <example>admin@example.com</example>
            [Required(ErrorMessage = "El usuario o email es requerido")]
            public string UsernameOrEmail { get; set; } = string.Empty;
        }

        /// <summary>
        /// Respuesta del proceso de recuperación de contraseña
        /// </summary>
        public class ForgotPasswordResponseDto
        {
            /// <summary>
            /// Indica si el proceso fue exitoso
            /// </summary>
            public bool Success { get; set; }

            /// <summary>
            /// Mensaje descriptivo del resultado
            /// </summary>
            public string Message { get; set; } = string.Empty;
        }

        /// <summary>
        /// Autentica un usuario en el sistema
        /// </summary>
        /// <param name="dto">Credenciales de acceso (usuario y contraseña)</param>
        /// <returns>Token JWT para acceso a endpoints protegidos</returns>
        /// <response code="200">Login exitoso - devuelve token JWT</response>
        /// <response code="401">Credenciales inválidas</response>
        /// <response code="400">Datos de entrada inválidos</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponseDto), 200)]
        [ProducesResponseType(typeof(LoginResponseDto), 401)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            try
            {
                var grpcRequest = new MS.Autenticacion.Grpc.LoginRequest
                {
                    Username = dto.Username,
                    Password = dto.Password
                };

                var grpcResponse = await _grpcClient.LoginAsync(grpcRequest);

                var resp = new LoginResponseDto
                {
                    Success = grpcResponse.Success,
                    Message = grpcResponse.Message,
                    Token = grpcResponse.Token,
                    ExpiresAt = grpcResponse.ExpiresAt
                };

                if (grpcResponse.Success)
                    return Ok(resp);
                else
                    return Unauthorized(resp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error proxying login to MS.Autenticacion");
                return StatusCode(500, new { success = false, message = "Error interno al autenticar" });
            }
        }

        /// <summary>
        /// Solicita la recuperación de contraseña para un usuario
        /// </summary>
        /// <param name="dto">Usuario o email para recuperar la contraseña</param>
        /// <returns>Confirmación del envío de la nueva contraseña temporal por email</returns>
        /// <response code="200">Solicitud procesada exitosamente - contraseña enviada por email</response>
        /// <response code="400">Usuario o email no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ForgotPasswordResponseDto), 200)]
        [ProducesResponseType(typeof(ForgotPasswordResponseDto), 400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            try
            {
                var grpcRequest = new MS.Autenticacion.Grpc.ForgotPasswordRequest
                {
                    UsernameOrEmail = dto.UsernameOrEmail
                };

                var grpcResponse = await _grpcClient.ForgotPasswordAsync(grpcRequest);

                var resp = new ForgotPasswordResponseDto
                {
                    Success = grpcResponse.Success,
                    Message = grpcResponse.Message
                };

                if (grpcResponse.Success)
                    return Ok(resp);
                else
                    return BadRequest(resp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error proxying forgot-password to MS.Autenticacion");
                return StatusCode(500, new { success = false, message = "Error interno al solicitar recuperación de contraseña" });
            }
        }
    }
}

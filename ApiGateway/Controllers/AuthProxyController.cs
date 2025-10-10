using Grpc.Net.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MS.Autenticacion.Grpc;
using System.Linq;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthProxyController : ControllerBase
    {
        private readonly ILogger<AuthProxyController> _logger;
        private readonly MS.Autenticacion.Grpc.AuthService.AuthServiceClient _grpcClient;

        public AuthProxyController(ILogger<AuthProxyController> logger, MS.Autenticacion.Grpc.AuthService.AuthServiceClient grpcClient)
        {
            _logger = logger;
            _grpcClient = grpcClient;
        }

        public class LoginRequestDto
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class LoginResponseDto
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public string Token { get; set; } = string.Empty;
            public long ExpiresAt { get; set; }
        }

        public class ForgotPasswordRequestDto
        {
            public string UsernameOrEmail { get; set; } = string.Empty;
        }

        public class ForgotPasswordResponseDto
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
        }

        // 🟢 LOGIN: devuelve el token
        [HttpPost("login")]
        [AllowAnonymous]
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

        // 🟢 FORGOT PASSWORD: solicita que MS.Autenticacion envíe una contraseña temporal
        [HttpPost("forgot-password")]
        [AllowAnonymous]
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

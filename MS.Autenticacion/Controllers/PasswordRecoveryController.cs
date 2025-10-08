using Microsoft.AspNetCore.Mvc;
using MS.Autenticacion.Domain.Interfaces;

namespace MS.Autenticacion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PasswordRecoveryController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<PasswordRecoveryController> _logger;

        public PasswordRecoveryController(IAuthService authService, ILogger<PasswordRecoveryController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Solicita una contraseña temporal que será enviada por email
        /// </summary>
        /// <param name="request">Solicitud con el nombre de usuario o email</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.UsernameOrEmail))
                {
                    return BadRequest(new { Message = "Debe proporcionar un nombre de usuario o email" });
                }

                var result = await _authService.SendTemporaryPasswordAsync(request.UsernameOrEmail);

                if (result.Success)
                {
                    return Ok(new { Message = result.Message });
                }

                return BadRequest(new { Message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando solicitud de recuperación de contraseña");
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Genera una nueva contraseña temporal (endpoint de prueba)
        /// </summary>
        /// <returns>Nueva contraseña temporal</returns>
        [HttpGet("generate-temp-password")]
        public IActionResult GenerateTemporaryPassword()
        {
            try
            {
                // Este endpoint es solo para pruebas y desarrollo
                var tempPassword = _authService.GenerateTemporaryPassword();
                return Ok(new { TemporaryPassword = tempPassword });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando contraseña temporal");
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }
    }

    /// <summary>
    /// Modelo para la solicitud de recuperación de contraseña
    /// </summary>
    public class ForgotPasswordRequest
    {
        /// <summary>
        /// Nombre de usuario o email del usuario
        /// </summary>
        public string UsernameOrEmail { get; set; } = string.Empty;
    }
}
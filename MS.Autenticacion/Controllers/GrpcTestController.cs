using Microsoft.AspNetCore.Mvc;
using MS.Autenticacion.Application.Services;
using MS.Autenticacion.Domain.Interfaces;

namespace MS.Autenticacion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GrpcTestController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<GrpcTestController> _logger;

        public GrpcTestController(IAuthService authService, ILogger<GrpcTestController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("test-login")]
        public async Task<IActionResult> TestLogin([FromBody] TestLoginRequest request)
        {
            try
            {
                _logger.LogInformation("Testing login for: {Username}", request.Username);

                var (success, token, message) = await _authService.LoginAsync(request.Username, request.Password);
                
                return Ok(new
                {
                    success = success,
                    message = message,
                    token = token,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing login");
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message,
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet("grpc-status")]
        public IActionResult GetGrpcStatus()
        {
            return Ok(new
            {
                message = "gRPC service is configured",
                endpoint = "http://localhost:5235",
                service = "MS.Autenticacion.Grpc.AuthService",
                methods = new[] { "Login", "ValidateToken" },
                timestamp = DateTime.Now
            });
        }
    }

    public class TestLoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
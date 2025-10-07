using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using MS.Autenticacion.Domain.Entities;
using MS.Autenticacion.Domain.Interfaces;

namespace MS.Autenticacion.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRolRepository _rolRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserRepository userRepository, IRolRepository rolRepository, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _rolRepository = rolRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<(bool Success, string Token, string Message)> LoginAsync(string username, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                    return (false, string.Empty, "Credenciales inválidas");

                // Buscar solo por nombre de usuario
                var user = await _userRepository.GetByNombreUsuarioAsync(username);

                if (user == null || user.Estado != 1)
                {
                    _logger.LogWarning("Intento de login fallido para {Username}", username);
                    return (false, string.Empty, "Credenciales inválidas");
                }

                // Verificar contraseña
                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    _logger.LogWarning("Contraseña incorrecta para {Username}", username);
                    return (false, string.Empty, "Credenciales inválidas");
                }

                // Obtener información del rol
                var rol = await _rolRepository.GetByIdAsync(user.RolId);

                // Actualizar último acceso
                await _userRepository.UpdateUltimoAccesoAsync(user.Id, DateTime.UtcNow);

                // Generar JWT con información del rol
                var token = GenerateJwtToken(user, rol);

                _logger.LogInformation("Login exitoso para {Username}", username);
                return (true, token, "Login exitoso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el login");
                return (false, string.Empty, "Error interno del servidor");
            }
        }

        public Task<(bool IsValid, User? User, string Message, DateTime? ExpiresAt)> ValidateTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return Task.FromResult<(bool IsValid, User? User, string Message, DateTime? ExpiresAt)>((false, null, "Token no proporcionado", null));

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new InvalidOperationException("JWT_SECRET no configurado"));

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "MS.Autenticacion",
                    ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "MS.Autenticacion",
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                
                // Extraer información del usuario del token usando los nuevos claims
                var userIdClaim = principal.FindFirst("id")?.Value; // Cambiar de ClaimTypes.NameIdentifier a "id"
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Task.FromResult<(bool IsValid, User? User, string Message, DateTime? ExpiresAt)>((false, null, "Token inválido", null));

                // Crear objeto User desde los claims usando los nuevos nombres
                var user = new User
                {
                    Id = userId,
                    NombreUsuario = principal.FindFirst("username")?.Value ?? string.Empty, // Cambiar de ClaimTypes.Name a "username"
                    Email = principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty,
                    RolId = int.TryParse(principal.FindFirst("RolId")?.Value, out int rolId) ? rolId : 0,
                    Estado = int.TryParse(principal.FindFirst("Estado")?.Value, out int estado) ? estado : 0
                };

                // Agregar los nuevos campos de fecha si están disponibles
                if (DateTime.TryParse(principal.FindFirst("fecha_creacion")?.Value, out DateTime fechaCreacion))
                    user.FechaCreacion = fechaCreacion;
                
                if (DateTime.TryParse(principal.FindFirst("ultimo_acceso")?.Value, out DateTime ultimoAcceso))
                    user.UltimoAcceso = ultimoAcceso;

                // Extraer fecha de expiración
                var expClaim = principal.FindFirst("exp")?.Value;
                DateTime? expiresAt = null;
                if (long.TryParse(expClaim, out long exp))
                {
                    expiresAt = DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;
                }

                return Task.FromResult<(bool IsValid, User? User, string Message, DateTime? ExpiresAt)>((true, user, "Token válido", expiresAt));
            }
            catch (SecurityTokenExpiredException)
            {
                return Task.FromResult<(bool IsValid, User? User, string Message, DateTime? ExpiresAt)>((false, null, "Token expirado", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando token");
                return Task.FromResult<(bool IsValid, User? User, string Message, DateTime? ExpiresAt)>((false, null, "Token inválido", null));
            }
        }

        public string GenerateJwtToken(User user, Rol? rol = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new InvalidOperationException("JWT_SECRET no configurado"));
            
            var claims = new List<Claim>
            {
                new Claim("id", user.Id.ToString()), // Cambiar de NameIdentifier a "id"
                new Claim("username", user.NombreUsuario), // Cambiar de Name a "username"
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("RolId", user.RolId.ToString()),
                new Claim("Estado", user.Estado.ToString()),
                new Claim("fecha_creacion", user.FechaCreacion.ToString("yyyy-MM-dd HH:mm:ss")),
                new Claim("ultimo_acceso", user.UltimoAcceso?.ToString("yyyy-MM-dd HH:mm:ss") ?? "")
            };

            // Agregar información del rol si está disponible
            if (rol != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, rol.Nombre));
                claims.Add(new Claim("RolNombre", rol.Nombre));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1), // 1 hora de expiración
                Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "MS.Autenticacion",
                Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "MS.Autenticacion",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
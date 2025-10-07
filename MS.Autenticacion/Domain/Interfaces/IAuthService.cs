using MS.Autenticacion.Domain.Entities;

namespace MS.Autenticacion.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string Token, string Message)> LoginAsync(string username, string password);
        Task<(bool IsValid, User? User, string Message, DateTime? ExpiresAt)> ValidateTokenAsync(string token);
        string GenerateJwtToken(User user, Rol? rol = null);
    }
}
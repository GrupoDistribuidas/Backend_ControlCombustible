using MS.Autenticacion.Domain.Entities;

namespace MS.Autenticacion.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByNombreUsuarioAsync(string nombreUsuario);
        Task<User?> GetByEmailAsync(string email);
        Task UpdateUltimoAccesoAsync(int userId, DateTime ultimoAcceso);
        Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash);
    }
}
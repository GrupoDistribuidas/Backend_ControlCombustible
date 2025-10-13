using MS.Autenticacion.Domain.Entities;

namespace MS.Autenticacion.Domain.Interfaces
{
    public interface IUserRepository
    {
        // Métodos existentes
        Task<User?> GetByNombreUsuarioAsync(string nombreUsuario);
        Task<User?> GetByEmailAsync(string email);
        Task UpdateUltimoAccesoAsync(int userId, DateTime ultimoAcceso);
        Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash);
        
        // Nuevos métodos CRUD
        Task<int> CreateAsync(User user);
        Task<User?> GetByIdAsync(int id);
        Task<List<User>> GetAllAsync();
        Task<int> UpdateAsync(User user);
        Task<int> UpdateEstadoAsync(int userId, int estado);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByUsernameAsync(string username);
        Task<bool> ExistsByEmailAsync(string email);
    }
}
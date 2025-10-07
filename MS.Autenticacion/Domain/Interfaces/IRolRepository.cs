using MS.Autenticacion.Domain.Entities;

namespace MS.Autenticacion.Domain.Interfaces
{
    public interface IRolRepository
    {
        Task<Rol?> GetByIdAsync(int id);
        Task<List<Rol>> GetAllAsync();
    }
}
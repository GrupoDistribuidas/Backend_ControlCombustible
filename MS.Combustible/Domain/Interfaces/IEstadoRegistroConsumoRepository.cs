using MS.Combustible.Domain.Entities;

namespace MS.Combustible.Domain.Interfaces
{
    public interface IEstadoRegistroConsumoRepository
    {
        Task<EstadoRegistroConsumo?> GetByIdAsync(int id);
        Task<IEnumerable<EstadoRegistroConsumo>> GetAllAsync();
        Task<EstadoRegistroConsumo?> GetByNombreAsync(string nombre);
    }
}
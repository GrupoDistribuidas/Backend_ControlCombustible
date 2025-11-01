using MS.Combustible.Domain.Entities;

namespace MS.Combustible.Domain.Interfaces
{
    public interface IRegistroConsumoRepository
    {
        Task<int> CreateAsync(RegistroConsumo registroConsumo);
        Task<int> UpdateAsync(RegistroConsumo registroConsumo);
        Task<int> UpdateEstadoAsync(int id, int estadoId);
        Task<RegistroConsumo?> GetByIdAsync(int id);
        Task<IEnumerable<RegistroConsumo>> GetAllAsync();
        Task<IEnumerable<RegistroConsumo>> GetByAsignacionRutaIdAsync(int asignacionRutaId);
        Task<IEnumerable<RegistroConsumo>> GetByEstadoIdAsync(int estadoId);
        Task<IEnumerable<RegistroConsumo>> GetByFechaRangoAsync(DateTime fechaInicio, DateTime fechaFin);
        Task<RegistroConsumo?> GetByAsignacionRutaIdAndEstadoAsync(int asignacionRutaId, int estadoId);
        Task<bool> ExistsByAsignacionRutaIdAsync(int asignacionRutaId);
    }
}
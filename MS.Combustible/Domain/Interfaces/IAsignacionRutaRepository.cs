using MS.Combustible.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MS.Combustible.Domain.Interfaces
{
    public interface IAsignacionRutaRepository
    {
        Task<int> CreateAsync(AsignacionRuta asignacion);
        Task<AsignacionRuta?> GetByIdAsync(int id);
        Task<IEnumerable<AsignacionRuta>> GetAllAsync();
        Task<IEnumerable<AsignacionRuta>> GetByChoferIdAsync(int choferId);
        Task<IEnumerable<AsignacionRuta>> GetByVehiculoIdAsync(int vehiculoId);
        Task<IEnumerable<AsignacionRuta>> GetByRutaIdAsync(int rutaId);
        Task<IEnumerable<AsignacionRuta>> GetByEstadoIdAsync(int estadoId);
        Task<int> UpdateAsync(AsignacionRuta asignacion);
        Task<int> UpdateEstadoAsync(int id, int estadoId);
    }
}

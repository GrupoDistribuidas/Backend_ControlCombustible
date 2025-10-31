using MS.Combustible.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MS.Combustible.Domain.Interfaces
{
    public interface IEstadoAsignacionRepository
    {
        Task<EstadoAsignacion?> GetByIdAsync(int id);
        Task<IEnumerable<EstadoAsignacion>> GetAllAsync();
        Task<EstadoAsignacion?> GetByNombreAsync(string nombre);
    }
}

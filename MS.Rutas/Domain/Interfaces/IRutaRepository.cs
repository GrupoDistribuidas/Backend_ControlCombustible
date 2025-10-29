using MS.Rutas.Domain.Entities;
using System.Collections.Generic;

namespace MS.Rutas.Domain.Interfaces
{
    public interface IRutaRepository
    {
        Task<int> CreateAsync(Ruta ruta);
        Task<Ruta?> GetByIdAsync(int id);
        Task<IEnumerable<Ruta>> GetAllAsync();
        Task<IEnumerable<Ruta>> GetAllIncludingInactiveAsync();
        Task<Ruta?> GetByNombreAsync(string nombre);
        Task<int> UpdateAsync(Ruta ruta, bool? estado = null);
        Task<int> UpdateEstadoAsync(int id, bool estado);
        Task<IEnumerable<Ruta>> SearchByTermAsync(string term);
    }
}

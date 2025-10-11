using MS.Vehiculos.Domain.Entities;
using System.Collections.Generic;

namespace MS.Vehiculos.Domain.Interfaces
{
    public interface ITipoMaquinariaRepository
    {
        Task<IEnumerable<TipoMaquinaria>> GetAllAsync();
        Task<TipoMaquinaria?> GetByIdAsync(int id);
        Task<int> CreateAsync(TipoMaquinaria tipo);
    }
}

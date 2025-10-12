using MS.Choferes.Domain.Entities;
using System.Collections.Generic;

namespace MS.Choferes.Domain.Interfaces
{
    public interface ITipoMaquinariaRepository
    {
        Task<IEnumerable<TipoMaquinaria>> GetAllAsync();
        Task<TipoMaquinaria?> GetByIdAsync(int id);
        Task<int> CreateAsync(TipoMaquinaria tipo);
    }
}

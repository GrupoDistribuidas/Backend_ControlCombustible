using MS.Choferes.Domain.Entities;
using System.Collections.Generic;

namespace MS.Choferes.Domain.Interfaces
{
    public interface IChoferRepository
    {
        Task<int> CreateAsync(Chofer chofer);
        Task<Chofer?> GetByIdAsync(int id);
        Task<IEnumerable<Chofer>> GetAllAsync();
        Task<Chofer?> GetByIdentificacionAsync(string identificacion);
        Task<Chofer?> GetByUsuarioIdAsync(int usuarioId);
        Task<int> UpdateAsync(Chofer chofer, bool? estado = null);
        Task<int> UpdateEstadoAsync(int id, bool estado);
        Task<int> UpdateDisponibilidadAsync(int id, bool disponible);
        Task<int> UpdateUsuarioAsync(int choferId, int usuarioId);
        Task<IEnumerable<Chofer>> SearchAsync(MS.Choferes.Application.DTOs.ChoferFilterDto filter);
        Task<IEnumerable<Chofer>> SearchByTermAsync(string term);
    }
}

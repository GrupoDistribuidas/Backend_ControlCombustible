using MS.Vehiculos.Domain.Entities;
using System.Collections.Generic;

namespace MS.Vehiculos.Domain.Interfaces
{
    public interface IVehiculoRepository
    {
        Task<int> CreateAsync(Vehiculo vehiculo);
        Task<Vehiculo?> GetByIdAsync(int id);
        Task<IEnumerable<Vehiculo>> GetAllAsync();
        Task<Vehiculo?> GetByPlacaAsync(string placa);
    }
}

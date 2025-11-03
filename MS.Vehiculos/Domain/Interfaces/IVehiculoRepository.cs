using MS.Vehiculos.Domain.Entities;
using System.Collections.Generic;

namespace MS.Vehiculos.Domain.Interfaces
{
    public interface IVehiculoRepository
    {
        Task<int> CreateAsync(Vehiculo vehiculo);
        Task<Vehiculo?> GetByIdAsync(int id);
        Task<IEnumerable<Vehiculo>> GetAllAsync();
        Task<IEnumerable<Vehiculo>> GetAllWithoutStateFilterAsync();
        Task<Vehiculo?> GetByPlacaAsync(string placa);
        Task<int> UpdateAsync(Vehiculo vehiculo, bool? estado = null);
        Task<IEnumerable<Vehiculo>> SearchAsync(MS.Vehiculos.Application.DTOs.VehiculoFilterDto filter);
        Task<IEnumerable<Vehiculo>> SearchByTermAsync(string term);

        // Métodos para reportes
        Task<(int TotalActivos, int TotalInactivos, int TotalGeneral)> GetTotalVehiculosActivosAsync();
        Task<List<(int TipoId, string TipoNombre, int Cantidad)>> GetVehiculosPorTipoAsync();
        Task<List<(string Disponibilidad, int Cantidad)>> GetVehiculosPorEstadoAsync();
    }
}

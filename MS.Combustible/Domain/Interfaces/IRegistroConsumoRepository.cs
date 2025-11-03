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

        // Métodos para reportes
        Task<double> GetConsumoPromedioAsync();
        Task<int> GetTotalRegistrosAsync();
        Task<List<(int RutaId, double ConsumoPromedio, int TotalViajes, double TotalEstimado, double TotalReal)>> GetConsumoPorRutaAsync();
        
        // Métodos para reportes avanzados
        Task<List<(int TipoVehiculoId, string TipoVehiculoNombre, double ConsumoPromedioReal, double ConsumoPromedioEstimado, double DesviacionPromedio, int TotalViajes, double CombustibleTotalReal)>> GetConsumoPorTipoVehiculoAsync();
        Task<List<(int RegistroId, int AsignacionId, int RutaId, string RutaNombre, int VehiculoId, string VehiculoNombre, double CombustibleEstimado, double CombustibleReal, double Desviacion, double PorcentajeDesviacion, string Motivo)>> GetDesviacionesCombustibleAsync(int limite = 50);
        Task<List<(int VehiculoId, string VehiculoNombre, string VehiculoPlaca, string TipoVehiculo, double RatioEficiencia, int TotalViajes, double ConsumoPromedioReal, double AhorroCombustible)>> GetVehiculosMasEficientesAsync(int limite = 10);
        Task<List<(int VehiculoId, string VehiculoNombre, string VehiculoPlaca, string TipoVehiculo, double RatioEficiencia, int TotalViajes, double ConsumoPromedioReal, double AhorroCombustible)>> GetVehiculosMenosEficientesAsync(int limite = 10);
    }
}
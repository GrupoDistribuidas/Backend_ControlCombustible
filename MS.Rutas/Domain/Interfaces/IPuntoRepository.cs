using MS.Rutas.Domain.Entities;
using System.Collections.Generic;

namespace MS.Rutas.Domain.Interfaces
{
    public interface IPuntoRepository
    {
        Task<int> CreateAsync(Punto punto);
        Task<Punto?> GetByIdAsync(int id);
        Task<IEnumerable<Punto>> GetAllAsync();
        Task<Punto?> GetByNombreAsync(string nombre);
        Task<int> UpdateAsync(Punto punto);
        Task<IEnumerable<Punto>> SearchByTermAsync(string term);
        
        // Métodos para reportes avanzados
        Task<List<(string TipoPunto, int Cantidad, string ProvinciaPrincipal)>> GetPuntosPorTipoAsync();
        Task<List<(int PuntoId, string NombrePunto, string Provincia, string TipoPunto, int VecesComoInicio, int VecesComoFin, int TotalUsos)>> GetPuntosMasUtilizadosAsync();
    }
}

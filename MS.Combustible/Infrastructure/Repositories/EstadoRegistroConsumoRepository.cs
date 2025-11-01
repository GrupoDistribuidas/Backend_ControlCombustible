using MS.Combustible.Domain.Entities;
using MS.Combustible.Domain.Interfaces;
using MS.Combustible.Services;
using System.Data;

namespace MS.Combustible.Infrastructure.Repositories
{
    public class EstadoRegistroConsumoRepository : IEstadoRegistroConsumoRepository
    {
        private readonly IDatabaseService _db;

        public EstadoRegistroConsumoRepository(IDatabaseService db)
        {
            _db = db;
        }

        public async Task<EstadoRegistroConsumo?> GetByIdAsync(int id)
        {
            var query = "SELECT * FROM estadosregistroconsumo WHERE Id = @Id";
            var parameters = new Dictionary<string, object> { { "@Id", id } };

            var result = await _db.ExecuteQueryAsync(query, parameters);
            if (result.Rows.Count == 0) return null;

            return MapToEntity(result.Rows[0]);
        }

        public async Task<IEnumerable<EstadoRegistroConsumo>> GetAllAsync()
        {
            var query = "SELECT * FROM estadosregistroconsumo ORDER BY Id";
            var result = await _db.ExecuteQueryAsync(query);
            return result.AsEnumerable().Select<DataRow, EstadoRegistroConsumo>(MapToEntity);
        }

        public async Task<EstadoRegistroConsumo?> GetByNombreAsync(string nombre)
        {
            var query = "SELECT * FROM estadosregistroconsumo WHERE Nombre = @Nombre";
            var parameters = new Dictionary<string, object> { { "@Nombre", nombre } };

            var result = await _db.ExecuteQueryAsync(query, parameters);
            if (result.Rows.Count == 0) return null;

            return MapToEntity(result.Rows[0]);
        }

        private EstadoRegistroConsumo MapToEntity(DataRow row)
        {
            return new EstadoRegistroConsumo
            {
                Id = Convert.ToInt32(row["Id"]),
                Nombre = row["Nombre"]?.ToString() ?? string.Empty,
                Descripcion = row["Descripcion"]?.ToString() ?? string.Empty
            };
        }
    }
}
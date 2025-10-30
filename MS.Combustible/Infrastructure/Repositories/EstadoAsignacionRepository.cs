using MS.Combustible.Domain.Entities;
using MS.Combustible.Domain.Interfaces;
using MS.Combustible.Services;
using System.Collections.Generic;
using System.Data;

namespace MS.Combustible.Infrastructure.Repositories
{
    public class EstadoAsignacionRepository : IEstadoAsignacionRepository
    {
        private readonly IDatabaseService _db;

        public EstadoAsignacionRepository(IDatabaseService db)
        {
            _db = db;
        }

        public async Task<EstadoAsignacion?> GetByIdAsync(int id)
        {
            var query = "SELECT Id, Nombre, Descripcion FROM estadosasignacion WHERE Id = @Id LIMIT 1;";
            var parameters = new Dictionary<string, object> { { "@Id", id } };
            
            var dt = await _db.ExecuteQueryAsync(query, parameters);
            
            if (dt.Rows.Count == 0) return null;
            
            var row = dt.Rows[0];
            return new EstadoAsignacion
            {
                Id = Convert.ToInt32(row["Id"]),
                Nombre = row["Nombre"].ToString() ?? string.Empty,
                Descripcion = row["Descripcion"].ToString() ?? string.Empty
            };
        }

        public async Task<IEnumerable<EstadoAsignacion>> GetAllAsync()
        {
            var dt = await _db.ExecuteQueryAsync("SELECT Id, Nombre, Descripcion FROM estadosasignacion;");
            var list = new List<EstadoAsignacion>();
            
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new EstadoAsignacion
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Nombre = row["Nombre"].ToString() ?? string.Empty,
                    Descripcion = row["Descripcion"].ToString() ?? string.Empty
                });
            }
            return list;
        }

        public async Task<EstadoAsignacion?> GetByNombreAsync(string nombre)
        {
            var query = "SELECT Id, Nombre, Descripcion FROM estadosasignacion WHERE Nombre = @Nombre LIMIT 1;";
            var parameters = new Dictionary<string, object> { { "@Nombre", nombre } };
            
            var dt = await _db.ExecuteQueryAsync(query, parameters);
            
            if (dt.Rows.Count == 0) return null;
            
            var row = dt.Rows[0];
            return new EstadoAsignacion
            {
                Id = Convert.ToInt32(row["Id"]),
                Nombre = row["Nombre"].ToString() ?? string.Empty,
                Descripcion = row["Descripcion"].ToString() ?? string.Empty
            };
        }
    }
}

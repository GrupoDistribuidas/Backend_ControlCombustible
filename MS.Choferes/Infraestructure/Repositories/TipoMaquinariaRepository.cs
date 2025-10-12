using MS.Choferes.Domain.Entities;
using MS.Choferes.Domain.Interfaces;
using MS.Choferes.Services;
using System.Collections.Generic;
using System.Data;

namespace MS.Choferes.Infraestructure.Repositories
{
    public class TipoMaquinariaRepository : ITipoMaquinariaRepository
    {
        private readonly IDatabaseService _db;

        public TipoMaquinariaRepository(IDatabaseService db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(TipoMaquinaria tipo)
        {
            var query = @"INSERT INTO TipoMaquinaria (Nombre, Descripcion, Estado) VALUES (@Nombre, @Descripcion, @Estado); SELECT LAST_INSERT_ID();";
            var parameters = new Dictionary<string, object>
            {
                { "@Nombre", tipo.Nombre },
                { "@Descripcion", tipo.Descripcion ?? (object)DBNull.Value },
                { "@Estado", tipo.Estado }
            };

            var result = await _db.ExecuteScalarAsync(query, parameters);
            return Convert.ToInt32(result);
        }

        public async Task<IEnumerable<TipoMaquinaria>> GetAllAsync()
        {
            var dt = await _db.ExecuteQueryAsync("SELECT Id, Nombre, Descripcion, Estado, FechaCreacion, FechaModificacion FROM TipoMaquinaria WHERE Estado = 1;");
            var list = new List<TipoMaquinaria>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new TipoMaquinaria
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Nombre = row["Nombre"].ToString() ?? string.Empty,
                    Descripcion = row["Descripcion"].ToString(),
                    Estado = Convert.ToBoolean(row["Estado"]),
                    FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                    FechaModificacion = Convert.ToDateTime(row["FechaModificacion"])
                });
            }
            return list;
        }

        public async Task<TipoMaquinaria?> GetByIdAsync(int id)
        {
            var dt = await _db.ExecuteQueryAsync($"SELECT Id, Nombre, Descripcion, Estado, FechaCreacion, FechaModificacion FROM TipoMaquinaria WHERE Id = {id} LIMIT 1;");
            if (dt.Rows.Count == 0) return null;
            var row = dt.Rows[0];
            return new TipoMaquinaria
            {
                Id = Convert.ToInt32(row["Id"]),
                Nombre = row["Nombre"].ToString() ?? string.Empty,
                Descripcion = row["Descripcion"].ToString(),
                Estado = Convert.ToBoolean(row["Estado"]),
                FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                FechaModificacion = Convert.ToDateTime(row["FechaModificacion"])
            };
        }
    }
}

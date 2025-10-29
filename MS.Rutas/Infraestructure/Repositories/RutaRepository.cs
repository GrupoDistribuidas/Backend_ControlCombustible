using MS.Rutas.Domain.Entities;
using MS.Rutas.Domain.Interfaces;
using MS.Rutas.Services;
using System.Collections.Generic;
using System.Data;

namespace MS.Rutas.Infraestructure.Repositories
{
    public class RutaRepository : IRutaRepository
    {
        private readonly IDatabaseService _db;

        public RutaRepository(IDatabaseService db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(Ruta ruta)
        {
            var query = @"INSERT INTO Rutas (Nombre, PuntoInicioId, PuntoFinId, Distancia, Estado)
VALUES (@Nombre, @PuntoInicioId, @PuntoFinId, @Distancia, @Estado);
SELECT LAST_INSERT_ID();";

            var parameters = new Dictionary<string, object>
            {
                { "@Nombre", ruta.Nombre },
                { "@PuntoInicioId", ruta.PuntoInicioId },
                { "@PuntoFinId", ruta.PuntoFinId },
                { "@Distancia", ruta.Distancia },
                { "@Estado", ruta.Estado }
            };

            var result = await _db.ExecuteScalarAsync(query, parameters);
            return Convert.ToInt32(result);
        }

        public async Task<IEnumerable<Ruta>> GetAllAsync()
        {
            var dt = await _db.ExecuteQueryAsync("SELECT Id, Nombre, PuntoInicioId, PuntoFinId, Distancia, Estado, FechaCreacion, FechaModificacion FROM Rutas WHERE Estado = 1;");
            var list = new List<Ruta>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Ruta
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Nombre = row["Nombre"].ToString() ?? string.Empty,
                    PuntoInicioId = Convert.ToInt32(row["PuntoInicioId"]),
                    PuntoFinId = Convert.ToInt32(row["PuntoFinId"]),
                    Distancia = Convert.ToDouble(row["Distancia"]),
                    Estado = Convert.ToBoolean(row["Estado"]),
                    FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                    FechaModificacion = Convert.ToDateTime(row["FechaModificacion"])
                });
            }
            return list;
        }

        public async Task<IEnumerable<Ruta>> GetAllIncludingInactiveAsync()
        {
            var dt = await _db.ExecuteQueryAsync("SELECT Id, Nombre, PuntoInicioId, PuntoFinId, Distancia, Estado, FechaCreacion, FechaModificacion FROM Rutas;");
            var list = new List<Ruta>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Ruta
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Nombre = row["Nombre"].ToString() ?? string.Empty,
                    PuntoInicioId = Convert.ToInt32(row["PuntoInicioId"]),
                    PuntoFinId = Convert.ToInt32(row["PuntoFinId"]),
                    Distancia = Convert.ToDouble(row["Distancia"]),
                    Estado = Convert.ToBoolean(row["Estado"]),
                    FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                    FechaModificacion = Convert.ToDateTime(row["FechaModificacion"])
                });
            }
            return list;
        }

        public async Task<Ruta?> GetByIdAsync(int id)
        {
            var dt = await _db.ExecuteQueryAsync($"SELECT Id, Nombre, PuntoInicioId, PuntoFinId, Distancia, Estado, FechaCreacion, FechaModificacion FROM Rutas WHERE Id = {id} LIMIT 1;");
            if (dt.Rows.Count == 0) return null;
            var row = dt.Rows[0];
            return new Ruta
            {
                Id = Convert.ToInt32(row["Id"]),
                Nombre = row["Nombre"].ToString() ?? string.Empty,
                PuntoInicioId = Convert.ToInt32(row["PuntoInicioId"]),
                PuntoFinId = Convert.ToInt32(row["PuntoFinId"]),
                Distancia = Convert.ToDouble(row["Distancia"]),
                Estado = Convert.ToBoolean(row["Estado"]),
                FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                FechaModificacion = Convert.ToDateTime(row["FechaModificacion"])
            };
        }

        public async Task<Ruta?> GetByNombreAsync(string nombre)
        {
            var dt = await _db.ExecuteQueryAsync($"SELECT Id, Nombre, PuntoInicioId, PuntoFinId, Distancia, Estado, FechaCreacion, FechaModificacion FROM Rutas WHERE Nombre = '{MySql.Data.MySqlClient.MySqlHelper.EscapeString(nombre)}' LIMIT 1;");
            if (dt.Rows.Count == 0) return null;
            var row = dt.Rows[0];
            return new Ruta
            {
                Id = Convert.ToInt32(row["Id"]),
                Nombre = row["Nombre"].ToString() ?? string.Empty,
                PuntoInicioId = Convert.ToInt32(row["PuntoInicioId"]),
                PuntoFinId = Convert.ToInt32(row["PuntoFinId"]),
                Distancia = Convert.ToDouble(row["Distancia"]),
                Estado = Convert.ToBoolean(row["Estado"]),
                FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                FechaModificacion = Convert.ToDateTime(row["FechaModificacion"])
            };
        }

        public async Task<int> UpdateAsync(Ruta ruta, bool? estado = null)
        {
            var setParts = new List<string>
            {
                "Nombre = @Nombre",
                "PuntoInicioId = @PuntoInicioId",
                "PuntoFinId = @PuntoFinId",
                "Distancia = @Distancia"
            };

            if (estado.HasValue)
                setParts.Add("Estado = @Estado");

            var setClause = string.Join(", ", setParts) + ", FechaModificacion = CURRENT_TIMESTAMP";
            var query = $"UPDATE Rutas SET {setClause} WHERE Id = @Id;";

            var parameters = new Dictionary<string, object>
            {
                { "@Nombre", ruta.Nombre },
                { "@PuntoInicioId", ruta.PuntoInicioId },
                { "@PuntoFinId", ruta.PuntoFinId },
                { "@Distancia", ruta.Distancia },
                { "@Id", ruta.Id }
            };

            if (estado.HasValue)
                parameters.Add("@Estado", estado.Value);

            return await _db.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<int> UpdateEstadoAsync(int id, bool estado)
        {
            var query = "UPDATE Rutas SET Estado = @Estado, FechaModificacion = CURRENT_TIMESTAMP WHERE Id = @Id;";
            var parameters = new Dictionary<string, object>
            {
                { "@Estado", estado },
                { "@Id", id }
            };
            return await _db.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<IEnumerable<Ruta>> SearchByTermAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return new List<Ruta>();

            var safe = MySql.Data.MySqlClient.MySqlHelper.EscapeString(term);
            var sql = $"SELECT Id, Nombre, PuntoInicioId, PuntoFinId, Distancia, Estado, FechaCreacion, FechaModificacion FROM Rutas WHERE Nombre LIKE '%{safe}%';";
            var dt = await _db.ExecuteQueryAsync(sql);
            var list = new List<Ruta>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Ruta
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Nombre = row["Nombre"].ToString() ?? string.Empty,
                    PuntoInicioId = Convert.ToInt32(row["PuntoInicioId"]),
                    PuntoFinId = Convert.ToInt32(row["PuntoFinId"]),
                    Distancia = Convert.ToDouble(row["Distancia"]),
                    Estado = Convert.ToBoolean(row["Estado"]),
                    FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                    FechaModificacion = Convert.ToDateTime(row["FechaModificacion"])
                });
            }
            return list;
        }
    }
}

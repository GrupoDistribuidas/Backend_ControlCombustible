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

        // ========== Métodos para Reportes ==========

        public async Task<(int TotalActivas, int TotalInactivas, int TotalGeneral, double DistanciaTotal)> GetTotalRutasActivasAsync()
        {
            var query = @"
                SELECT 
                    SUM(CASE WHEN Estado = 1 THEN 1 ELSE 0 END) as TotalActivas,
                    SUM(CASE WHEN Estado = 0 THEN 1 ELSE 0 END) as TotalInactivas,
                    COUNT(*) as TotalGeneral,
                    SUM(CASE WHEN Estado = 1 THEN Distancia ELSE 0 END) as DistanciaTotal
                FROM Rutas";

            var dataTable = await _db.ExecuteQueryAsync(query);
            if (dataTable.Rows.Count > 0)
            {
                var row = dataTable.Rows[0];
                return (
                    Convert.ToInt32(row["TotalActivas"]),
                    Convert.ToInt32(row["TotalInactivas"]),
                    Convert.ToInt32(row["TotalGeneral"]),
                    Convert.ToDouble(row["DistanciaTotal"])
                );
            }
            return (0, 0, 0, 0.0);
        }

        public async Task<List<(string Provincia, int TotalRutas, double DistanciaTotal, int RutasActivas, int RutasInactivas)>> GetRutasPorProvinciaAsync()
        {
            var query = @"
                SELECT 
                    p.Provincia,
                    COUNT(DISTINCT r.Id) as TotalRutas,
                    SUM(r.Distancia) as DistanciaTotal,
                    SUM(CASE WHEN r.Estado = 1 THEN 1 ELSE 0 END) as RutasActivas,
                    SUM(CASE WHEN r.Estado = 0 THEN 1 ELSE 0 END) as RutasInactivas
                FROM Rutas r
                INNER JOIN Puntos p ON (r.PuntoInicioId = p.Id OR r.PuntoFinId = p.Id)
                GROUP BY p.Provincia
                ORDER BY TotalRutas DESC, DistanciaTotal DESC";

            var dataTable = await _db.ExecuteQueryAsync(query);
            var resultado = new List<(string, int, double, int, int)>();

            foreach (DataRow row in dataTable.Rows)
            {
                resultado.Add((
                    row["Provincia"]?.ToString() ?? "Sin provincia",
                    Convert.ToInt32(row["TotalRutas"]),
                    Convert.ToDouble(row["DistanciaTotal"]),
                    Convert.ToInt32(row["RutasActivas"]),
                    Convert.ToInt32(row["RutasInactivas"])
                ));
            }

            return resultado;
        }
    }
}

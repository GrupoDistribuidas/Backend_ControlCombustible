using MS.Rutas.Domain.Entities;
using MS.Rutas.Domain.Interfaces;
using MS.Rutas.Services;
using System.Collections.Generic;
using System.Data;

namespace MS.Rutas.Infraestructure.Repositories
{
    public class PuntoRepository : IPuntoRepository
    {
        private readonly IDatabaseService _db;

        public PuntoRepository(IDatabaseService db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(Punto punto)
        {
            var query = @"INSERT INTO Puntos (Nombre, Direccion, Provincia, TipoPunto)
VALUES (@Nombre, @Direccion, @Provincia, @TipoPunto);
SELECT LAST_INSERT_ID();";

            var parameters = new Dictionary<string, object>
            {
                { "@Nombre", punto.Nombre },
                { "@Direccion", punto.Direccion },
                { "@Provincia", punto.Provincia },
                { "@TipoPunto", punto.TipoPunto }
            };

            var result = await _db.ExecuteScalarAsync(query, parameters);
            return Convert.ToInt32(result);
        }

        public async Task<IEnumerable<Punto>> GetAllAsync()
        {
            var dt = await _db.ExecuteQueryAsync("SELECT Id, Nombre, Direccion, Provincia, TipoPunto, FechaCreacion, FechaModificacion FROM Puntos;");
            var list = new List<Punto>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Punto
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Nombre = row["Nombre"].ToString() ?? string.Empty,
                    Direccion = row["Direccion"].ToString() ?? string.Empty,
                    Provincia = row["Provincia"].ToString() ?? string.Empty,
                    TipoPunto = row["TipoPunto"].ToString() ?? string.Empty,
                    FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                    FechaModificacion = Convert.ToDateTime(row["FechaModificacion"])
                });
            }
            return list;
        }

        public async Task<Punto?> GetByIdAsync(int id)
        {
            var dt = await _db.ExecuteQueryAsync($"SELECT Id, Nombre, Direccion, Provincia, TipoPunto, FechaCreacion, FechaModificacion FROM Puntos WHERE Id = {id} LIMIT 1;");
            if (dt.Rows.Count == 0) return null;
            var row = dt.Rows[0];
            return new Punto
            {
                Id = Convert.ToInt32(row["Id"]),
                Nombre = row["Nombre"].ToString() ?? string.Empty,
                Direccion = row["Direccion"].ToString() ?? string.Empty,
                Provincia = row["Provincia"].ToString() ?? string.Empty,
                TipoPunto = row["TipoPunto"].ToString() ?? string.Empty,
                FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                FechaModificacion = Convert.ToDateTime(row["FechaModificacion"])
            };
        }

        public async Task<Punto?> GetByNombreAsync(string nombre)
        {
            var dt = await _db.ExecuteQueryAsync($"SELECT Id, Nombre, Direccion, Provincia, TipoPunto, FechaCreacion, FechaModificacion FROM Puntos WHERE Nombre = '{MySql.Data.MySqlClient.MySqlHelper.EscapeString(nombre)}' LIMIT 1;");
            if (dt.Rows.Count == 0) return null;
            var row = dt.Rows[0];
            return new Punto
            {
                Id = Convert.ToInt32(row["Id"]),
                Nombre = row["Nombre"].ToString() ?? string.Empty,
                Direccion = row["Direccion"].ToString() ?? string.Empty,
                Provincia = row["Provincia"].ToString() ?? string.Empty,
                TipoPunto = row["TipoPunto"].ToString() ?? string.Empty,
                FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                FechaModificacion = Convert.ToDateTime(row["FechaModificacion"])
            };
        }

        public async Task<int> UpdateAsync(Punto punto)
        {
            var query = @"UPDATE Puntos 
SET Nombre = @Nombre, 
    Direccion = @Direccion, 
    Provincia = @Provincia, 
    TipoPunto = @TipoPunto, 
    FechaModificacion = CURRENT_TIMESTAMP 
WHERE Id = @Id;";

            var parameters = new Dictionary<string, object>
            {
                { "@Nombre", punto.Nombre },
                { "@Direccion", punto.Direccion },
                { "@Provincia", punto.Provincia },
                { "@TipoPunto", punto.TipoPunto },
                { "@Id", punto.Id }
            };

            return await _db.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<IEnumerable<Punto>> SearchByTermAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return new List<Punto>();

            var safe = MySql.Data.MySqlClient.MySqlHelper.EscapeString(term);
            var sql = $"SELECT Id, Nombre, Direccion, Provincia, TipoPunto, FechaCreacion, FechaModificacion FROM Puntos WHERE Nombre LIKE '%{safe}%' OR Direccion LIKE '%{safe}%' OR Provincia LIKE '%{safe}%';";
            var dt = await _db.ExecuteQueryAsync(sql);
            var list = new List<Punto>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Punto
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Nombre = row["Nombre"].ToString() ?? string.Empty,
                    Direccion = row["Direccion"].ToString() ?? string.Empty,
                    Provincia = row["Provincia"].ToString() ?? string.Empty,
                    TipoPunto = row["TipoPunto"].ToString() ?? string.Empty,
                    FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                    FechaModificacion = Convert.ToDateTime(row["FechaModificacion"])
                });
            }
            return list;
        }

        // ========== Métodos para Reportes Avanzados ==========

        public async Task<List<(string TipoPunto, int Cantidad, string ProvinciaPrincipal)>> GetPuntosPorTipoAsync()
        {
            var query = @"
                SELECT 
                    TipoPunto,
                    COUNT(*) as Cantidad,
                    (SELECT Provincia 
                     FROM Puntos p2 
                     WHERE p2.TipoPunto = p.TipoPunto 
                     GROUP BY Provincia 
                     ORDER BY COUNT(*) DESC 
                     LIMIT 1) as ProvinciaPrincipal
                FROM Puntos p
                GROUP BY TipoPunto
                ORDER BY Cantidad DESC";

            var dataTable = await _db.ExecuteQueryAsync(query);
            var resultado = new List<(string, int, string)>();

            foreach (DataRow row in dataTable.Rows)
            {
                resultado.Add((
                    row["TipoPunto"]?.ToString() ?? "Desconocido",
                    Convert.ToInt32(row["Cantidad"]),
                    row["ProvinciaPrincipal"]?.ToString() ?? "N/A"
                ));
            }

            return resultado;
        }

        public async Task<List<(int PuntoId, string NombrePunto, string Provincia, string TipoPunto, int VecesComoInicio, int VecesComoFin, int TotalUsos)>> GetPuntosMasUtilizadosAsync()
        {
            var query = @"
                SELECT 
                    p.Id as PuntoId,
                    p.Nombre as NombrePunto,
                    p.Provincia,
                    p.TipoPunto,
                    COALESCE(inicio.VecesComoInicio, 0) as VecesComoInicio,
                    COALESCE(fin.VecesComoFin, 0) as VecesComoFin,
                    (COALESCE(inicio.VecesComoInicio, 0) + COALESCE(fin.VecesComoFin, 0)) as TotalUsos
                FROM Puntos p
                LEFT JOIN (
                    SELECT PuntoInicioId, COUNT(*) as VecesComoInicio
                    FROM Rutas
                    WHERE Estado = 1
                    GROUP BY PuntoInicioId
                ) inicio ON p.Id = inicio.PuntoInicioId
                LEFT JOIN (
                    SELECT PuntoFinId, COUNT(*) as VecesComoFin
                    FROM Rutas
                    WHERE Estado = 1
                    GROUP BY PuntoFinId
                ) fin ON p.Id = fin.PuntoFinId
                HAVING TotalUsos > 0
                ORDER BY TotalUsos DESC
                LIMIT 20";

            var dataTable = await _db.ExecuteQueryAsync(query);
            var resultado = new List<(int, string, string, string, int, int, int)>();

            foreach (DataRow row in dataTable.Rows)
            {
                resultado.Add((
                    Convert.ToInt32(row["PuntoId"]),
                    row["NombrePunto"]?.ToString() ?? "Sin nombre",
                    row["Provincia"]?.ToString() ?? "Sin provincia",
                    row["TipoPunto"]?.ToString() ?? "Desconocido",
                    Convert.ToInt32(row["VecesComoInicio"]),
                    Convert.ToInt32(row["VecesComoFin"]),
                    Convert.ToInt32(row["TotalUsos"])
                ));
            }

            return resultado;
        }
    }
}

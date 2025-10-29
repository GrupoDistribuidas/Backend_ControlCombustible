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
    }
}

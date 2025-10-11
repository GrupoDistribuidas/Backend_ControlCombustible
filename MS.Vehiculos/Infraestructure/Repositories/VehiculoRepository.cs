using MS.Vehiculos.Domain.Entities;
using MS.Vehiculos.Domain.Interfaces;
using MS.Vehiculos.Services;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace MS.Vehiculos.Infraestructure.Repositories
{
    public class VehiculoRepository : IVehiculoRepository
    {
        private readonly IDatabaseService _db;

        public VehiculoRepository(IDatabaseService db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(Vehiculo vehiculo)
        {
            var query = @"INSERT INTO Vehiculos (Nombre, Placa, Marca, Modelo, TipoMaquinariaId, Disponible, ConsumoCombustibleKm, CapacidadCombustible, Estado)
VALUES (@Nombre, @Placa, @Marca, @Modelo, @TipoMaquinariaId, @Disponible, @ConsumoCombustibleKm, @CapacidadCombustible, @Estado);
SELECT LAST_INSERT_ID();";

            var parameters = new Dictionary<string, object>
            {
                { "@Nombre", vehiculo.Nombre },
                { "@Placa", vehiculo.Placa },
                { "@Marca", vehiculo.Marca },
                { "@Modelo", vehiculo.Modelo },
                { "@TipoMaquinariaId", vehiculo.TipoMaquinariaId },
                { "@Disponible", vehiculo.Disponible },
                { "@ConsumoCombustibleKm", vehiculo.ConsumoCombustibleKm },
                { "@CapacidadCombustible", vehiculo.CapacidadCombustible },
                { "@Estado", vehiculo.Estado }
            };

            var result = await _db.ExecuteScalarAsync(query, parameters);
            return Convert.ToInt32(result);
        }

        public async Task<IEnumerable<Vehiculo>> GetAllAsync()
        {
            var dt = await _db.ExecuteQueryAsync("SELECT Id, Nombre, Placa, Marca, Modelo, TipoMaquinariaId, Disponible, ConsumoCombustibleKm, CapacidadCombustible, Estado, FechaCreacion, FechaModificacion FROM Vehiculos WHERE Estado = 1;");
            var list = new List<Vehiculo>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Vehiculo
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Nombre = row["Nombre"].ToString() ?? string.Empty,
                    Placa = row["Placa"].ToString() ?? string.Empty,
                    Marca = row["Marca"].ToString() ?? string.Empty,
                    Modelo = row["Modelo"].ToString() ?? string.Empty,
                    TipoMaquinariaId = Convert.ToInt32(row["TipoMaquinariaId"]),
                    Disponible = row["Disponible"].ToString() ?? string.Empty,
                    ConsumoCombustibleKm = Convert.ToDecimal(row["ConsumoCombustibleKm"]),
                    CapacidadCombustible = Convert.ToDecimal(row["CapacidadCombustible"]),
                    Estado = Convert.ToBoolean(row["Estado"]),
                    FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                    FechaModificacion = Convert.ToDateTime(row["FechaModificacion"])
                });
            }
            return list;
        }

        public async Task<Vehiculo?> GetByIdAsync(int id)
        {
            var dt = await _db.ExecuteQueryAsync($"SELECT Id, Nombre, Placa, Marca, Modelo, TipoMaquinariaId, Disponible, ConsumoCombustibleKm, CapacidadCombustible, Estado, FechaCreacion, FechaModificacion FROM Vehiculos WHERE Id = {id} LIMIT 1;");
            if (dt.Rows.Count == 0) return null;
            var row = dt.Rows[0];
            return new Vehiculo
            {
                Id = Convert.ToInt32(row["Id"]),
                Nombre = row["Nombre"].ToString() ?? string.Empty,
                Placa = row["Placa"].ToString() ?? string.Empty,
                Marca = row["Marca"].ToString() ?? string.Empty,
                Modelo = row["Modelo"].ToString() ?? string.Empty,
                TipoMaquinariaId = Convert.ToInt32(row["TipoMaquinariaId"]),
                Disponible = row["Disponible"].ToString() ?? string.Empty,
                ConsumoCombustibleKm = Convert.ToDecimal(row["ConsumoCombustibleKm"]),
                CapacidadCombustible = Convert.ToDecimal(row["CapacidadCombustible"]),
                Estado = Convert.ToBoolean(row["Estado"]),
                FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                FechaModificacion = Convert.ToDateTime(row["FechaModificacion"])
            };
        }

        public async Task<Vehiculo?> GetByPlacaAsync(string placa)
        {
            // using ExecuteQueryAsync currently does not accept parameters, escape the placa value
            var safePlaca = MySql.Data.MySqlClient.MySqlHelper.EscapeString(placa);
            var dt = await _db.ExecuteQueryAsync($"SELECT Id, Nombre, Placa, Marca, Modelo, TipoMaquinariaId, Disponible, ConsumoCombustibleKm, CapacidadCombustible, Estado, FechaCreacion, FechaModificacion FROM Vehiculos WHERE Placa = '{safePlaca}' LIMIT 1;");
            if (dt.Rows.Count == 0) return null;
            var row = dt.Rows[0];
            return new Vehiculo
            {
                Id = Convert.ToInt32(row["Id"]),
                Nombre = row["Nombre"].ToString() ?? string.Empty,
                Placa = row["Placa"].ToString() ?? string.Empty,
                Marca = row["Marca"].ToString() ?? string.Empty,
                Modelo = row["Modelo"].ToString() ?? string.Empty,
                TipoMaquinariaId = Convert.ToInt32(row["TipoMaquinariaId"]),
                Disponible = row["Disponible"].ToString() ?? string.Empty,
                ConsumoCombustibleKm = Convert.ToDecimal(row["ConsumoCombustibleKm"]),
                CapacidadCombustible = Convert.ToDecimal(row["CapacidadCombustible"]),
                Estado = Convert.ToBoolean(row["Estado"]),
                FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                FechaModificacion = Convert.ToDateTime(row["FechaModificacion"])
            };
        }

        public async Task<int> UpdateAsync(Vehiculo vehiculo, bool? estado = null)
        {
            // Build SET clause dynamically to avoid overwriting Estado when not provided
            var setParts = new List<string>
            {
                "Nombre = @Nombre",
                "Placa = @Placa",
                "Marca = @Marca",
                "Modelo = @Modelo",
                "TipoMaquinariaId = @TipoMaquinariaId",
                "Disponible = @Disponible",
                "ConsumoCombustibleKm = @ConsumoCombustibleKm",
                "CapacidadCombustible = @CapacidadCombustible"
            };

            if (estado.HasValue)
            {
                setParts.Add("Estado = @Estado");
            }

            var setClause = string.Join(", ", setParts) + ", FechaModificacion = CURRENT_TIMESTAMP";
            var query = $"UPDATE Vehiculos SET {setClause} WHERE Id = @Id;";

            var parameters = new Dictionary<string, object>
            {
                { "@Nombre", vehiculo.Nombre },
                { "@Placa", vehiculo.Placa },
                { "@Marca", vehiculo.Marca },
                { "@Modelo", vehiculo.Modelo },
                { "@TipoMaquinariaId", vehiculo.TipoMaquinariaId },
                { "@Disponible", vehiculo.Disponible },
                { "@ConsumoCombustibleKm", vehiculo.ConsumoCombustibleKm },
                { "@CapacidadCombustible", vehiculo.CapacidadCombustible },
                { "@Id", vehiculo.Id }
            };

            if (estado.HasValue)
            {
                parameters.Add("@Estado", estado.Value);
            }

            return await _db.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<IEnumerable<Vehiculo>> SearchAsync(MS.Vehiculos.Application.DTOs.VehiculoFilterDto filter)
        {
            var where = new List<string>();
            if (filter.Estado.HasValue)
                where.Add($"Estado = {(filter.Estado.Value ? 1 : 0)}");
            if (filter.TipoMaquinariaId.HasValue)
                where.Add($"TipoMaquinariaId = {filter.TipoMaquinariaId.Value}");
            if (!string.IsNullOrWhiteSpace(filter.Marca))
                where.Add($"Marca LIKE '%{MySql.Data.MySqlClient.MySqlHelper.EscapeString(filter.Marca)}%'");
            if (!string.IsNullOrWhiteSpace(filter.Modelo))
                where.Add($"Modelo LIKE '%{MySql.Data.MySqlClient.MySqlHelper.EscapeString(filter.Modelo)}%'");
            if (!string.IsNullOrWhiteSpace(filter.Disponible))
                where.Add($"Disponible = '{MySql.Data.MySqlClient.MySqlHelper.EscapeString(filter.Disponible)}'");
            if (filter.CapacidadMin.HasValue)
                where.Add($"CapacidadCombustible >= {filter.CapacidadMin.Value.ToString(CultureInfo.InvariantCulture)}");
            if (filter.CapacidadMax.HasValue)
                where.Add($"CapacidadCombustible <= {filter.CapacidadMax.Value.ToString(CultureInfo.InvariantCulture)}");
            if (filter.ConsumoMin.HasValue)
                where.Add($"ConsumoCombustibleKm >= {filter.ConsumoMin.Value.ToString(CultureInfo.InvariantCulture)}");
            if (filter.ConsumoMax.HasValue)
                where.Add($"ConsumoCombustibleKm <= {filter.ConsumoMax.Value.ToString(CultureInfo.InvariantCulture)}");

            var whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";
            var sql = $"SELECT Id, Nombre, Placa, Marca, Modelo, TipoMaquinariaId, Disponible, ConsumoCombustibleKm, CapacidadCombustible, Estado, FechaCreacion, FechaModificacion FROM Vehiculos {whereClause};";
            var dt = await _db.ExecuteQueryAsync(sql);

            var list = new List<Vehiculo>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Vehiculo
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Nombre = row["Nombre"].ToString() ?? string.Empty,
                    Placa = row["Placa"].ToString() ?? string.Empty,
                    Marca = row["Marca"].ToString() ?? string.Empty,
                    Modelo = row["Modelo"].ToString() ?? string.Empty,
                    TipoMaquinariaId = Convert.ToInt32(row["TipoMaquinariaId"]),
                    Disponible = row["Disponible"].ToString() ?? string.Empty,
                    ConsumoCombustibleKm = Convert.ToDecimal(row["ConsumoCombustibleKm"]),
                    CapacidadCombustible = Convert.ToDecimal(row["CapacidadCombustible"]),
                    Estado = Convert.ToBoolean(row["Estado"]),
                    FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                    FechaModificacion = Convert.ToDateTime(row["FechaModificacion"])
                });
            }
            return list;
        }

        public async Task<IEnumerable<Vehiculo>> SearchByTermAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return new List<Vehiculo>();
            }

            var safe = MySql.Data.MySqlClient.MySqlHelper.EscapeString(term);
            var sql = $"SELECT Id, Nombre, Placa, Marca, Modelo, TipoMaquinariaId, Disponible, ConsumoCombustibleKm, CapacidadCombustible, Estado, FechaCreacion, FechaModificacion FROM Vehiculos WHERE Nombre LIKE '%{safe}%' OR Placa LIKE '%{safe}%';";
            var dt = await _db.ExecuteQueryAsync(sql);
            var list = new List<Vehiculo>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Vehiculo
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Nombre = row["Nombre"].ToString() ?? string.Empty,
                    Placa = row["Placa"].ToString() ?? string.Empty,
                    Marca = row["Marca"].ToString() ?? string.Empty,
                    Modelo = row["Modelo"].ToString() ?? string.Empty,
                    TipoMaquinariaId = Convert.ToInt32(row["TipoMaquinariaId"]),
                    Disponible = row["Disponible"].ToString() ?? string.Empty,
                    ConsumoCombustibleKm = Convert.ToDecimal(row["ConsumoCombustibleKm"]),
                    CapacidadCombustible = Convert.ToDecimal(row["CapacidadCombustible"]),
                    Estado = Convert.ToBoolean(row["Estado"]),
                    FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                    FechaModificacion = Convert.ToDateTime(row["FechaModificacion"])
                });
            }
            return list;
        }
    }
}

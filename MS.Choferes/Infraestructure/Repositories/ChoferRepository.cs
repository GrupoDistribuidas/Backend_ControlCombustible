using MS.Choferes.Domain.Entities;
using MS.Choferes.Domain.Interfaces;
using MS.Choferes.Services;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace MS.Choferes.Infraestructure.Repositories
{
    public class ChoferRepository : IChoferRepository
    {
        private readonly IDatabaseService _db;

        public ChoferRepository(IDatabaseService db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(Chofer chofer)
        {
            var query = @"INSERT INTO Choferes (PrimerNombre, SegundoNombre, PrimerApellido, SegundoApellido, Identificacion, FechaNacimiento, Disponible, UsuarioId, TipoMaquinariaId, Estado)
VALUES (@PrimerNombre, @SegundoNombre, @PrimerApellido, @SegundoApellido, @Identificacion, @FechaNacimiento, @Disponible, @UsuarioId, @TipoMaquinariaId, @Estado);
SELECT LAST_INSERT_ID();";

            var parameters = new Dictionary<string, object>
            {
                { "@PrimerNombre", chofer.PrimerNombre },
                { "@SegundoNombre", chofer.SegundoNombre ?? (object)DBNull.Value },
                { "@PrimerApellido", chofer.PrimerApellido },
                { "@SegundoApellido", chofer.SegundoApellido ?? (object)DBNull.Value },
                { "@Identificacion", chofer.Identificacion },
                { "@FechaNacimiento", chofer.FechaNacimiento },
                { "@Disponible", chofer.Disponible },
                { "@UsuarioId", chofer.UsuarioId },
                { "@TipoMaquinariaId", chofer.TipoMaquinariaId },
                { "@Estado", chofer.Estado }
            };

            var result = await _db.ExecuteScalarAsync(query, parameters);
            return Convert.ToInt32(result);
        }

        public async Task<IEnumerable<Chofer>> GetAllAsync()
        {
            var dt = await _db.ExecuteQueryAsync("SELECT Id, PrimerNombre, SegundoNombre, PrimerApellido, SegundoApellido, NombreCompleto, Identificacion, FechaNacimiento, Disponible, UsuarioId, TipoMaquinariaId, Estado, FechaCreacion, FechaModificacion FROM Choferes WHERE Estado = 1;");
            var list = new List<Chofer>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Chofer
                {
                    Id = Convert.ToInt32(row["Id"]),
                    PrimerNombre = row["PrimerNombre"].ToString() ?? string.Empty,
                    SegundoNombre = row["SegundoNombre"].ToString(),
                    PrimerApellido = row["PrimerApellido"].ToString() ?? string.Empty,
                    SegundoApellido = row["SegundoApellido"].ToString(),
                    NombreCompleto = row["NombreCompleto"].ToString() ?? string.Empty,
                    Identificacion = row["Identificacion"].ToString() ?? string.Empty,
                    FechaNacimiento = Convert.ToDateTime(row["FechaNacimiento"]),
                    Disponible = Convert.ToBoolean(row["Disponible"]),
                    UsuarioId = Convert.ToInt32(row["UsuarioId"]),
                    TipoMaquinariaId = Convert.ToInt32(row["TipoMaquinariaId"]),
                    Estado = Convert.ToBoolean(row["Estado"]),
                    FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                    FechaModificacion = Convert.ToDateTime(row["FechaModificacion"])
                });
            }
            return list;
        }

        public async Task<IEnumerable<Chofer>> GetAllIncludingInactiveAsync()
        {
            var dt = await _db.ExecuteQueryAsync("SELECT Id, PrimerNombre, SegundoNombre, PrimerApellido, SegundoApellido, NombreCompleto, Identificacion, FechaNacimiento, Disponible, UsuarioId, TipoMaquinariaId, Estado, FechaCreacion, FechaModificacion FROM Choferes;");
            var list = new List<Chofer>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Chofer
                {
                    Id = Convert.ToInt32(row["Id"]),
                    PrimerNombre = row["PrimerNombre"].ToString() ?? string.Empty,
                    SegundoNombre = row["SegundoNombre"].ToString(),
                    PrimerApellido = row["PrimerApellido"].ToString() ?? string.Empty,
                    SegundoApellido = row["SegundoApellido"].ToString(),
                    NombreCompleto = row["NombreCompleto"].ToString() ?? string.Empty,
                    Identificacion = row["Identificacion"].ToString() ?? string.Empty,
                    FechaNacimiento = Convert.ToDateTime(row["FechaNacimiento"]),
                    Disponible = Convert.ToBoolean(row["Disponible"]),
                    UsuarioId = Convert.ToInt32(row["UsuarioId"]),
                    TipoMaquinariaId = Convert.ToInt32(row["TipoMaquinariaId"]),
                    Estado = Convert.ToBoolean(row["Estado"]),
                    FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                    FechaModificacion = Convert.ToDateTime(row["FechaModificacion"])
                });
            }
            return list;
        }

        public async Task<Chofer?> GetByIdAsync(int id)
        {
            var dt = await _db.ExecuteQueryAsync($"SELECT Id, PrimerNombre, SegundoNombre, PrimerApellido, SegundoApellido, NombreCompleto, Identificacion, FechaNacimiento, Disponible, UsuarioId, TipoMaquinariaId, Estado, FechaCreacion, FechaModificacion FROM Choferes WHERE Id = {id} LIMIT 1;");
            if (dt.Rows.Count == 0) return null;
            var row = dt.Rows[0];
            return new Chofer
            {
                Id = Convert.ToInt32(row["Id"]),
                PrimerNombre = row["PrimerNombre"].ToString() ?? string.Empty,
                SegundoNombre = row["SegundoNombre"].ToString(),
                PrimerApellido = row["PrimerApellido"].ToString() ?? string.Empty,
                SegundoApellido = row["SegundoApellido"].ToString(),
                NombreCompleto = row["NombreCompleto"].ToString() ?? string.Empty,
                Identificacion = row["Identificacion"].ToString() ?? string.Empty,
                FechaNacimiento = Convert.ToDateTime(row["FechaNacimiento"]),
                Disponible = Convert.ToBoolean(row["Disponible"]),
                UsuarioId = Convert.ToInt32(row["UsuarioId"]),
                TipoMaquinariaId = Convert.ToInt32(row["TipoMaquinariaId"]),
                Estado = Convert.ToBoolean(row["Estado"]),
                FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                FechaModificacion = Convert.ToDateTime(row["FechaModificacion"])
            };
        }

        public async Task<Chofer?> GetByIdentificacionAsync(string identificacion)
        {
            var dt = await _db.ExecuteQueryAsync($"SELECT Id, PrimerNombre, SegundoNombre, PrimerApellido, SegundoApellido, NombreCompleto, Identificacion, FechaNacimiento, Disponible, UsuarioId, TipoMaquinariaId, Estado, FechaCreacion, FechaModificacion FROM Choferes WHERE Identificacion = '{MySql.Data.MySqlClient.MySqlHelper.EscapeString(identificacion)}' LIMIT 1;");
            if (dt.Rows.Count == 0) return null;
            var row = dt.Rows[0];
            return new Chofer
            {
                Id = Convert.ToInt32(row["Id"]),
                PrimerNombre = row["PrimerNombre"].ToString() ?? string.Empty,
                SegundoNombre = row["SegundoNombre"].ToString(),
                PrimerApellido = row["PrimerApellido"].ToString() ?? string.Empty,
                SegundoApellido = row["SegundoApellido"].ToString(),
                NombreCompleto = row["NombreCompleto"].ToString() ?? string.Empty,
                Identificacion = row["Identificacion"].ToString() ?? string.Empty,
                FechaNacimiento = Convert.ToDateTime(row["FechaNacimiento"]),
                Disponible = Convert.ToBoolean(row["Disponible"]),
                UsuarioId = Convert.ToInt32(row["UsuarioId"]),
                TipoMaquinariaId = Convert.ToInt32(row["TipoMaquinariaId"]),
                Estado = Convert.ToBoolean(row["Estado"]),
                FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                FechaModificacion = Convert.ToDateTime(row["FechaModificacion"])
            };
        }

        public async Task<Chofer?> GetByUsuarioIdAsync(int usuarioId)
        {
            var dt = await _db.ExecuteQueryAsync($"SELECT Id, PrimerNombre, SegundoNombre, PrimerApellido, SegundoApellido, NombreCompleto, Identificacion, FechaNacimiento, Disponible, UsuarioId, TipoMaquinariaId, Estado, FechaCreacion, FechaModificacion FROM Choferes WHERE UsuarioId = {usuarioId} LIMIT 1;");
            if (dt.Rows.Count == 0) return null;
            var row = dt.Rows[0];
            return new Chofer
            {
                Id = Convert.ToInt32(row["Id"]),
                PrimerNombre = row["PrimerNombre"].ToString() ?? string.Empty,
                SegundoNombre = row["SegundoNombre"].ToString(),
                PrimerApellido = row["PrimerApellido"].ToString() ?? string.Empty,
                SegundoApellido = row["SegundoApellido"].ToString(),
                NombreCompleto = row["NombreCompleto"].ToString() ?? string.Empty,
                Identificacion = row["Identificacion"].ToString() ?? string.Empty,
                FechaNacimiento = Convert.ToDateTime(row["FechaNacimiento"]),
                Disponible = Convert.ToBoolean(row["Disponible"]),
                UsuarioId = Convert.ToInt32(row["UsuarioId"]),
                TipoMaquinariaId = Convert.ToInt32(row["TipoMaquinariaId"]),
                Estado = Convert.ToBoolean(row["Estado"]),
                FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                FechaModificacion = Convert.ToDateTime(row["FechaModificacion"])
            };
        }

        public async Task<int> UpdateAsync(Chofer chofer, bool? estado = null)
        {
            var setParts = new List<string>
            {
                "PrimerNombre = @PrimerNombre",
                "SegundoNombre = @SegundoNombre",
                "PrimerApellido = @PrimerApellido",
                "SegundoApellido = @SegundoApellido",
                "Identificacion = @Identificacion",
                "FechaNacimiento = @FechaNacimiento",
                "Disponible = @Disponible",
                "UsuarioId = @UsuarioId",
                "TipoMaquinariaId = @TipoMaquinariaId"
            };

            if (estado.HasValue)
                setParts.Add("Estado = @Estado");

            var setClause = string.Join(", ", setParts) + ", FechaModificacion = CURRENT_TIMESTAMP";
            var query = $"UPDATE Choferes SET {setClause} WHERE Id = @Id;";

            var parameters = new Dictionary<string, object>
            {
                { "@PrimerNombre", chofer.PrimerNombre },
                { "@SegundoNombre", chofer.SegundoNombre ?? (object)DBNull.Value },
                { "@PrimerApellido", chofer.PrimerApellido },
                { "@SegundoApellido", chofer.SegundoApellido ?? (object)DBNull.Value },
                { "@Identificacion", chofer.Identificacion },
                { "@FechaNacimiento", chofer.FechaNacimiento },
                { "@Disponible", chofer.Disponible },
                { "@UsuarioId", chofer.UsuarioId },
                { "@TipoMaquinariaId", chofer.TipoMaquinariaId },
                { "@Id", chofer.Id }
            };

            if (estado.HasValue)
                parameters.Add("@Estado", estado.Value);

            return await _db.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<int> UpdateEstadoAsync(int id, bool estado)
        {
            var query = "UPDATE Choferes SET Estado = @Estado, FechaModificacion = CURRENT_TIMESTAMP WHERE Id = @Id;";
            var parameters = new Dictionary<string, object>
            {
                { "@Estado", estado },
                { "@Id", id }
            };
            return await _db.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<int> UpdateDisponibilidadAsync(int id, bool disponible)
        {
            var query = "UPDATE Choferes SET Disponible = @Disponible, FechaModificacion = CURRENT_TIMESTAMP WHERE Id = @Id;";
            var parameters = new Dictionary<string, object>
            {
                { "@Disponible", disponible },
                { "@Id", id }
            };
            return await _db.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<int> UpdateUsuarioAsync(int choferId, int usuarioId)
        {
            var query = "UPDATE Choferes SET UsuarioId = @UsuarioId, FechaModificacion = CURRENT_TIMESTAMP WHERE Id = @ChoferId;";
            var parameters = new Dictionary<string, object>
            {
                { "@UsuarioId", usuarioId },
                { "@ChoferId", choferId }
            };
            return await _db.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<IEnumerable<Chofer>> SearchAsync(MS.Choferes.Application.DTOs.ChoferFilterDto filter)
        {
            var where = new List<string>();
            if (filter.Estado.HasValue)
                where.Add($"Estado = {(filter.Estado.Value ? 1 : 0)}");
            if (filter.TipoMaquinariaId.HasValue)
                where.Add($"TipoMaquinariaId = {filter.TipoMaquinariaId.Value}");
            if (filter.Disponible.HasValue)
                where.Add($"Disponible = {(filter.Disponible.Value ? 1 : 0)}");
            if (filter.FechaNacimientoDesde.HasValue)
                where.Add($"FechaNacimiento >= '{filter.FechaNacimientoDesde.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}'");
            if (filter.FechaNacimientoHasta.HasValue)
                where.Add($"FechaNacimiento <= '{filter.FechaNacimientoHasta.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}'");

            var whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";
            var sql = $"SELECT Id, PrimerNombre, SegundoNombre, PrimerApellido, SegundoApellido, NombreCompleto, Identificacion, FechaNacimiento, Disponible, UsuarioId, TipoMaquinariaId, Estado, FechaCreacion, FechaModificacion FROM Choferes {whereClause};";
            var dt = await _db.ExecuteQueryAsync(sql);

            var list = new List<Chofer>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Chofer
                {
                    Id = Convert.ToInt32(row["Id"]),
                    PrimerNombre = row["PrimerNombre"].ToString() ?? string.Empty,
                    SegundoNombre = row["SegundoNombre"].ToString(),
                    PrimerApellido = row["PrimerApellido"].ToString() ?? string.Empty,
                    SegundoApellido = row["SegundoApellido"].ToString(),
                    NombreCompleto = row["NombreCompleto"].ToString() ?? string.Empty,
                    Identificacion = row["Identificacion"].ToString() ?? string.Empty,
                    FechaNacimiento = Convert.ToDateTime(row["FechaNacimiento"]),
                    Disponible = Convert.ToBoolean(row["Disponible"]),
                    UsuarioId = Convert.ToInt32(row["UsuarioId"]),
                    TipoMaquinariaId = Convert.ToInt32(row["TipoMaquinariaId"]),
                    Estado = Convert.ToBoolean(row["Estado"]),
                    FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                    FechaModificacion = Convert.ToDateTime(row["FechaModificacion"])
                });
            }
            return list;
        }

        public async Task<IEnumerable<Chofer>> SearchByTermAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return new List<Chofer>();

            var safe = MySql.Data.MySqlClient.MySqlHelper.EscapeString(term);
            var sql = $"SELECT Id, PrimerNombre, SegundoNombre, PrimerApellido, SegundoApellido, NombreCompleto, Identificacion, FechaNacimiento, Disponible, UsuarioId, TipoMaquinariaId, Estado, FechaCreacion, FechaModificacion FROM Choferes WHERE NombreCompleto LIKE '%{safe}%' OR Identificacion LIKE '%{safe}%';";
            var dt = await _db.ExecuteQueryAsync(sql);
            var list = new List<Chofer>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Chofer
                {
                    Id = Convert.ToInt32(row["Id"]),
                    PrimerNombre = row["PrimerNombre"].ToString() ?? string.Empty,
                    SegundoNombre = row["SegundoNombre"].ToString(),
                    PrimerApellido = row["PrimerApellido"].ToString() ?? string.Empty,
                    SegundoApellido = row["SegundoApellido"].ToString(),
                    NombreCompleto = row["NombreCompleto"].ToString() ?? string.Empty,
                    Identificacion = row["Identificacion"].ToString() ?? string.Empty,
                    FechaNacimiento = Convert.ToDateTime(row["FechaNacimiento"]),
                    Disponible = Convert.ToBoolean(row["Disponible"]),
                    UsuarioId = Convert.ToInt32(row["UsuarioId"]),
                    TipoMaquinariaId = Convert.ToInt32(row["TipoMaquinariaId"]),
                    Estado = Convert.ToBoolean(row["Estado"]),
                    FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                    FechaModificacion = Convert.ToDateTime(row["FechaModificacion"])
                });
            }
            return list;
        }

        // ========== Métodos para Reportes ==========

        public async Task<(int TotalActivos, int TotalInactivos, int TotalDisponibles, int TotalGeneral)> GetTotalChoferesActivosAsync()
        {
            var query = @"
                SELECT 
                    SUM(CASE WHEN Estado = 1 THEN 1 ELSE 0 END) as TotalActivos,
                    SUM(CASE WHEN Estado = 0 THEN 1 ELSE 0 END) as TotalInactivos,
                    SUM(CASE WHEN Estado = 1 AND Disponible = 1 THEN 1 ELSE 0 END) as TotalDisponibles,
                    COUNT(*) as TotalGeneral
                FROM Choferes";

            var dataTable = await _db.ExecuteQueryAsync(query);
            if (dataTable.Rows.Count > 0)
            {
                var row = dataTable.Rows[0];
                return (
                    Convert.ToInt32(row["TotalActivos"]),
                    Convert.ToInt32(row["TotalInactivos"]),
                    Convert.ToInt32(row["TotalDisponibles"]),
                    Convert.ToInt32(row["TotalGeneral"])
                );
            }
            return (0, 0, 0, 0);
        }
    }
}

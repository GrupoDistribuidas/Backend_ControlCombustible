using MS.Combustible.Domain.Entities;
using MS.Combustible.Domain.Interfaces;
using MS.Combustible.Services;
using System.Collections.Generic;
using System.Data;

namespace MS.Combustible.Infrastructure.Repositories
{
    public class RegistroConsumoRepository : IRegistroConsumoRepository
    {
        private readonly IDatabaseService _db;

        public RegistroConsumoRepository(IDatabaseService db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(RegistroConsumo registroConsumo)
        {
            var query = @"INSERT INTO registrosconsumo 
                (AsignacionRutaId, FechaRegistro, CombustibleEstimado, CombustibleReal, Motivo, EstadoId, FechaCreacion, FechaModificacion)
                VALUES (@AsignacionRutaId, @FechaRegistro, @CombustibleEstimado, @CombustibleReal, @Motivo, @EstadoId, @FechaCreacion, @FechaModificacion);
                SELECT LAST_INSERT_ID();";

            var parameters = new Dictionary<string, object>
            {
                { "@AsignacionRutaId", registroConsumo.AsignacionRutaId },
                { "@FechaRegistro", registroConsumo.FechaRegistro },
                { "@CombustibleEstimado", registroConsumo.CombustibleEstimado },
                { "@CombustibleReal", registroConsumo.CombustibleReal },
                { "@Motivo", registroConsumo.Motivo },
                { "@EstadoId", registroConsumo.EstadoId },
                { "@FechaCreacion", registroConsumo.FechaCreacion },
                { "@FechaModificacion", registroConsumo.FechaModificacion }
            };

            var result = await _db.ExecuteScalarAsync(query, parameters);
            return Convert.ToInt32(result);
        }

        public async Task<int> UpdateAsync(RegistroConsumo registroConsumo)
        {
            var query = @"UPDATE registrosconsumo SET 
                FechaRegistro = @FechaRegistro,
                CombustibleEstimado = @CombustibleEstimado,
                CombustibleReal = @CombustibleReal,
                Motivo = @Motivo,
                EstadoId = @EstadoId,
                FechaModificacion = @FechaModificacion
                WHERE Id = @Id";

            var parameters = new Dictionary<string, object>
            {
                { "@Id", registroConsumo.Id },
                { "@FechaRegistro", registroConsumo.FechaRegistro },
                { "@CombustibleEstimado", registroConsumo.CombustibleEstimado },
                { "@CombustibleReal", registroConsumo.CombustibleReal },
                { "@Motivo", registroConsumo.Motivo },
                { "@EstadoId", registroConsumo.EstadoId },
                { "@FechaModificacion", registroConsumo.FechaModificacion }
            };

            return await _db.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<int> UpdateEstadoAsync(int id, int estadoId)
        {
            var query = @"UPDATE registrosconsumo SET 
                EstadoId = @EstadoId,
                FechaModificacion = @FechaModificacion
                WHERE Id = @Id";

            var parameters = new Dictionary<string, object>
            {
                { "@Id", id },
                { "@EstadoId", estadoId },
                { "@FechaModificacion", DateTime.Now }
            };

            return await _db.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<RegistroConsumo?> GetByIdAsync(int id)
        {
            var query = "SELECT * FROM registrosconsumo WHERE Id = @Id";
            var parameters = new Dictionary<string, object> { { "@Id", id } };

            var result = await _db.ExecuteQueryAsync(query, parameters);
            if (result.Rows.Count == 0) return null;

            return MapToEntity(result.Rows[0]);
        }

        public async Task<IEnumerable<RegistroConsumo>> GetAllAsync()
        {
            var query = "SELECT * FROM registrosconsumo ORDER BY FechaCreacion DESC";
            var result = await _db.ExecuteQueryAsync(query);
            return result.AsEnumerable().Select<DataRow, RegistroConsumo>(MapToEntity);
        }

        public async Task<IEnumerable<RegistroConsumo>> GetByAsignacionRutaIdAsync(int asignacionRutaId)
        {
            var query = "SELECT * FROM registrosconsumo WHERE AsignacionRutaId = @AsignacionRutaId ORDER BY FechaCreacion DESC";
            var parameters = new Dictionary<string, object> { { "@AsignacionRutaId", asignacionRutaId } };

            var result = await _db.ExecuteQueryAsync(query, parameters);
            return result.AsEnumerable().Select<DataRow, RegistroConsumo>(MapToEntity);
        }

        public async Task<IEnumerable<RegistroConsumo>> GetByEstadoIdAsync(int estadoId)
        {
            var query = "SELECT * FROM registrosconsumo WHERE EstadoId = @EstadoId ORDER BY FechaCreacion DESC";
            var parameters = new Dictionary<string, object> { { "@EstadoId", estadoId } };

            var result = await _db.ExecuteQueryAsync(query, parameters);
            return result.AsEnumerable().Select<DataRow, RegistroConsumo>(MapToEntity);
        }

        public async Task<IEnumerable<RegistroConsumo>> GetByFechaRangoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var query = @"SELECT * FROM registrosconsumo 
                WHERE FechaRegistro >= @FechaInicio AND FechaRegistro <= @FechaFin 
                ORDER BY FechaRegistro DESC";

            var parameters = new Dictionary<string, object>
            {
                { "@FechaInicio", fechaInicio },
                { "@FechaFin", fechaFin }
            };

            var result = await _db.ExecuteQueryAsync(query, parameters);
            return result.AsEnumerable().Select<DataRow, RegistroConsumo>(MapToEntity);
        }

        public async Task<RegistroConsumo?> GetByAsignacionRutaIdAndEstadoAsync(int asignacionRutaId, int estadoId)
        {
            var query = "SELECT * FROM registrosconsumo WHERE AsignacionRutaId = @AsignacionRutaId AND EstadoId = @EstadoId";
            var parameters = new Dictionary<string, object>
            {
                { "@AsignacionRutaId", asignacionRutaId },
                { "@EstadoId", estadoId }
            };

            var result = await _db.ExecuteQueryAsync(query, parameters);
            if (result.Rows.Count == 0) return null;

            return MapToEntity(result.Rows[0]);
        }

        public async Task<bool> ExistsByAsignacionRutaIdAsync(int asignacionRutaId)
        {
            var query = "SELECT COUNT(*) FROM registrosconsumo WHERE AsignacionRutaId = @AsignacionRutaId";
            var parameters = new Dictionary<string, object> { { "@AsignacionRutaId", asignacionRutaId } };

            var result = await _db.ExecuteScalarAsync(query, parameters);
            var count = Convert.ToInt32(result);
            return count > 0;
        }

        private RegistroConsumo MapToEntity(DataRow row)
        {
            return new RegistroConsumo
            {
                Id = Convert.ToInt32(row["Id"]),
                AsignacionRutaId = Convert.ToInt32(row["AsignacionRutaId"]),
                FechaRegistro = Convert.ToDateTime(row["FechaRegistro"]),
                CombustibleEstimado = Convert.ToDouble(row["CombustibleEstimado"]),
                CombustibleReal = Convert.ToDouble(row["CombustibleReal"]),
                Motivo = row["Motivo"]?.ToString() ?? string.Empty,
                EstadoId = Convert.ToInt32(row["EstadoId"]),
                FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                FechaModificacion = Convert.ToDateTime(row["FechaModificacion"])
            };
        }
    }
}
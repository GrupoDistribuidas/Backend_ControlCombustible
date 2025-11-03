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

        // ========== Métodos para Reportes ==========

        public async Task<double> GetConsumoPromedioAsync()
        {
            var query = "SELECT AVG(CombustibleReal) as ConsumoPromedio FROM registrosconsumo";
            var result = await _db.ExecuteScalarAsync(query);
            return result != null && result != DBNull.Value ? Convert.ToDouble(result) : 0.0;
        }

        public async Task<int> GetTotalRegistrosAsync()
        {
            var query = "SELECT COUNT(*) FROM registrosconsumo";
            var result = await _db.ExecuteScalarAsync(query);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task<List<(int RutaId, double ConsumoPromedio, int TotalViajes, double TotalEstimado, double TotalReal)>> GetConsumoPorRutaAsync()
        {
            var query = @"
                SELECT 
                    ar.RutaId,
                    AVG(rc.CombustibleReal) as ConsumoPromedio,
                    COUNT(rc.Id) as TotalViajes,
                    SUM(rc.CombustibleEstimado) as TotalEstimado,
                    SUM(rc.CombustibleReal) as TotalReal
                FROM registrosconsumo rc
                INNER JOIN asignacionesrutas ar ON rc.AsignacionRutaId = ar.Id
                GROUP BY ar.RutaId
                ORDER BY ConsumoPromedio DESC";

            var dataTable = await _db.ExecuteQueryAsync(query);
            var resultado = new List<(int, double, int, double, double)>();

            foreach (DataRow row in dataTable.Rows)
            {
                resultado.Add((
                    Convert.ToInt32(row["RutaId"]),
                    Convert.ToDouble(row["ConsumoPromedio"]),
                    Convert.ToInt32(row["TotalViajes"]),
                    Convert.ToDouble(row["TotalEstimado"]),
                    Convert.ToDouble(row["TotalReal"])
                ));
            }

            return resultado;
        }

        // ========== Métodos para Reportes Avanzados ==========

        public async Task<List<(int TipoVehiculoId, string TipoVehiculoNombre, double ConsumoPromedioReal, double ConsumoPromedioEstimado, double DesviacionPromedio, int TotalViajes, double CombustibleTotalReal)>> GetConsumoPorTipoVehiculoAsync()
        {
            // Solo consulta FuelDB - El enriquecimiento se hace en el servicio gRPC
            var query = @"
                SELECT 
                    ar.VehiculoId as TipoVehiculoId,
                    '' as TipoVehiculoNombre,
                    AVG(rc.CombustibleReal) as ConsumoPromedioReal,
                    AVG(rc.CombustibleEstimado) as ConsumoPromedioEstimado,
                    AVG(rc.CombustibleReal - rc.CombustibleEstimado) as DesviacionPromedio,
                    COUNT(rc.Id) as TotalViajes,
                    SUM(rc.CombustibleReal) as CombustibleTotalReal
                FROM registrosconsumo rc
                INNER JOIN asignacionesrutas ar ON rc.AsignacionRutaId = ar.Id
                GROUP BY ar.VehiculoId
                ORDER BY ConsumoPromedioReal DESC";

            var dataTable = await _db.ExecuteQueryAsync(query);
            var resultado = new List<(int, string, double, double, double, int, double)>();

            foreach (DataRow row in dataTable.Rows)
            {
                resultado.Add((
                    Convert.ToInt32(row["TipoVehiculoId"]),
                    row["TipoVehiculoNombre"]?.ToString() ?? "",
                    Convert.ToDouble(row["ConsumoPromedioReal"]),
                    Convert.ToDouble(row["ConsumoPromedioEstimado"]),
                    Convert.ToDouble(row["DesviacionPromedio"]),
                    Convert.ToInt32(row["TotalViajes"]),
                    Convert.ToDouble(row["CombustibleTotalReal"])
                ));
            }

            return resultado;
        }

        public async Task<List<(int RegistroId, int AsignacionId, int RutaId, string RutaNombre, int VehiculoId, string VehiculoNombre, double CombustibleEstimado, double CombustibleReal, double Desviacion, double PorcentajeDesviacion, string Motivo)>> GetDesviacionesCombustibleAsync(int limite = 50)
        {
            // Solo consulta FuelDB - El enriquecimiento se hace en el servicio gRPC
            var query = $@"
                SELECT 
                    rc.Id as RegistroId,
                    rc.AsignacionRutaId as AsignacionId,
                    ar.RutaId,
                    '' as RutaNombre,
                    ar.VehiculoId,
                    '' as VehiculoNombre,
                    rc.CombustibleEstimado,
                    rc.CombustibleReal,
                    (rc.CombustibleReal - rc.CombustibleEstimado) as Desviacion,
                    ((rc.CombustibleReal - rc.CombustibleEstimado) / rc.CombustibleEstimado * 100) as PorcentajeDesviacion,
                    COALESCE(rc.Motivo, '') as Motivo
                FROM registrosconsumo rc
                INNER JOIN asignacionesrutas ar ON rc.AsignacionRutaId = ar.Id
                WHERE ABS(rc.CombustibleReal - rc.CombustibleEstimado) > 0.01
                ORDER BY ABS(rc.CombustibleReal - rc.CombustibleEstimado) DESC
                LIMIT {limite}";

            var dataTable = await _db.ExecuteQueryAsync(query);
            var resultado = new List<(int, int, int, string, int, string, double, double, double, double, string)>();

            foreach (DataRow row in dataTable.Rows)
            {
                resultado.Add((
                    Convert.ToInt32(row["RegistroId"]),
                    Convert.ToInt32(row["AsignacionId"]),
                    Convert.ToInt32(row["RutaId"]),
                    row["RutaNombre"]?.ToString() ?? "",
                    Convert.ToInt32(row["VehiculoId"]),
                    row["VehiculoNombre"]?.ToString() ?? "",
                    Convert.ToDouble(row["CombustibleEstimado"]),
                    Convert.ToDouble(row["CombustibleReal"]),
                    Convert.ToDouble(row["Desviacion"]),
                    Convert.ToDouble(row["PorcentajeDesviacion"]),
                    row["Motivo"]?.ToString() ?? ""
                ));
            }

            return resultado;
        }

        public async Task<List<(int VehiculoId, string VehiculoNombre, string VehiculoPlaca, string TipoVehiculo, double RatioEficiencia, int TotalViajes, double ConsumoPromedioReal, double AhorroCombustible)>> GetVehiculosMasEficientesAsync(int limite = 10)
        {
            // Solo consulta FuelDB - El enriquecimiento se hace en el servicio gRPC
            var query = $@"
                SELECT 
                    ar.VehiculoId,
                    '' as VehiculoNombre,
                    '' as VehiculoPlaca,
                    '' as TipoVehiculo,
                    (SUM(rc.CombustibleReal) / NULLIF(SUM(rc.CombustibleEstimado), 0)) as RatioEficiencia,
                    COUNT(rc.Id) as TotalViajes,
                    AVG(rc.CombustibleReal) as ConsumoPromedioReal,
                    (SUM(rc.CombustibleEstimado) - SUM(rc.CombustibleReal)) as AhorroCombustible
                FROM registrosconsumo rc
                INNER JOIN asignacionesrutas ar ON rc.AsignacionRutaId = ar.Id
                GROUP BY ar.VehiculoId
                HAVING TotalViajes >= 1
                ORDER BY RatioEficiencia ASC
                LIMIT {limite}";

            var dataTable = await _db.ExecuteQueryAsync(query);
            var resultado = new List<(int, string, string, string, double, int, double, double)>();

            foreach (DataRow row in dataTable.Rows)
            {
                resultado.Add((
                    Convert.ToInt32(row["VehiculoId"]),
                    row["VehiculoNombre"]?.ToString() ?? "",
                    row["VehiculoPlaca"]?.ToString() ?? "",
                    row["TipoVehiculo"]?.ToString() ?? "",
                    Convert.ToDouble(row["RatioEficiencia"]),
                    Convert.ToInt32(row["TotalViajes"]),
                    Convert.ToDouble(row["ConsumoPromedioReal"]),
                    Convert.ToDouble(row["AhorroCombustible"])
                ));
            }

            return resultado;
        }

        public async Task<List<(int VehiculoId, string VehiculoNombre, string VehiculoPlaca, string TipoVehiculo, double RatioEficiencia, int TotalViajes, double ConsumoPromedioReal, double AhorroCombustible)>> GetVehiculosMenosEficientesAsync(int limite = 10)
        {
            // Solo consulta FuelDB - El enriquecimiento se hace en el servicio gRPC
            var query = $@"
                SELECT 
                    ar.VehiculoId,
                    '' as VehiculoNombre,
                    '' as VehiculoPlaca,
                    '' as TipoVehiculo,
                    (SUM(rc.CombustibleReal) / NULLIF(SUM(rc.CombustibleEstimado), 0)) as RatioEficiencia,
                    COUNT(rc.Id) as TotalViajes,
                    AVG(rc.CombustibleReal) as ConsumoPromedioReal,
                    (SUM(rc.CombustibleEstimado) - SUM(rc.CombustibleReal)) as AhorroCombustible
                FROM registrosconsumo rc
                INNER JOIN asignacionesrutas ar ON rc.AsignacionRutaId = ar.Id
                GROUP BY ar.VehiculoId
                HAVING TotalViajes >= 1
                ORDER BY RatioEficiencia DESC
                LIMIT {limite}";

            var dataTable = await _db.ExecuteQueryAsync(query);
            var resultado = new List<(int, string, string, string, double, int, double, double)>();

            foreach (DataRow row in dataTable.Rows)
            {
                resultado.Add((
                    Convert.ToInt32(row["VehiculoId"]),
                    row["VehiculoNombre"]?.ToString() ?? "",
                    row["VehiculoPlaca"]?.ToString() ?? "",
                    row["TipoVehiculo"]?.ToString() ?? "",
                    Convert.ToDouble(row["RatioEficiencia"]),
                    Convert.ToInt32(row["TotalViajes"]),
                    Convert.ToDouble(row["ConsumoPromedioReal"]),
                    Convert.ToDouble(row["AhorroCombustible"])
                ));
            }

            return resultado;
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
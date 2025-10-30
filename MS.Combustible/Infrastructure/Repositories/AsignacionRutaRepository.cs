using MS.Combustible.Domain.Entities;
using MS.Combustible.Domain.Interfaces;
using MS.Combustible.Services;
using System.Collections.Generic;
using System.Data;

namespace MS.Combustible.Infrastructure.Repositories
{
    public class AsignacionRutaRepository : IAsignacionRutaRepository
    {
        private readonly IDatabaseService _db;

        public AsignacionRutaRepository(IDatabaseService db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(AsignacionRuta asignacion)
        {
            var query = @"INSERT INTO asignacionesrutas 
                (ChoferId, VehiculoId, RutaId, FechaAsignacion, CombustibleEstimado, EstadoId)
                VALUES (@ChoferId, @VehiculoId, @RutaId, @FechaAsignacion, @CombustibleEstimado, @EstadoId);
                SELECT LAST_INSERT_ID();";

            var parameters = new Dictionary<string, object>
            {
                { "@ChoferId", asignacion.ChoferId },
                { "@VehiculoId", asignacion.VehiculoId },
                { "@RutaId", asignacion.RutaId },
                { "@FechaAsignacion", asignacion.FechaAsignacion },
                { "@CombustibleEstimado", asignacion.CombustibleEstimado },
                { "@EstadoId", asignacion.EstadoId }
            };

            var result = await _db.ExecuteScalarAsync(query, parameters);
            return Convert.ToInt32(result);
        }

        public async Task<AsignacionRuta?> GetByIdAsync(int id)
        {
            var query = "SELECT Id, ChoferId, VehiculoId, RutaId, FechaAsignacion, CombustibleEstimado, EstadoId, FechaCreacion, FechaModificacion FROM asignacionesrutas WHERE Id = @Id LIMIT 1;";
            var parameters = new Dictionary<string, object> { { "@Id", id } };
            
            var dt = await _db.ExecuteQueryAsync(query, parameters);
            
            if (dt.Rows.Count == 0) return null;
            
            var row = dt.Rows[0];
            return new AsignacionRuta
            {
                Id = Convert.ToInt32(row["Id"]),
                ChoferId = Convert.ToInt32(row["ChoferId"]),
                VehiculoId = Convert.ToInt32(row["VehiculoId"]),
                RutaId = Convert.ToInt32(row["RutaId"]),
                FechaAsignacion = Convert.ToDateTime(row["FechaAsignacion"]),
                CombustibleEstimado = Convert.ToDouble(row["CombustibleEstimado"]),
                EstadoId = Convert.ToInt32(row["EstadoId"]),
                FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                FechaModificacion = Convert.ToDateTime(row["FechaModificacion"])
            };
        }

        public async Task<IEnumerable<AsignacionRuta>> GetAllAsync()
        {
            var dt = await _db.ExecuteQueryAsync("SELECT Id, ChoferId, VehiculoId, RutaId, FechaAsignacion, CombustibleEstimado, EstadoId, FechaCreacion, FechaModificacion FROM asignacionesrutas;");
            var list = new List<AsignacionRuta>();
            
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new AsignacionRuta
                {
                    Id = Convert.ToInt32(row["Id"]),
                    ChoferId = Convert.ToInt32(row["ChoferId"]),
                    VehiculoId = Convert.ToInt32(row["VehiculoId"]),
                    RutaId = Convert.ToInt32(row["RutaId"]),
                    FechaAsignacion = Convert.ToDateTime(row["FechaAsignacion"]),
                    CombustibleEstimado = Convert.ToDouble(row["CombustibleEstimado"]),
                    EstadoId = Convert.ToInt32(row["EstadoId"]),
                    FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                    FechaModificacion = Convert.ToDateTime(row["FechaModificacion"])
                });
            }
            return list;
        }

        public async Task<IEnumerable<AsignacionRuta>> GetByChoferIdAsync(int choferId)
        {
            var query = "SELECT Id, ChoferId, VehiculoId, RutaId, FechaAsignacion, CombustibleEstimado, EstadoId, FechaCreacion, FechaModificacion FROM asignacionesrutas WHERE ChoferId = @ChoferId;";
            var parameters = new Dictionary<string, object> { { "@ChoferId", choferId } };
            
            var dt = await _db.ExecuteQueryAsync(query, parameters);
            var list = new List<AsignacionRuta>();
            
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapRowToEntity(row));
            }
            return list;
        }

        public async Task<IEnumerable<AsignacionRuta>> GetByVehiculoIdAsync(int vehiculoId)
        {
            var query = "SELECT Id, ChoferId, VehiculoId, RutaId, FechaAsignacion, CombustibleEstimado, EstadoId, FechaCreacion, FechaModificacion FROM asignacionesrutas WHERE VehiculoId = @VehiculoId;";
            var parameters = new Dictionary<string, object> { { "@VehiculoId", vehiculoId } };
            
            var dt = await _db.ExecuteQueryAsync(query, parameters);
            var list = new List<AsignacionRuta>();
            
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapRowToEntity(row));
            }
            return list;
        }

        public async Task<IEnumerable<AsignacionRuta>> GetByRutaIdAsync(int rutaId)
        {
            var query = "SELECT Id, ChoferId, VehiculoId, RutaId, FechaAsignacion, CombustibleEstimado, EstadoId, FechaCreacion, FechaModificacion FROM asignacionesrutas WHERE RutaId = @RutaId;";
            var parameters = new Dictionary<string, object> { { "@RutaId", rutaId } };
            
            var dt = await _db.ExecuteQueryAsync(query, parameters);
            var list = new List<AsignacionRuta>();
            
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapRowToEntity(row));
            }
            return list;
        }

        public async Task<IEnumerable<AsignacionRuta>> GetByEstadoIdAsync(int estadoId)
        {
            var query = "SELECT Id, ChoferId, VehiculoId, RutaId, FechaAsignacion, CombustibleEstimado, EstadoId, FechaCreacion, FechaModificacion FROM asignacionesrutas WHERE EstadoId = @EstadoId;";
            var parameters = new Dictionary<string, object> { { "@EstadoId", estadoId } };
            
            var dt = await _db.ExecuteQueryAsync(query, parameters);
            var list = new List<AsignacionRuta>();
            
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapRowToEntity(row));
            }
            return list;
        }

        public async Task<int> UpdateAsync(AsignacionRuta asignacion)
        {
            var query = @"UPDATE asignacionesrutas 
                SET ChoferId = @ChoferId,
                    VehiculoId = @VehiculoId,
                    RutaId = @RutaId,
                    FechaAsignacion = @FechaAsignacion,
                    CombustibleEstimado = @CombustibleEstimado,
                    EstadoId = @EstadoId,
                    FechaModificacion = CURRENT_TIMESTAMP
                WHERE Id = @Id;";

            var parameters = new Dictionary<string, object>
            {
                { "@Id", asignacion.Id },
                { "@ChoferId", asignacion.ChoferId },
                { "@VehiculoId", asignacion.VehiculoId },
                { "@RutaId", asignacion.RutaId },
                { "@FechaAsignacion", asignacion.FechaAsignacion },
                { "@CombustibleEstimado", asignacion.CombustibleEstimado },
                { "@EstadoId", asignacion.EstadoId }
            };

            return await _db.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<int> UpdateEstadoAsync(int id, int estadoId)
        {
            var query = @"UPDATE asignacionesrutas 
                SET EstadoId = @EstadoId, 
                    FechaModificacion = CURRENT_TIMESTAMP 
                WHERE Id = @Id;";

            var parameters = new Dictionary<string, object>
            {
                { "@Id", id },
                { "@EstadoId", estadoId }
            };

            return await _db.ExecuteNonQueryAsync(query, parameters);
        }

        private AsignacionRuta MapRowToEntity(DataRow row)
        {
            return new AsignacionRuta
            {
                Id = Convert.ToInt32(row["Id"]),
                ChoferId = Convert.ToInt32(row["ChoferId"]),
                VehiculoId = Convert.ToInt32(row["VehiculoId"]),
                RutaId = Convert.ToInt32(row["RutaId"]),
                FechaAsignacion = Convert.ToDateTime(row["FechaAsignacion"]),
                CombustibleEstimado = Convert.ToDouble(row["CombustibleEstimado"]),
                EstadoId = Convert.ToInt32(row["EstadoId"]),
                FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                FechaModificacion = Convert.ToDateTime(row["FechaModificacion"])
            };
        }
    }
}

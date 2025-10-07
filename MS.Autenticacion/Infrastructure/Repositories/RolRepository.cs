using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System.Data;
using MS.Autenticacion.Domain.Entities;
using MS.Autenticacion.Domain.Interfaces;

namespace MS.Autenticacion.Infrastructure.Repositories
{
    public class RolRepository : IRolRepository
    {
        private readonly IDatabaseConnection _dbConnection;
        private readonly ILogger<RolRepository> _logger;

        public RolRepository(IDatabaseConnection dbConnection, ILogger<RolRepository> logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public async Task<Rol?> GetByIdAsync(int id)
        {
            try
            {
                var query = "SELECT * FROM roles WHERE Id = @id";
                var parameters = new[] { new MySqlParameter("@id", id) };
                var dataTable = await _dbConnection.ExecuteQueryAsync(query, parameters);

                if (dataTable.Rows.Count == 0) return null;

                return MapDataRowToRol(dataTable.Rows[0]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener rol por ID: {RolId}", id);
                return null;
            }
        }

        public async Task<List<Rol>> GetAllAsync()
        {
            try
            {
                var query = "SELECT * FROM roles ORDER BY Nombre";
                var dataTable = await _dbConnection.ExecuteQueryAsync(query);

                var roles = new List<Rol>();
                foreach (DataRow row in dataTable.Rows)
                {
                    roles.Add(MapDataRowToRol(row));
                }

                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los roles");
                return new List<Rol>();
            }
        }

        private Rol MapDataRowToRol(DataRow row)
        {
            return new Rol
            {
                Id = Convert.ToInt32(row["Id"]),
                Nombre = row["Nombre"]?.ToString() ?? string.Empty,
                Descripcion = row["Descripcion"]?.ToString(),
                Estado = Convert.ToBoolean(row["Estado"]),
                FechaCreacion = Convert.ToDateTime(row["FechaCreacion"])
                // FechaModificacion no existe en la tabla Roles según el schema
            };
        }
    }
}
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System.Data;
using MS.Autenticacion.Domain.Entities;
using MS.Autenticacion.Domain.Interfaces;

namespace MS.Autenticacion.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDatabaseConnection _dbConnection;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(IDatabaseConnection dbConnection, ILogger<UserRepository> logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public async Task<User?> GetByNombreUsuarioAsync(string nombreUsuario)
        {
            try
            {
                var query = "SELECT * FROM usuarios WHERE NombreUsuario = @nombreUsuario";
                var parameters = new[] { new MySqlParameter("@nombreUsuario", nombreUsuario) };
                var dataTable = await _dbConnection.ExecuteQueryAsync(query, parameters);

                if (dataTable.Rows.Count == 0) return null;

                return MapDataRowToUser(dataTable.Rows[0]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por NombreUsuario: {NombreUsuario}", nombreUsuario);
                return null;
            }
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            try
            {
                var query = "SELECT * FROM usuarios WHERE Email = @email";
                var parameters = new[] { new MySqlParameter("@email", email) };
                var dataTable = await _dbConnection.ExecuteQueryAsync(query, parameters);

                if (dataTable.Rows.Count == 0) return null;

                return MapDataRowToUser(dataTable.Rows[0]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por Email: {Email}", email);
                return null;
            }
        }

        public async Task UpdateUltimoAccesoAsync(int userId, DateTime ultimoAcceso)
        {
            try
            {
                var query = "UPDATE usuarios SET UltimoAcceso = @ultimoAcceso, FechaModificacion = @fechaModificacion WHERE Id = @userId";
                var parameters = new[]
                {
                    new MySqlParameter("@ultimoAcceso", ultimoAcceso),
                    new MySqlParameter("@fechaModificacion", DateTime.UtcNow),
                    new MySqlParameter("@userId", userId)
                };

                await _dbConnection.ExecuteNonQueryAsync(query, parameters);
                _logger.LogInformation("UltimoAcceso actualizado para UserId: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar UltimoAcceso para UserId: {UserId}", userId);
                throw;
            }
        }

        private User MapDataRowToUser(DataRow row)
        {
            return new User
            {
                Id = Convert.ToInt32(row["Id"]),
                Email = row["Email"]?.ToString() ?? string.Empty,
                NombreUsuario = row["NombreUsuario"]?.ToString() ?? string.Empty,
                PasswordHash = row["PasswordHash"]?.ToString() ?? string.Empty,
                RolId = Convert.ToInt32(row["RolId"]),
                Estado = Convert.ToInt32(row["Estado"]),
                FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                FechaModificacion = row["FechaModificacion"] != DBNull.Value 
                    ? (DateTime?)Convert.ToDateTime(row["FechaModificacion"]) 
                    : null,
                UltimoAcceso = row["UltimoAcceso"] != DBNull.Value 
                    ? (DateTime?)Convert.ToDateTime(row["UltimoAcceso"]) 
                    : null
            };
        }
    }
}
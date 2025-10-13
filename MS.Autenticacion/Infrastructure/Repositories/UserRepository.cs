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

        public async Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash)
        {
            try
            {
                var query = "UPDATE usuarios SET PasswordHash = @passwordHash, FechaModificacion = @fechaModificacion WHERE Id = @userId";
                var parameters = new[]
                {
                    new MySqlParameter("@passwordHash", newPasswordHash),
                    new MySqlParameter("@fechaModificacion", DateTime.UtcNow),
                    new MySqlParameter("@userId", userId)
                };

                var affectedRows = await _dbConnection.ExecuteNonQueryAsync(query, parameters);
                _logger.LogInformation("Contraseña actualizada para UserId: {UserId}", userId);
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar contraseña para UserId: {UserId}", userId);
                return false;
            }
        }

        public async Task<int> CreateAsync(User user)
        {
            try
            {
                var query = @"INSERT INTO usuarios (Email, NombreUsuario, PasswordHash, RolId, Estado, FechaCreacion) 
                             VALUES (@email, @nombreUsuario, @passwordHash, @rolId, @estado, @fechaCreacion);
                             SELECT LAST_INSERT_ID();";
                
                var parameters = new[]
                {
                    new MySqlParameter("@email", user.Email),
                    new MySqlParameter("@nombreUsuario", user.NombreUsuario),
                    new MySqlParameter("@passwordHash", user.PasswordHash),
                    new MySqlParameter("@rolId", user.RolId),
                    new MySqlParameter("@estado", user.Estado),
                    new MySqlParameter("@fechaCreacion", user.FechaCreacion)
                };

                var result = await _dbConnection.ExecuteScalarAsync(query, parameters);
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario: {NombreUsuario}", user.NombreUsuario);
                throw;
            }
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            try
            {
                var query = "SELECT * FROM usuarios WHERE Id = @id";
                var parameters = new[] { new MySqlParameter("@id", id) };
                var dataTable = await _dbConnection.ExecuteQueryAsync(query, parameters);

                if (dataTable.Rows.Count == 0) return null;

                return MapDataRowToUser(dataTable.Rows[0]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por ID: {Id}", id);
                return null;
            }
        }

        public async Task<List<User>> GetAllAsync()
        {
            try
            {
                var query = "SELECT * FROM usuarios ORDER BY FechaCreacion DESC";
                var dataTable = await _dbConnection.ExecuteQueryAsync(query, Array.Empty<MySqlParameter>());

                var users = new List<User>();
                foreach (DataRow row in dataTable.Rows)
                {
                    users.Add(MapDataRowToUser(row));
                }

                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los usuarios");
                return new List<User>();
            }
        }

        public async Task<int> UpdateAsync(User user)
        {
            try
            {
                var query = @"UPDATE usuarios 
                             SET Email = @email, 
                                 NombreUsuario = @nombreUsuario, 
                                 RolId = @rolId, 
                                 FechaModificacion = @fechaModificacion
                             WHERE Id = @id";
                
                var parameters = new[]
                {
                    new MySqlParameter("@id", user.Id),
                    new MySqlParameter("@email", user.Email),
                    new MySqlParameter("@nombreUsuario", user.NombreUsuario),
                    new MySqlParameter("@rolId", user.RolId),
                    new MySqlParameter("@fechaModificacion", DateTime.UtcNow)
                };

                return await _dbConnection.ExecuteNonQueryAsync(query, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario: {Id}", user.Id);
                throw;
            }
        }

        public async Task<int> UpdateEstadoAsync(int userId, int estado)
        {
            try
            {
                var query = @"UPDATE usuarios 
                             SET Estado = @estado, 
                                 FechaModificacion = @fechaModificacion
                             WHERE Id = @id";
                
                var parameters = new[]
                {
                    new MySqlParameter("@id", userId),
                    new MySqlParameter("@estado", estado),
                    new MySqlParameter("@fechaModificacion", DateTime.UtcNow)
                };

                return await _dbConnection.ExecuteNonQueryAsync(query, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado del usuario: {Id}", userId);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                var query = "SELECT COUNT(*) FROM usuarios WHERE Id = @id";
                var parameters = new[] { new MySqlParameter("@id", id) };
                var result = await _dbConnection.ExecuteScalarAsync(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia del usuario: {Id}", id);
                return false;
            }
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            try
            {
                var query = "SELECT COUNT(*) FROM usuarios WHERE NombreUsuario = @username";
                var parameters = new[] { new MySqlParameter("@username", username) };
                var result = await _dbConnection.ExecuteScalarAsync(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia del username: {Username}", username);
                return false;
            }
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            try
            {
                var query = "SELECT COUNT(*) FROM usuarios WHERE Email = @email";
                var parameters = new[] { new MySqlParameter("@email", email) };
                var result = await _dbConnection.ExecuteScalarAsync(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia del email: {Email}", email);
                return false;
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
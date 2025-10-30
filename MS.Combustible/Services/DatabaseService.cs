using MySql.Data.MySqlClient;
using System.Data;

namespace MS.Combustible.Services
{
    public interface IDatabaseService
    {
        Task<bool> TestConnectionAsync();
        Task<DataTable> ExecuteQueryAsync(string query, Dictionary<string, object>? parameters = null);
        Task<object?> ExecuteScalarAsync(string query, Dictionary<string, object>? parameters = null);
        Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object>? parameters = null);
    }

    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(ILogger<DatabaseService> logger)
        {
            _logger = logger;
            
            // Leer variables específicas de FuelDB del .env
            var host = Environment.GetEnvironmentVariable("FUEL_DB_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("FUEL_DB_PORT") ?? "3307";
            var database = Environment.GetEnvironmentVariable("FUEL_DB_NAME") ?? "FuelDB";
            var user = Environment.GetEnvironmentVariable("FUEL_DB_USER") ?? "root";
            var password = Environment.GetEnvironmentVariable("FUEL_DB_PASS") ?? "root";

            _connectionString = $"Server={host};Port={port};Database={database};Uid={user};Pwd={password};";
            
            _logger.LogInformation($"Configuración FuelDB: Host={host}, Port={port}, Database={database}, User={user}");
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                _logger.LogInformation("Conexión a FuelDB exitosa");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al conectar con FuelDB");
                return false;
            }
        }

        public async Task<DataTable> ExecuteQueryAsync(string query, Dictionary<string, object>? parameters = null)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                
                using var command = new MySqlCommand(query, connection);
                
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }
                
                using var adapter = new MySqlDataAdapter(command);
                
                var dataTable = new DataTable();
                adapter.Fill(dataTable);
                
                _logger.LogInformation($"Query ejecutado exitosamente en FuelDB: {query}");
                return dataTable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al ejecutar query en FuelDB: {query}");
                throw;
            }
        }

        public async Task<object?> ExecuteScalarAsync(string query, Dictionary<string, object>? parameters = null)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand(query, connection);
                if (parameters != null)
                {
                    foreach (var kv in parameters)
                    {
                        command.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);
                    }
                }

                var result = await command.ExecuteScalarAsync();
                _logger.LogInformation($"ExecuteScalar ejecutado en FuelDB: {query}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en ExecuteScalar en FuelDB: {query}");
                throw;
            }
        }

        public async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object>? parameters = null)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand(query, connection);
                if (parameters != null)
                {
                    foreach (var kv in parameters)
                    {
                        command.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);
                    }
                }

                var affected = await command.ExecuteNonQueryAsync();
                _logger.LogInformation($"ExecuteNonQuery ejecutado en FuelDB: {query}");
                return affected;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en ExecuteNonQuery en FuelDB: {query}");
                throw;
            }
        }
    }
}
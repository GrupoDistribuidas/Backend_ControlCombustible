using MySql.Data.MySqlClient;
using System.Data;

namespace MS.Vehiculos.Services
{
    public interface IDatabaseService
    {
        Task<bool> TestConnectionAsync();
        Task<DataTable> ExecuteQueryAsync(string query);
        Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object>? parameters = null);
        Task<object?> ExecuteScalarAsync(string query, Dictionary<string, object>? parameters = null);
    }

    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(ILogger<DatabaseService> logger)
        {
            _logger = logger;

            // Leer variables específicas de VehiclesDB del .env
            var host = Environment.GetEnvironmentVariable("VEHICLES_DB_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("VEHICLES_DB_PORT") ?? "3307";
            var database = Environment.GetEnvironmentVariable("VEHICLES_DB_NAME") ?? "VehiclesDB";
            var user = Environment.GetEnvironmentVariable("VEHICLES_DB_USER") ?? "root";
            var password = Environment.GetEnvironmentVariable("VEHICLES_DB_PASS") ?? "root";

            _connectionString = $"Server={host};Port={port};Database={database};Uid={user};Pwd={password};";

            _logger.LogInformation($"Configuración VehiclesDB: Host={host}, Port={port}, Database={database}");
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                _logger.LogInformation("Conexión a VehiclesDB exitosa");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al conectar con VehiclesDB");
                return false;
            }
        }

        public async Task<DataTable> ExecuteQueryAsync(string query)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand(query, connection);
                using var adapter = new MySqlDataAdapter(command);

                var dataTable = new DataTable();
                adapter.Fill(dataTable);

                _logger.LogInformation($"Query ejecutado exitosamente en VehiclesDB: {query}");
                return dataTable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al ejecutar query en VehiclesDB: {query}");
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
                _logger.LogInformation($"ExecuteNonQuery ejecutado. Query: {query}. Affected: {affected}");
                return affected;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en ExecuteNonQuery. Query: {query}");
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
                _logger.LogInformation($"ExecuteScalar ejecutado. Query: {query}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en ExecuteScalar. Query: {query}");
                throw;
            }
        }
    }
}
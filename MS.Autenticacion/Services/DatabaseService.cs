using MySql.Data.MySqlClient;
using System.Data;

namespace MS.Autenticacion.Services
{
    public interface IDatabaseService
    {
        Task<bool> TestConnectionAsync();
        Task<DataTable> ExecuteQueryAsync(string query);
    }

    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(ILogger<DatabaseService> logger)
        {
            _logger = logger;
            
            // Leer variables específicas de AuthDB del .env
            var host = Environment.GetEnvironmentVariable("AUTH_DB_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("AUTH_DB_PORT") ?? "3306";
            var database = Environment.GetEnvironmentVariable("AUTH_DB_NAME") ?? "AuthDB";
            var user = Environment.GetEnvironmentVariable("AUTH_DB_USER") ?? "root";
            var password = Environment.GetEnvironmentVariable("AUTH_DB_PASS") ?? "root";

            _connectionString = $"Server={host};Port={port};Database={database};Uid={user};Pwd={password};";
            
            _logger.LogInformation($"Configuración AuthDB: Host={host}, Port={port}, Database={database}, User={user}");
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                _logger.LogInformation("Conexión a la base de datos exitosa");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al conectar con la base de datos");
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
                
                _logger.LogInformation($"Query ejecutado exitosamente: {query}");
                return dataTable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al ejecutar query: {query}");
                throw;
            }
        }
    }
}
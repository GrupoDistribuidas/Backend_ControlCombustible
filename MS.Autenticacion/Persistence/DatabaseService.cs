using MySql.Data.MySqlClient;
using System.Data;
using Microsoft.Extensions.Logging;
using MS.Autenticacion.Domain.Interfaces;

namespace MS.Autenticacion.Persistence
{
    public class DatabaseService : IDatabaseConnection
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(ILogger<DatabaseService> logger)
        {
            _logger = logger;

            // Leer directamente de variables de entorno (cargadas por DotNetEnv en Program.cs)
            var host = Environment.GetEnvironmentVariable("AUTH_DB_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("AUTH_DB_PORT") ?? "3306";
            var database = Environment.GetEnvironmentVariable("AUTH_DB_NAME") ?? "AuthDB";
            var user = Environment.GetEnvironmentVariable("AUTH_DB_USER") ?? "root";
            var password = Environment.GetEnvironmentVariable("AUTH_DB_PASS") ?? "root";

            _connectionString = $"Server={host};Port={port};Database={database};Uid={user};Pwd={password};";

            _logger.LogInformation("DatabaseService inicializada: Host={Host}, Port={Port}, Database={Database}", host, port, database);
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

        public async Task<DataTable> ExecuteQueryAsync(string query, MySqlParameter[]? parameters = null)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand(query, connection);
                
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                using var adapter = new MySqlDataAdapter(command);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);

                _logger.LogInformation("Query ejecutado exitosamente: {Query}", query);
                return dataTable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al ejecutar query: {Query}", query);
                throw;
            }
        }

        public async Task<int> ExecuteNonQueryAsync(string query, MySqlParameter[]? parameters = null)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand(query, connection);
                
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                var affectedRows = await command.ExecuteNonQueryAsync();
                _logger.LogInformation("Query de actualización ejecutado exitosamente: {Query}, Filas afectadas: {AffectedRows}", query, affectedRows);
                return affectedRows;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al ejecutar query de actualización: {Query}", query);
                throw;
            }
        }
    }
}
using System.Data;
using MySql.Data.MySqlClient;

namespace MS.Autenticacion.Domain.Interfaces
{
    public interface IDatabaseConnection
    {
        Task<bool> TestConnectionAsync();
        Task<DataTable> ExecuteQueryAsync(string query, MySqlParameter[]? parameters = null);
        Task<int> ExecuteNonQueryAsync(string query, MySqlParameter[]? parameters = null);
    }
}
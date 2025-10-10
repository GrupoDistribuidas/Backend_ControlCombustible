using System.Data;
using MySql.Data.MySqlClient;

namespace MS.Autenticacion.Domain.Interfaces
{
       public interface IEmailService
    {
        Task<bool> SendPasswordByEmailAsync(string toEmail, string recipientName, string username, string password);
    }

}
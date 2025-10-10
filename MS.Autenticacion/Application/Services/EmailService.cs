using MS.Autenticacion.Domain.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;
using System.Text;

namespace MS.Autenticacion.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService()
        {
            // Cargar configuración desde variables de entorno
            _smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? throw new InvalidOperationException("SMTP_HOST not configured");
            _smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
            _smtpUsername = Environment.GetEnvironmentVariable("SMTP_USERNAME") ?? throw new InvalidOperationException("SMTP_USERNAME not configured");
            _smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? throw new InvalidOperationException("SMTP_PASSWORD not configured");
            _fromEmail = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL") ?? throw new InvalidOperationException("SMTP_FROM_EMAIL not configured");
            _fromName = Environment.GetEnvironmentVariable("SMTP_FROM_NAME") ?? "Sistema Control Combustible";
        }

        public async Task<bool> SendPasswordByEmailAsync(string toEmail, string recipientName, string username, string password)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_fromName, _fromEmail));
                message.To.Add(new MailboxAddress(recipientName, toEmail));
                message.Subject = "Contraseña Temporal - Control Combustible";

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = CreateEmailTemplate(recipientName, username, password);
                bodyBuilder.TextBody = CreatePlainTextEmail(recipientName, username, password);
                
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                
                // Conectar al servidor SMTP
                await client.ConnectAsync(_smtpHost, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                
                // Autenticarse
                await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
                
                // Enviar el mensaje
                await client.SendAsync(message);
                
                // Desconectar
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                // En un entorno de producción, loguear la excepción
                Console.WriteLine($"Error enviando email: {ex.Message}");
                return false;
            }
        }

        private string CreateEmailTemplate(string recipientName, string username, string temporaryPassword)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Contraseña Temporal</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .password-box {{ background-color: #e9ecef; padding: 15px; border-left: 4px solid #007bff; margin: 20px 0; }}
        .warning {{ color: #dc3545; font-weight: bold; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Control Combustible System</h1>
        </div>
        <div class='content'>
            <h2>Hola {recipientName},</h2>
            <p>Se ha generado una contraseña temporal para tu usuario <strong>{username}</strong>.</p>
            
            <div class='password-box'>
                <strong>Contraseña Temporal: {temporaryPassword}</strong>
            </div>
            
            <p class='warning'>⚠️ IMPORTANTE:</p>
            <ul>
                <li>Esta contraseña es temporal y debe ser cambiada después del primer inicio de sesión</li>
                <li>Por seguridad, no compartas esta contraseña con nadie</li>
                <li>Si no solicitaste este cambio, contacta al administrador del sistema inmediatamente</li>
            </ul>
            
            <p>Para iniciar sesión:</p>
            <ol>
                <li>Ve al sistema de Control Combustible</li>
                <li>Ingresa tu usuario y esta contraseña temporal</li>
                <li>Cambia tu contraseña por una nueva y segura</li>
            </ol>
        </div>
        <div class='footer'>
            <p>Este es un mensaje automático, por favor no responder a este email.</p>
            <p>&copy; 2024 Control Combustible System</p>
        </div>
    </div>
</body>
</html>";
        }

        private string CreatePlainTextEmail(string recipientName, string username, string temporaryPassword)
        {
            var sb = new StringBuilder();
            sb.AppendLine("CONTROL COMBUSTIBLE SYSTEM");
            sb.AppendLine("=========================");
            sb.AppendLine();
            sb.AppendLine($"Hola {recipientName},");
            sb.AppendLine();
            sb.AppendLine($"Se ha generado una contraseña temporal para tu usuario {username}.");
            sb.AppendLine();
            sb.AppendLine($"Contraseña Temporal: {temporaryPassword}");
            sb.AppendLine();
            sb.AppendLine("IMPORTANTE:");
            sb.AppendLine("- Esta contraseña es temporal y debe ser cambiada después del primer inicio de sesión");
            sb.AppendLine("- Por seguridad, no compartas esta contraseña con nadie");
            sb.AppendLine("- Si no solicitaste este cambio, contacta al administrador del sistema inmediatamente");
            sb.AppendLine();
            sb.AppendLine("Para iniciar sesión:");
            sb.AppendLine("1. Ve al sistema de Control Combustible");
            sb.AppendLine("2. Ingresa tu usuario y esta contraseña temporal");
            sb.AppendLine("3. Cambia tu contraseña por una nueva y segura");
            sb.AppendLine();
            sb.AppendLine("Este es un mensaje automático, por favor no responder a este email.");
            sb.AppendLine("© 2024 Control Combustible System");
            
            return sb.ToString();
        }
    }
}
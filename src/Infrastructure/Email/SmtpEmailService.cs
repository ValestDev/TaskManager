using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Interfaces;

namespace TaskManager.Infrastructure.Email;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAdminCredentialsAsync(string toEmail, string password)
    {
        var host = _config["Smtp:Host"] ?? "localhost";
        var port = int.Parse(_config["Smtp:Port"] ?? "1025");
        var from = _config["Smtp:From"] ?? "no-reply@taskmanager.local";
        var user = _config["Smtp:User"];
        var pass = _config["Smtp:Password"];

        try
        {
            using var client = new SmtpClient(host, port);

            if (!string.IsNullOrEmpty(user))
            {
                client.Credentials = new NetworkCredential(user, pass);
            }

            var message = new MailMessage(from, toEmail)
            {
                Subject = "Credenciales de administrador - Task Manager",
                Body = $"Se ha creado el usuario administrador inicial.\n\n" +
                       $"Email: {toEmail}\n" +
                       $"Password: {password}\n\n" +
                       $"Por seguridad, cambia esta contraseña después de iniciar sesión."
            };

            await client.SendMailAsync(message);
            _logger.LogInformation("Credenciales de administrador enviadas a {Email}", toEmail);
        }
        catch (Exception ex)
        {
            // No queremos que un fallo de correo tumbe el arranque de la aplicación.
            _logger.LogError(ex, "No se pudo enviar el correo con las credenciales de administrador.");
        }
    }
}
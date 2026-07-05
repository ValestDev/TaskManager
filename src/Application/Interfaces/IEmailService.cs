namespace TaskManager.Application.Interfaces;

public interface IEmailService
{
    Task SendAdminCredentialsAsync(string toEmail, string password);
}
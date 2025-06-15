using AutoRegister;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MimeKit;
using Polly;
using Policy = Polly.Policy;

namespace application.services;

[Register(ServiceLifetime.Scoped)]
public class EmailService(IConfiguration config, ILogger<EmailService> logger)
{
    public async Task SendPasswordResetEmail(string email, string token)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Twitter Clone", config["Email:From"]));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = "Restablecer contraseña - Twitter Clone";

        var resetUrl = $"{config["Frontend:BaseUrl"]}/reset-password?token={token}";
        var body = $"""
            <h1>Restablecer contraseña</h1>
            <p>Haz clic en el siguiente enlace para restablecer tu contraseña:</p>
            <a href="{resetUrl}">{resetUrl}</a>
            <p>Este enlace expirará en 1 hora.</p>
            """;

        message.Body = new TextPart("html") { Text = body };

        var policy = Policy.Handle<Exception>()
            .WaitAndRetryAsync(3, retry => TimeSpan.FromSeconds(Math.Pow(2, retry)));

        await policy.ExecuteAsync(async () =>
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(
                config["Email:SmtpHost"],
                int.Parse(config.GetSection("Email:SmtpPort").Value),
                bool.Parse(config.GetSection("Email:EnableSsl").Value) 
                    ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

            await client.AuthenticateAsync(
                config["Email:SmtpUser"],
                config["Email:SmtpPass"]);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            logger.LogInformation("Email de restablecimiento enviado a {Email}", email);
        });
    }
}
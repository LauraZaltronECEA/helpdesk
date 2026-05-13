using api.models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using MimeKit;

namespace api.helpdesk.Services;

public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}

public class EmailSenderService : IEmailSender<AppUser>
{
    private readonly SmtpSettings _settings;

    public EmailSenderService(IConfiguration configuration)
    {
        _settings = configuration.GetSection("Smtp").Get<SmtpSettings>()
            ?? throw new InvalidOperationException("SMTP configuration section 'Smtp' is missing.");
    }

    public async Task SendConfirmationLinkAsync(AppUser user, string email, string confirmationLink)
    {
        var body = $@"
<html>
<body>
<h2>Bienvenido, {user.Fullname}!</h2>
<p>Gracias por registrarte. Por favor confirma tu correo electronico haciendo clic en el siguiente enlace:</p>
<p><a href='{confirmationLink}'>Confirmar correo electronico</a></p>
<p>O copia y pega esta URL en tu navegador:</p>
<p>{confirmationLink}</p>
</body>
</html>";

        await SendEmailAsync(email, "Confirma tu correo electronico", body);
    }

    public async Task SendPasswordResetLinkAsync(AppUser user, string email, string resetLink)
    {
        var body = $@"
<html>
<body>
<h2>Restablece tu contrasena</h2>
<p>Hola, {user.Fullname}. Haz clic en el siguiente enlace para restablecer tu contrasena:</p>
<p><a href='{resetLink}'>Restablecer contrasena</a></p>
<p>Si no solicitaste este cambio, ignora este mensaje.</p>
</body>
</html>";

        await SendEmailAsync(email, "Restablece tu contrasena", body);
    }

    public async Task SendPasswordResetCodeAsync(AppUser user, string email, string resetCode)
    {
        var body = $@"
<html>
<body>
<h2>Tu codigo de verificacion</h2>
<p>Hola, {user.Fullname}. Usa el siguiente codigo para restablecer tu contrasena:</p>
<p><strong>{resetCode}</strong></p>
</body>
</html>";

        await SendEmailAsync(email, "Codigo de verificacion", body);
    }

    private async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        message.To.Add(new MailboxAddress("", email));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlMessage
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.Host, _settings.Port, _settings.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable);

        if (!string.IsNullOrEmpty(_settings.Username))
        {
            await client.AuthenticateAsync(_settings.Username, _settings.Password);
        }

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}

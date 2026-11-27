using System.Net;
using System.Net.Mail;
using MagnaTaller.Web.Services.Interfaces;

namespace MagnaTaller.Web.Services.Implementations
{
    // Servicio de correo real mediante SMTP leyendo credenciales desde User Secrets
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task EnviarCorreoAsync(string destinatario, string asunto, string mensajeHtml)
        {
            // Lectura de credenciales SMTP desde User Secrets
            var servidor = _configuration["SmtpSettings:Servidor"] ?? "smtp.gmail.com";
            var puerto = int.Parse(_configuration["SmtpSettings:Puerto"] ?? "587");
            var usuario = _configuration["SmtpSettings:Usuario"];
            var password = _configuration["SmtpSettings:Password"];
            var remitente = _configuration["SmtpSettings:Remitente"] ?? usuario;

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("Las credenciales SMTP no están configuradas en User Secrets.");
            }

            // Configuración del cliente SMTP con cifrado SSL/TLS
            using var cliente = new SmtpClient(servidor, puerto)
            {
                Credentials = new NetworkCredential(usuario, password),
                EnableSsl = true
            };

            using var mensaje = new MailMessage
            {
                From = new MailAddress(remitente!, "MagnaTaller Seguridad"),
                Subject = asunto,
                Body = mensajeHtml,
                IsBodyHtml = true
            };

            mensaje.To.Add(destinatario);

            await cliente.SendMailAsync(mensaje);
        }
    }
}

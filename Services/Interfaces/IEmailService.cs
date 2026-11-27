namespace MagnaTaller.Web.Services.Interfaces
{
    // Contrato para el servicio de envío de correos electrónicos
    public interface IEmailService
    {
        Task EnviarCorreoAsync(string destinatario, string asunto, string mensajeHtml);
    }
}

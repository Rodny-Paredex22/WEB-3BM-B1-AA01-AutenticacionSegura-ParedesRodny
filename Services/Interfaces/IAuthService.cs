using MagnaTaller.Web.Models.ViewModels;

namespace MagnaTaller.Web.Services.Interfaces
{
    // Contrato de la Capa de Negocio para autenticación, perfil y administración
    public interface IAuthService
    {
        Task<(bool Exito, string? Error)> RegistrarClienteAsync(RegistroViewModel modelo);
        Task<(bool Exito, string? Error)> IniciarSesionAsync(LoginViewModel modelo);
        Task CerrarSesionAsync();
        Task<(bool Exito, string? Token)> GenerarTokenRecuperacionAsync(string email);
        Task EnviarCorreoRecuperacionAsync(string email, string enlace);
        Task<(bool Exito, string? Error)> RestablecerContrasenaAsync(RestablecerContrasenaViewModel modelo);
        Task<PerfilViewModel?> ObtenerPerfilAsync(string userId);
        Task<(bool Exito, string? Error)> CambiarContrasenaAsync(string userId, CambiarContrasenaViewModel modelo);
        Task<List<UsuarioAdminViewModel>> ListarUsuariosAsync();
        Task<(bool Exito, string? Error)> CambiarRolAsync(string usuarioId, string nuevoRol);
        Task<(bool Exito, string? Error)> CambiarEstadoAsync(string usuarioId, bool activo);
    }
}

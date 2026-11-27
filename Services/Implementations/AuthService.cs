using MagnaTaller.Web.Data.Entities;
using MagnaTaller.Web.Models.ViewModels;
using MagnaTaller.Web.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MagnaTaller.Web.Services.Implementations
{
    // Capa de Negocio: reglas de seguridad, bloqueo, tokens y control de roles
    public class AuthService : IAuthService
    {
        private readonly UserManager<UsuarioMagna> _userManager;
        private readonly SignInManager<UsuarioMagna> _signInManager;
        private readonly IEmailService _emailService;

        public AuthService(
            UserManager<UsuarioMagna> userManager,
            SignInManager<UsuarioMagna> signInManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        // Registro público: siempre asigna el rol Cliente y cuenta activa
        public async Task<(bool Exito, string? Error)> RegistrarClienteAsync(RegistroViewModel modelo)
        {
            var existe = await _userManager.FindByEmailAsync(modelo.Email);
            if (existe != null)
            {
                return (false, "El correo electrónico ya está registrado.");
            }

            var usuario = new UsuarioMagna
            {
                UserName = modelo.Email,
                Email = modelo.Email,
                NombreCompleto = modelo.NombreCompleto,
                Activo = true,
                EmailConfirmed = true
            };

            // UserManager cifra automáticamente la contraseña usando PBKDF2 con sal
            var resultado = await _userManager.CreateAsync(usuario, modelo.Password);
            if (!resultado.Succeeded)
            {
                return (false, string.Join(" ", resultado.Errors.Select(e => e.Description)));
            }

            await _userManager.AddToRoleAsync(usuario, "Cliente");
            return (true, null);
        }

        // Inicio de sesión: valida estado activo, bloqueo por intentos y persistencia de cookie
        public async Task<(bool Exito, string? Error)> IniciarSesionAsync(LoginViewModel modelo)
        {
            var usuario = await _userManager.FindByEmailAsync(modelo.Email);
            if (usuario == null || !usuario.Activo)
            {
                return (false, "Credenciales inválidas.");
            }

            var resultado = await _signInManager.PasswordSignInAsync(
                usuario.UserName!,
                modelo.Password,
                isPersistent: modelo.Recordarme,
                lockoutOnFailure: true);

            if (resultado.Succeeded)
            {
                return (true, null);
            }

            if (resultado.IsLockedOut)
            {
                return (false, "Cuenta bloqueada temporalmente por intentos fallidos. Intente en 1 minuto.");
            }

            return (false, "Credenciales inválidas.");
        }

        public async Task CerrarSesionAsync()
        {
            await _signInManager.SignOutAsync();
        }

        // Generación de token temporal único para recuperación de contraseña
        public async Task<(bool Exito, string? Token)> GenerarTokenRecuperacionAsync(string email)
        {
            var usuario = await _userManager.FindByEmailAsync(email);
            if (usuario == null || !usuario.Activo)
            {
                return (false, null);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
            return (true, token);
        }

        // Envía el enlace real de recuperación mediante IEmailService
        public async Task EnviarCorreoRecuperacionAsync(string email, string enlace)
        {
            string cuerpoHtml = $@"
                <div style='font-family: Arial, sans-serif; color: #24292f;'>
                    <h2 style='color: #1f2428;'>MagnaTaller - Recuperación de Contraseña</h2>
                    <p>Hemos recibido una solicitud para restablecer tu contraseña.</p>
                    <p>Haz clic en el siguiente botón para crear una nueva clave:</p>
                    <p><a href='{enlace}' style='background-color: #e65c00; color: #fff; padding: 10px 18px; text-decoration: none; border-radius: 4px; font-weight: bold;'>Restablecer Contraseña</a></p>
                    <p style='font-size: 12px; color: #666;'>Este enlace es de un solo uso y tiene tiempo limitado.</p>
                </div>";

            await _emailService.EnviarCorreoAsync(email, "MagnaTaller - Restablecer Contraseña", cuerpoHtml);
        }

        // Restablece la contraseña e invalida el token de inmediato
        public async Task<(bool Exito, string? Error)> RestablecerContrasenaAsync(RestablecerContrasenaViewModel modelo)
        {
            var usuario = await _userManager.FindByEmailAsync(modelo.Email);
            if (usuario == null)
            {
                return (false, "No se pudo restablecer la contraseña.");
            }

            var resultado = await _userManager.ResetPasswordAsync(usuario, modelo.Token, modelo.NuevaPassword);
            if (!resultado.Succeeded)
            {
                return (false, "El enlace de recuperación es inválido, ha expirado o ya fue utilizado.");
            }

            return (true, null);
        }

        public async Task<PerfilViewModel?> ObtenerPerfilAsync(string userId)
        {
            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario == null) return null;

            var roles = await _userManager.GetRolesAsync(usuario);
            return new PerfilViewModel
            {
                NombreCompleto = usuario.NombreCompleto,
                Email = usuario.Email ?? string.Empty,
                Rol = roles.FirstOrDefault() ?? "Cliente",
                Activo = usuario.Activo
            };
        }

        public async Task<(bool Exito, string? Error)> CambiarContrasenaAsync(string userId, CambiarContrasenaViewModel modelo)
        {
            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario == null)
            {
                return (false, "Usuario no encontrado.");
            }

            var resultado = await _userManager.ChangePasswordAsync(usuario, modelo.PasswordActual, modelo.NuevaPassword);
            if (!resultado.Succeeded)
            {
                return (false, string.Join(" ", resultado.Errors.Select(e => e.Description)));
            }

            return (true, null);
        }

        // Listado administrativo de usuarios
        public async Task<List<UsuarioAdminViewModel>> ListarUsuariosAsync()
        {
            var usuarios = await _userManager.Users.OrderBy(u => u.NombreCompleto).ToListAsync();
            var admins = await _userManager.GetUsersInRoleAsync("Administrador");
            int adminsActivos = admins.Count(a => a.Activo);

            var lista = new List<UsuarioAdminViewModel>();
            foreach (var u in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(u);
                var rol = roles.FirstOrDefault() ?? "Cliente";

                lista.Add(new UsuarioAdminViewModel
                {
                    Id = u.Id,
                    NombreCompleto = u.NombreCompleto,
                    Email = u.Email ?? string.Empty,
                    Rol = rol,
                    Activo = u.Activo,
                    EstaBloqueado = u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow,
                    EsUltimoAdmin = rol == "Administrador" && u.Activo && adminsActivos <= 1
                });
            }

            return lista;
        }

        // Cambio de rol con protección al último administrador activo
        public async Task<(bool Exito, string? Error)> CambiarRolAsync(string usuarioId, string nuevoRol)
        {
            if (nuevoRol != "Administrador" && nuevoRol != "Cliente")
            {
                return (false, "Rol no válido.");
            }

            var usuario = await _userManager.FindByIdAsync(usuarioId);
            if (usuario == null)
            {
                return (false, "Usuario no encontrado.");
            }

            var rolesActuales = await _userManager.GetRolesAsync(usuario);
            if (rolesActuales.Contains(nuevoRol))
            {
                return (true, null);
            }

            if (rolesActuales.Contains("Administrador") && nuevoRol == "Cliente")
            {
                var admins = await _userManager.GetUsersInRoleAsync("Administrador");
                var activos = admins.Where(a => a.Activo).ToList();
                if (activos.Count <= 1 && activos.Any(a => a.Id == usuarioId))
                {
                    return (false, "No se puede quitar privilegios al último administrador activo.");
                }
            }

            await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
            await _userManager.AddToRoleAsync(usuario, nuevoRol);
            return (true, null);
        }

        // Habilitar o deshabilitar cuenta protegiendo al último administrador activo
        public async Task<(bool Exito, string? Error)> CambiarEstadoAsync(string usuarioId, bool activo)
        {
            var usuario = await _userManager.FindByIdAsync(usuarioId);
            if (usuario == null)
            {
                return (false, "Usuario no encontrado.");
            }

            if (!activo)
            {
                var roles = await _userManager.GetRolesAsync(usuario);
                if (roles.Contains("Administrador"))
                {
                    var admins = await _userManager.GetUsersInRoleAsync("Administrador");
                    var activos = admins.Where(a => a.Activo).ToList();
                    if (activos.Count <= 1 && activos.Any(a => a.Id == usuarioId))
                    {
                        return (false, "No se puede deshabilitar al último administrador activo.");
                    }
                }
            }

            usuario.Activo = activo;
            var resultado = await _userManager.UpdateAsync(usuario);
            if (!resultado.Succeeded)
            {
                return (false, "Error al actualizar el estado de la cuenta.");
            }

            return (true, null);
        }
    }
}

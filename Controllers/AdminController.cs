using MagnaTaller.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MagnaTaller.Web.Controllers
{
    // Capa de Presentación: Panel exclusivo para el rol Administrador
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly IAuthService _authService;

        public AdminController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> Usuarios()
        {
            var usuarios = await _authService.ListarUsuariosAsync();
            return View(usuarios);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarRol(string usuarioId, string nuevoRol)
        {
            var (exito, error) = await _authService.CambiarRolAsync(usuarioId, nuevoRol);
            TempData[exito ? "MensajeExito" : "MensajeError"] = exito
                ? "Rol actualizado correctamente."
                : error;

            return RedirectToAction(nameof(Usuarios));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(string usuarioId, bool activo)
        {
            var (exito, error) = await _authService.CambiarEstadoAsync(usuarioId, activo);
            TempData[exito ? "MensajeExito" : "MensajeError"] = exito
                ? "Estado de cuenta actualizado."
                : error;

            return RedirectToAction(nameof(Usuarios));
        }
    }
}

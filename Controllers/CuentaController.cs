using System.Security.Claims;
using MagnaTaller.Web.Models.ViewModels;
using MagnaTaller.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MagnaTaller.Web.Controllers
{
    // Capa de Presentación: Orquestador de autenticación y perfil
    public class CuentaController : Controller
    {
        private readonly IAuthService _authService;

        public CuentaController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(nameof(Perfil));
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel modelo, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var (exito, error) = await _authService.IniciarSesionAsync(modelo);
            if (exito)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction(nameof(Perfil));
            }

            ModelState.AddModelError(string.Empty, error ?? "Credenciales inválidas.");
            return View(modelo);
        }

        [HttpGet]
        public IActionResult Registro()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(nameof(Perfil));
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegistroViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var (exito, error) = await _authService.RegistrarClienteAsync(modelo);
            if (exito)
            {
                TempData["MensajeExito"] = "Cuenta creada correctamente. Ya puedes iniciar sesión.";
                return RedirectToAction(nameof(Login));
            }

            ModelState.AddModelError(string.Empty, error ?? "No se pudo completar el registro.");
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.CerrarSesionAsync();
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult OlvideContrasena() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OlvideContrasena(OlvideContrasenaViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var (exito, token) = await _authService.GenerarTokenRecuperacionAsync(modelo.Email);
            if (exito && token != null)
            {
                var enlace = Url.Action(
                    nameof(RestablecerContrasena),
                    "Cuenta",
                    new { email = modelo.Email, token },
                    Request.Scheme)!;

                await _authService.EnviarCorreoRecuperacionAsync(modelo.Email, enlace);
            }

            TempData["MensajeInfo"] = "Si el correo existe y está activo, recibirás un enlace de recuperación.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult RestablecerContrasena(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction(nameof(Login));
            }

            return View(new RestablecerContrasenaViewModel { Email = email, Token = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestablecerContrasena(RestablecerContrasenaViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var (exito, error) = await _authService.RestablecerContrasenaAsync(modelo);
            if (exito)
            {
                TempData["MensajeExito"] = "Contraseña restablecida. Inicia sesión con tu nueva clave.";
                return RedirectToAction(nameof(Login));
            }

            ModelState.AddModelError(string.Empty, error ?? "Token inválido o expirado.");
            return View(modelo);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var perfil = await _authService.ObtenerPerfilAsync(userId);
            if (perfil == null)
            {
                return RedirectToAction(nameof(Login));
            }
            return View(perfil);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarContrasena(CambiarContrasenaViewModel modelo)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            if (!ModelState.IsValid)
            {
                TempData["MensajeError"] = "Verifica los campos de la contraseña.";
                return RedirectToAction(nameof(Perfil));
            }

            var (exito, error) = await _authService.CambiarContrasenaAsync(userId, modelo);
            TempData[exito ? "MensajeExito" : "MensajeError"] = exito
                ? "Contraseña actualizada correctamente."
                : error;

            return RedirectToAction(nameof(Perfil));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            Response.StatusCode = 403;
            return View();
        }
    }
}

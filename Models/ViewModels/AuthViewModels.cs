using System.ComponentModel.DataAnnotations;

namespace MagnaTaller.Web.Models.ViewModels
{
    public class RegistroViewModel
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mínimo 6 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme su contraseña.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        [Display(Name = "Confirmar Contraseña")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Recordarme")]
        public bool Recordarme { get; set; }
    }

    public class OlvideContrasenaViewModel
    {
        [Required(ErrorMessage = "Ingrese su correo electrónico.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;
    }

    public class RestablecerContrasenaViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese la nueva contraseña.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mínimo 6 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]
        public string NuevaPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme la nueva contraseña.")]
        [DataType(DataType.Password)]
        [Compare("NuevaPassword", ErrorMessage = "Las contraseñas no coinciden.")]
        [Display(Name = "Confirmar Contraseña")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }

    public class PerfilViewModel
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class CambiarContrasenaViewModel
    {
        [Required(ErrorMessage = "Ingrese su contraseña actual.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña Actual")]
        public string PasswordActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese su nueva contraseña.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mínimo 6 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]
        public string NuevaPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme su nueva contraseña.")]
        [DataType(DataType.Password)]
        [Compare("NuevaPassword", ErrorMessage = "Las contraseñas no coinciden.")]
        [Display(Name = "Confirmar Nueva Contraseña")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }

    public class UsuarioAdminViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public bool EstaBloqueado { get; set; }
        public bool EsUltimoAdmin { get; set; }
    }
}

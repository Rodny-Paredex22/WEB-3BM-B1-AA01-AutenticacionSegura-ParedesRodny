using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace MagnaTaller.Web.Data.Entities
{
    // Entidad de usuario extendida para MagnaTaller
    public class UsuarioMagna : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string NombreCompleto { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
    }
}

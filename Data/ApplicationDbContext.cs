using MagnaTaller.Web.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MagnaTaller.Web.Data
{
    // Contexto de base de datos con soporte para ASP.NET Core Identity
    public class ApplicationDbContext : IdentityDbContext<UsuarioMagna>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}

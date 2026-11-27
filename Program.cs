using MagnaTaller.Web.Data;
using MagnaTaller.Web.Data.Entities;
using MagnaTaller.Web.Services.Implementations;
using MagnaTaller.Web.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Conexión a SQL Server con Entity Framework Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Configuración de ASP.NET Core Identity (PBKDF2 y Lockout)
builder.Services.AddIdentity<UsuarioMagna, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;

    // Bloqueo por 3 intentos fallidos durante 1 minuto
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. Configuración explícita de cookies de autenticación
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".MagnaTaller.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.LoginPath = "/Cuenta/Login";
    options.LogoutPath = "/Cuenta/Logout";
    options.AccessDeniedPath = "/Cuenta/AccessDenied";
});

// 4. Inyección de dependencias (Capas de Negocio e Infraestructura)
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 5. Inicialización controlada de roles y administrador inicial
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<UsuarioMagna>>();

    string[] roles = ["Administrador", "Cliente"];
    foreach (var rol in roles)
    {
        if (!await roleManager.RoleExistsAsync(rol))
        {
            await roleManager.CreateAsync(new IdentityRole(rol));
        }
    }

    // Administrador inicial activo
    const string adminEmail = "admin@magnataller.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new UsuarioMagna
        {
            UserName = adminEmail,
            Email = adminEmail,
            NombreCompleto = "Administrador MagnaTaller",
            Activo = true,
            EmailConfirmed = true
        };
        var res = await userManager.CreateAsync(admin, "Admin123!");
        if (res.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Administrador");
        }
    }
}

// 6. Pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Cuenta/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Cuenta}/{action=Login}/{id?}");

app.Run();

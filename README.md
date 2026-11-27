# MagnaTaller - Módulo Integral de Autenticación y Seguridad (AA01)

**Estudiante:** Rodny Paredes  
**Tecnología:** ASP.NET Core MVC (.NET 10) + Entity Framework Core + SQL Server + ASP.NET Core Identity  

## 1. Arquitectura en 3 Capas
- **Capa de Datos (`Data/`):** `ApplicationDbContext`, entidad `UsuarioMagna` (con `NombreCompleto` y `Activo`) y migraciones de EF Core.
- **Capa de Negocio (`Services/` y `Models/ViewModels/`):** `AuthService` (reglas de seguridad, lockout, tokens, protección del último administrador) y `EmailService` (envío SMTP real).
- **Capa de Presentación (`Controllers/` y `Views/`):** `CuentaController`, `AdminController` y vistas Razor con diseño industrial MagnaTaller.

## 2. Configuración Inicial (User Secrets y Base de Datos)
Las credenciales SMTP **nunca** se guardan en `appsettings.json`. Se configuran mediante User Secrets en la carpeta del proyecto:

```powershell
dotnet user-secrets set "SmtpSettings:Servidor" "smtp.gmail.com"
dotnet user-secrets set "SmtpSettings:Puerto" "587"
dotnet user-secrets set "SmtpSettings:Usuario" "TU_CORREO@gmail.com"
dotnet user-secrets set "SmtpSettings:Password" "TU_APP_PASSWORD_DE_GMAIL"
dotnet user-secrets set "SmtpSettings:Remitente" "TU_CORREO@gmail.com"
```

Para generar o actualizar la base de datos manualmente:
```powershell
dotnet ef database update
```

## 3. Cuenta Inicial (Seeding)
- **Administrador:** `admin@magnataller.com` / `Admin123!`

## 4. Casos de Prueba Principales
1. **Registro público:** Crea cuentas con rol `Cliente` y `Activo = true` automáticamente.
2. **Cifrado PBKDF2:** Contraseñas almacenadas con hash + sal en `AspNetUsers.PasswordHash`.
3. **Bloqueo (Lockout):** 3 intentos fallidos seguidos bloquean la cuenta por 1 minuto.
4. **Cookies y Recordarme:** `IsPersistent = true` mantiene sesión entre pestañas; ventana de incógnito permanece aislada.
5. **Recuperación real por correo:** Genera token único enviado a `ronypygg@gmail.com` y lo invalida tras su uso.
6. **Control de roles (403):** Un `Cliente` que intente entrar a `/Admin/Usuarios` recibe la vista `AccessDenied`.
7. **Protección del último Admin:** El sistema impide degradar o deshabilitar al único administrador activo.

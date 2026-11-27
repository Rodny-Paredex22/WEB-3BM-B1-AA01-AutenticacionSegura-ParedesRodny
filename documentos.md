# Documentación del proyecto MagnaTaller

## Propósito
Este proyecto es la solución **MagnaTaller** para la actividad AA01: *Módulo integral de autenticación y seguridad* en el taller automotriz.

## Arquitectura de 3 capas
- **Capa de Presentación** (`Controllers`, vistas Razor) – orquesta la interacción con el usuario.
- **Capa de Negocio** (`Services`, `AuthService`) – contiene reglas de negocio, validaciones y gestión de correos.
- **Capa de Datos** (`ApplicationDbContext`) – acceso a SQL Server mediante ASP.NET Core Identity.

## Roles y funcionalidades
| Rol          | Acciones permitidas                                           |
|--------------|--------------------------------------------------------------|
| **Cliente**  | Registro, acceso a su perfil, cambio de contraseña.        |
| **Admin**    | Panel de administración, gestión de usuarios y roles, habilitar/deshabilitar cuentas.

## Seguridad obligatoria
- Contraseñas encriptadas con PBKDF2 (Identity PasswordHasher).
- **Lockout** después de 3 intentos fallidos → 5 minutos.
- Recuperación de contraseña vía email con token temporal.
- Protección del último administrador activo (no se permite desactivar o eliminar).

## Configuración rápida
1. **Base de datos** – Cadena en *appsettings.Development.json* o `dotnet user-secrets`.
2. **SMTP** – Configurado con `dotnet user-secrets` (`SmtpSettings`).
3. **Ejecutar** – `dotnet run` desde la carpeta del proyecto.

## Próximos pasos
- Añadir pruebas unitarias.
- Mejorar UI con Bootstrap y paleta `#e65c00`.
- Documentar endpoints API (si se expande).

---
*Esta documentación está pensada para ser versionada en Git y servir como guía para el profesor y futuros desarrolladores.*
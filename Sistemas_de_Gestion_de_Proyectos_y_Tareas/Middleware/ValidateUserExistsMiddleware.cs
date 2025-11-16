using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Middleware
{
    public class ValidateUserExistsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ValidateUserExistsMiddleware> _logger;

        private static readonly string[] RutasPublicas =
        {
            "/login",
            "/logout",
            "/accessdenied",
            "/error",
            "/privacy"
        };

        public ValidateUserExistsMiddleware(
            RequestDelegate next,
            ILogger<ValidateUserExistsMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context,
            UsuarioApiClient usuarioApi)
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                string path = context.Request.Path.Value?.ToLower() ?? "";

                // ✔ Excluir rutas públicas
                if (RutasPublicas.Any(r => path.StartsWith(r)))
                {
                    await _next(context);
                    return;
                }

                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(userIdClaim, out int userId))
                {
                    try
                    {
                        var usuario = await usuarioApi.GetByIdAsync(userId);

                        // ✔ Usuario eliminado en la API
                        if (usuario == null)
                        {
                            _logger.LogWarning(
                                $"Usuario con ID {userId} no existe en la API. Cerrando sesión.");

                            await context.SignOutAsync("MyCookieAuth");

                            context.Response.Cookies.Append(
                                "SessionExpiredMessage",
                                "Tu cuenta ya no existe. Contacta al administrador.",
                                new CookieOptions
                                {
                                    HttpOnly = false,
                                    Secure = true,
                                    SameSite = SameSiteMode.Strict,
                                    Expires = DateTimeOffset.UtcNow.AddMinutes(1)
                                });

                            context.Response.Redirect("/Login/Login");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            $"Error al consultar usuario en API (ID {userId}).");
                    }
                }
            }

            await _next(context);
        }
    }

    public static class ValidateUserExistsMiddlewareExtensions
    {
        public static IApplicationBuilder UseValidateUserExists(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ValidateUserExistsMiddleware>();
        }
    }
}

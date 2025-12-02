using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Middleware
{
    public class ValidateUserExistsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ValidateUserExistsMiddleware> _logger;

        public ValidateUserExistsMiddleware(RequestDelegate next, ILogger<ValidateUserExistsMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, UsuarioApiClient usuarioApi)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            if (path.StartsWith("/login") ||
                path.StartsWith("/logout") ||
                path.StartsWith("/accessdenied") ||
                path.StartsWith("/error") ||
                path.StartsWith("/privacy") ||
                path.Contains(".css") ||
                path.Contains(".js") ||
                path.Contains(".png") ||
                path.Contains(".jpg") ||
                path.Contains("/lib/"))
            {
                await _next(context);
                return;
            }

            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                await _next(context);
                return;
            }

            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                await _next(context);
                return;
            }

            try
            {
                var usuario = await usuarioApi.GetByIdAsync(userId);

                if (usuario == null)
                {
                    _logger.LogWarning($"Usuario con ID {userId} no existe. Cerrando sesión.");
                    await context.SignOutAsync("MyCookieAuth");
                    context.Response.Redirect("/Login/Login");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando existencia del usuario.");
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

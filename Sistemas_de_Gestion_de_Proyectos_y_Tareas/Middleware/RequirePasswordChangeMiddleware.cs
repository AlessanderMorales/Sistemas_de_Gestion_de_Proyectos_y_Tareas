using System.Security.Claims;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Middleware
{
    public class RequirePasswordChangeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequirePasswordChangeMiddleware> _logger;

        public RequirePasswordChangeMiddleware(RequestDelegate next, ILogger<RequirePasswordChangeMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            if (path.StartsWith("/login") ||
                path.StartsWith("/logout") ||
                path.StartsWith("/accessdenied") ||
                path.StartsWith("/error") ||
                path.StartsWith("/privacy") ||
                path.StartsWith("/configuracion/cambiarcontrase") ||
                path.Contains(".css") ||
                path.Contains(".js") ||
                path.Contains(".png") ||
                path.Contains(".jpg") ||
                path.Contains(".ico") ||
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

            var requiereCambioClaim = context.User.FindFirst("RequiereCambioContraseña")?.Value;
            var userName = context.User.FindFirst(ClaimTypes.Name)?.Value ?? "Usuario desconocido";

            _logger.LogDebug($"Usuario: {userName}, RequiereCambioContraseña claim: '{requiereCambioClaim}'");

            if (requiereCambioClaim == "True")
            {
                _logger.LogInformation($"Usuario {userName} requiere cambio de contraseña obligatorio. Redirigiendo desde {path} a /Configuracion/CambiarPassword");
                context.Response.Redirect("/Configuracion/CambiarPassword");
                return;
            }

            await _next(context);
        }
    }

    public static class RequirePasswordChangeMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequirePasswordChange(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequirePasswordChangeMiddleware>();
        }
    }
}

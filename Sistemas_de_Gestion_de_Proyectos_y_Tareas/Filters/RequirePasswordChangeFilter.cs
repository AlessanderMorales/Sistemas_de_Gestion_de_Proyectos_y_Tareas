using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Filters
{
    public class RequirePasswordChangeFilter : IPageFilter
    {
        private readonly ILogger<RequirePasswordChangeFilter> _logger;

        public RequirePasswordChangeFilter(ILogger<RequirePasswordChangeFilter> logger)
        {
            _logger = logger;
        }

        public void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {
        }

        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            var path = context.HttpContext.Request.Path.Value ?? "";
            var pathLower = path.ToLower();

            _logger.LogDebug($"RequirePasswordChangeFilter ejecutándose en: {path}");

            var excludedPaths = new[]
            {
                "/login",
                "/logout",
                "/accessdenied",
                "/error",
                "/privacy",
                "/configuracion/cambiarpassword"
            };

            if (excludedPaths.Any(excluded => pathLower.Contains(excluded)))
            {
                _logger.LogDebug($"Ruta excluida, no se aplica filtro: {path}");
                return;
            }

            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogDebug("Usuario no autenticado, no se aplica filtro");
                return;
            }

            var requiereCambioClaim = context.HttpContext.User.FindFirst("RequiereCambioContraseña")?.Value;
            var userName = context.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? "Usuario desconocido";

            _logger.LogDebug($"Usuario: {userName}, RequiereCambioContraseña claim: '{requiereCambioClaim}'");

            if (string.Equals(requiereCambioClaim, "True", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning($"Usuario {userName} requiere cambio de contraseña. Redirigiendo desde {path} a /Configuracion/CambiarPassword");
                context.Result = new RedirectToPageResult("/Configuracion/CambiarPassword");
            }
            else
            {
                _logger.LogDebug($"Usuario {userName} NO requiere cambio de contraseña (claim: '{requiereCambioClaim}')");
            }
        }

        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
        }
    }
}

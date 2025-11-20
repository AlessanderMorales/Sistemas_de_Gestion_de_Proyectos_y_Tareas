using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.Application.Facades;
using System.Security.Claims;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly GestionProyectosFacade _facade;

        public IndexModel(GestionProyectosFacade facade)
        {
            _facade = facade;
        }

        public EstadisticasGeneralesViewModel Estadisticas { get; set; }
        public DashboardUsuarioViewModel DashboardUsuario { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (User.IsInRole("SuperAdmin"))
            {
                return RedirectToPage("/Usuarios/Index");
            }

            if (User.IsInRole("JefeDeProyecto"))
            {
                Estadisticas = await _facade.ObtenerEstadisticasGeneralesAsync();
            }

            if (User.IsInRole("Empleado") || User.IsInRole("JefeDeProyecto"))
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idClaim, out var usuarioId))
                {
                    DashboardUsuario = await _facade.ObtenerDashboardUsuarioAsync(usuarioId);

                    if (DashboardUsuario == null)
                    {
                        TempData["ErrorMessage"] = "Tu cuenta de usuario no fue encontrada. Por favor, contacta al administrador o inicia sesión nuevamente.";
                        return RedirectToPage("/Logout");
                    }
                }
            }

            return Page();
        }
    }
}
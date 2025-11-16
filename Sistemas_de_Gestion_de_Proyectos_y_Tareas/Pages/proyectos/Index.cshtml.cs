using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Proyectos;
using System.Security.Claims;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Proyectos
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ProyectoApiClient _proyectoApi;

        public List<ProyectoDTO> Proyectos { get; set; } = new();

        public IndexModel(ProyectoApiClient proyectoApi)
        {
            _proyectoApi = proyectoApi;
        }

        public async Task OnGetAsync()
        {
            // Caso 1: El usuario es empleado → solo ver proyectos asignados
            if (User.IsInRole("Empleado"))
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(idClaim, out var usuarioId))
                {
                    Proyectos = await _proyectoApi.GetByUsuarioAsync(usuarioId);
                    return;
                }
            }

            // Caso 2: Jefe o Admin → ver todos
            Proyectos = await _proyectoApi.GetAllAsync();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (User.IsInRole("Empleado"))
            {
                TempData["ErrorMessage"] = "No estás autorizado para eliminar proyectos.";
                return RedirectToPage("./Index");
            }

            var ok = await _proyectoApi.DeleteAsync(id);

            TempData[ok ? "SuccessMessage" : "ErrorMessage"] =
                ok ? "Proyecto eliminado correctamente."
                   : "Error al eliminar el proyecto.";

            return RedirectToPage("./Index");
        }

        // (Opcional) Si decides mover reportes al API:
        // public async Task<IActionResult> OnPostGenerarReporte() {...}
    }
}

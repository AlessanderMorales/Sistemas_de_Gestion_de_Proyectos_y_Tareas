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
            if (User.IsInRole("Empleado"))
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(idClaim, out var usuarioId))
                {
                    Proyectos = await _proyectoApi.GetByUsuarioAsync(usuarioId);
                    return;
                }
            }

            Proyectos = await _proyectoApi.GetAllAsync();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (User.IsInRole("Empleado"))
            {
                TempData["ErrorMessage"] = "No estás autorizado para eliminar proyectos.";
                return RedirectToPage("./Index");
            }

            var success = await _proyectoApi.DeleteAsync(id);

            TempData[success ? "SuccessMessage" : "ErrorMessage"] =
                success ? "Proyecto eliminado correctamente."
                        : "Error al eliminar el proyecto.";

            return RedirectToPage("./Index");
        }

    }
}

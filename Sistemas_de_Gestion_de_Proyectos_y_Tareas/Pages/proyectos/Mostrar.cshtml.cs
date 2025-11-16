using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Proyectos;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Proyectos
{
    [Authorize]
    public class MostrarModel : PageModel
    {
        private readonly ProyectoApiClient _proyectoApi;

        public ProyectoDTO Proyecto { get; private set; }
        public List<TareaDTO> Tareas { get; private set; } = new();

        public MostrarModel(ProyectoApiClient proyectoApi)
        {
            _proyectoApi = proyectoApi;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            // Obtener solo proyecto
            Proyecto = await _proyectoApi.GetByIdAsync(id.Value);
            if (Proyecto == null)
                return NotFound();

            // Obtener tareas del proyecto (tu API debe tener este endpoint)
            Tareas = await _proyectoApi.GetTareasByProyectoAsync(id.Value);

            return Page();
        }
    }
}

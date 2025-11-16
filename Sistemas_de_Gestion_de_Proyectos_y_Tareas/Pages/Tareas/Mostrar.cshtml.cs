using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas; // <-- usa tu DTO correcto
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients; // <-- usa tu namespace real

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Tareas
{
    [Authorize]
    public class MostrarModel : PageModel
    {
        private readonly TareaApiClient _tareaApi;

        public TareaDTO Tarea { get; set; } = default!;

        public MostrarModel(TareaApiClient tareaApi)
        {
            _tareaApi = tareaApi;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var tarea = await _tareaApi.GetAsync(id);

            if (tarea == null)
                return NotFound();

            Tarea = tarea;
            return Page();
        }
    }
}

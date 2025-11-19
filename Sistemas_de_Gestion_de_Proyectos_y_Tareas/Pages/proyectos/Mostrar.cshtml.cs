using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Proyectos;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas;
using System.Collections.Generic; // Asegúrate de que este using esté presente

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Proyectos
{
    [Authorize]
    public class MostrarModel : PageModel
    {
        private readonly ProyectoApiClient _proyectoApi;
        private readonly TareaApiClient _tareaApi; // <--- ¡Añadimos la inyección del TareaApiClient!

        public ProyectoDTO Proyecto { get; private set; }
        // public List<TareaDTO> Tareas { get; private set; } = new(); // <--- ¡Esta línea se ELIMINA!
        // Las tareas ahora se almacenarán directamente en Proyecto.Tareas

        // Modificamos el constructor para recibir TareaApiClient
        public MostrarModel(ProyectoApiClient proyectoApi, TareaApiClient tareaApi)
        {
            _proyectoApi = proyectoApi;
            _tareaApi = tareaApi; // Asignamos el cliente de tareas
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();
            Proyecto = await _proyectoApi.GetByIdAsync(id.Value);
            if (Proyecto == null)
                return NotFound();
            Proyecto.Tareas = await _tareaApi.GetByProyectoAsync(id.Value);

            return Page();
        }
    }
}
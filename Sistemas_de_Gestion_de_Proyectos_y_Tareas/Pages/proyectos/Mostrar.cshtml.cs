using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Proyectos;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas;
using System.Collections.Generic;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Proyectos
{
    [Authorize]
    public class MostrarModel : PageModel
    {
        private readonly ProyectoApiClient _proyectoApi;
        private readonly TareaApiClient _tareaApi;

        public ProyectoDTO Proyecto { get; private set; }

        public MostrarModel(ProyectoApiClient proyectoApi, TareaApiClient tareaApi)
        {
            _proyectoApi = proyectoApi;
            _tareaApi = tareaApi;
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

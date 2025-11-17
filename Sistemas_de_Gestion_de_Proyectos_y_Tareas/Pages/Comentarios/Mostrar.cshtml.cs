using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Comentarios;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Proyectos;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Comentarios
{
    [Authorize]
    public class MostrarModel : PageModel
    {
        private readonly ComentarioApiClient _comentarioApi;
        private readonly TareaApiClient _tareaApi;
        private readonly ProyectoApiClient _proyectoApi;

        public ComentarioDTO Comentario { get; set; }

        public MostrarModel(
            ComentarioApiClient comentarioApi,
            TareaApiClient tareaApi,
            ProyectoApiClient proyectoApi)
        {
            _comentarioApi = comentarioApi;
            _tareaApi = tareaApi;
            _proyectoApi = proyectoApi;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Obtener comentario
            Comentario = await _comentarioApi.GetByIdAsync(id);

            if (Comentario == null)
                return RedirectToPage("Index");

            // Si el comentario tiene tarea asociada
            if (Comentario.IdTarea > 0)
            {
                var tarea = await _tareaApi.GetByIdAsync(Comentario.IdTarea);

                if (tarea != null)
                {
                    Comentario.Tarea = tarea;

                    if (tarea.IdProyecto > 0)
                    {
                        var proyecto = await _proyectoApi.GetByIdAsync(tarea.IdProyecto);

                        if (proyecto != null)
                            Comentario.Tarea.ProyectoNombre = proyecto.Nombre;
                    }
                }
            }
            return Page();
        }
    }
}

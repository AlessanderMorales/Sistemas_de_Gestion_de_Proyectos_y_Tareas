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
                // Cambiar GetByUsuarioAsync para obtener una sola tarea
                var tareas = await _tareaApi.GetByUsuarioAsync(Comentario.IdTarea);

                if (tareas != null && tareas.Count > 0)
                {
                    // Asignar la primera tarea de la lista
                    Comentario.Tarea = tareas[0];

                    // Obtener proyecto
                    if (Comentario.Tarea.IdProyecto > 0)
                    {
                        var proyecto = await _proyectoApi.GetByIdAsync(Comentario.Tarea.IdProyecto);
                        if (proyecto != null)
                        {
                            Comentario.Tarea.ProyectoNombre = proyecto.Nombre;
                        }
                    }
                }
            }

            return Page();
        }
    }
}

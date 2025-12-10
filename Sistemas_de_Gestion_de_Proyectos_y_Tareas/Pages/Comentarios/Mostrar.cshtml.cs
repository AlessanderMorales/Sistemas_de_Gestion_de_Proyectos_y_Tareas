using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Comentarios;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Proyectos;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Comentarios
{
    [Authorize]
    public class MostrarModel : PageModel
    {
        private readonly ComentarioApiClient _comentarioApi;
        private readonly TareaApiClient _tareaApi;
        private readonly ProyectoApiClient _proyectoApi;
        private readonly UsuarioApiClient _usuarioApi;

        public ComentarioDTO Comentario { get; set; } = new();

        public MostrarModel(
            ComentarioApiClient comentarioApi,
            TareaApiClient tareaApi,
            ProyectoApiClient proyectoApi,
            UsuarioApiClient usuarioApi)
        {
            _comentarioApi = comentarioApi;
            _tareaApi = tareaApi;
            _proyectoApi = proyectoApi;
            _usuarioApi = usuarioApi;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Comentario = await _comentarioApi.GetByIdAsync(id);

            if (Comentario == null)
            {
                TempData["ErrorMessage"] = "Comentario no encontrado.";
                return RedirectToPage("/Comentarios/Index");
            }

            if (Comentario.IdUsuario > 0)
            {
                Comentario.Usuario = await _usuarioApi.GetByIdAsync(Comentario.IdUsuario);
            }

            if (Comentario.IdTarea.HasValue && Comentario.IdTarea.Value > 0)
            {
                var tarea = await _tareaApi.GetByIdAsync(Comentario.IdTarea.Value);

                if (tarea != null)
                {
                    Comentario.Tarea = tarea;

                    if (tarea.IdProyecto.HasValue && tarea.IdProyecto.Value > 0)
                    {
                        var proyecto = await _proyectoApi.GetByIdAsync(tarea.IdProyecto.Value);

                        if (proyecto != null)
                        {
                            Comentario.Tarea.ProyectoNombre = proyecto.Nombre;
                        }
                    }

                    if (tarea.IdUsuarioAsignado.HasValue && tarea.IdUsuarioAsignado.Value > 0)
                    {
                        var usuarioAsignado = await _usuarioApi.GetByIdAsync(tarea.IdUsuarioAsignado.Value);
                        Comentario.Tarea.UsuarioAsignadoNombre = $"{usuarioAsignado?.Nombres} {usuarioAsignado?.PrimerApellido}";
                    }
                }
            }

            if (Comentario.IdDestinatario.HasValue && Comentario.IdDestinatario.Value > 0)
            {
                Comentario.Destinatario = await _usuarioApi.GetByIdAsync(Comentario.IdDestinatario.Value);
            }

            return Page();
        }
    }
}

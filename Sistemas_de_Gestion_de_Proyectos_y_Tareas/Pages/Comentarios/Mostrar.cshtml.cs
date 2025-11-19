using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Comentarios;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Proyectos;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas; // Asegúrate de tener este using para TareaDTO

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Comentarios
{
    [Authorize]
    public class MostrarModel : PageModel
    {
        private readonly ComentarioApiClient _comentarioApi;
        private readonly TareaApiClient _tareaApi;
        private readonly ProyectoApiClient _proyectoApi;
        private readonly UsuarioApiClient _usuarioApi; // <-- Posiblemente necesario para el destinatario/autor

        public ComentarioDTO Comentario { get; set; } = new(); // Inicializar para evitar NullReferenceException

        public MostrarModel(
            ComentarioApiClient comentarioApi,
            TareaApiClient tareaApi,
            ProyectoApiClient proyectoApi,
            UsuarioApiClient usuarioApi) // <-- Inyectar UsuarioApiClient si se usa para enriquecer
        {
            _comentarioApi = comentarioApi;
            _tareaApi = tareaApi;
            _proyectoApi = proyectoApi;
            _usuarioApi = usuarioApi; // Asignar
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Obtener comentario
            Comentario = await _comentarioApi.GetByIdAsync(id);

            if (Comentario == null)
            {
                TempData["ErrorMessage"] = "Comentario no encontrado.";
                return RedirectToPage("/Comentarios/Index"); // Redirige a Index, no a una página relativa
            }

            // Enriquecer el autor del comentario (siempre debe existir)
            if (Comentario.IdUsuario > 0)
            {
                Comentario.Usuario = await _usuarioApi.GetByIdAsync(Comentario.IdUsuario);
            }

            // Si el comentario tiene tarea asociada
            // <-- ¡CORRECCIÓN CLAVE AQUÍ! -->
            if (Comentario.IdTarea.HasValue && Comentario.IdTarea.Value > 0)
            {
                var tarea = await _tareaApi.GetByIdAsync(Comentario.IdTarea.Value); // Usamos .Value porque .HasValue ya lo confirmó

                if (tarea != null)
                {
                    // Asegúrate de que Tarea en ComentarioDTO sea TareaDTO? para evitar NullRef si no se encuentra
                    // y que Comentario.Tarea = tarea; es una asignación válida.
                    Comentario.Tarea = tarea;

                    // Si la tarea tiene proyecto asociado
                    // <-- ¡CORRECCIÓN CLAVE AQUÍ! -->
                    if (tarea.IdProyecto.HasValue && tarea.IdProyecto.Value > 0)
                    {
                        var proyecto = await _proyectoApi.GetByIdAsync(tarea.IdProyecto.Value); // Usamos .Value

                        if (proyecto != null)
                        {
                            // Asegúrate de que tu TareaDTO en el proyecto cliente tiene ProyectoNombre
                            Comentario.Tarea.ProyectoNombre = proyecto.Nombre;
                        }
                    }
                    // Si IdUsuarioAsignado en TareaDTO es nullable (int?), también debemos usar .HasValue/.Value
                    if (tarea.IdUsuarioAsignado.HasValue && tarea.IdUsuarioAsignado.Value > 0)
                    {
                        var usuarioAsignado = await _usuarioApi.GetByIdAsync(tarea.IdUsuarioAsignado.Value);
                        Comentario.Tarea.UsuarioAsignadoNombre = $"{usuarioAsignado?.Nombres} {usuarioAsignado?.PrimerApellido}";
                    }
                }
            }

            // Si el comentario tiene un destinatario directo
            // <-- ¡CORRECCIÓN CLAVE AQUÍ! -->
            if (Comentario.IdDestinatario.HasValue && Comentario.IdDestinatario.Value > 0)
            {
                Comentario.Destinatario = await _usuarioApi.GetByIdAsync(Comentario.IdDestinatario.Value);
            }


            return Page();
        }
    }
}
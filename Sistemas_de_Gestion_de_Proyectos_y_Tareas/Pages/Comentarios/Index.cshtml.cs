using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Comentarios;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas;
using System.Security.Claims;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Comentarios
{
    [Authorize]
    public class ComentariosIndexModel : PageModel
    {
        private readonly TareaApiClient _tareaApiClient;
        private readonly UsuarioApiClient _usuarioApiClient;
        private readonly ProyectoApiClient _proyectoApiClient;
        private readonly ComentarioApiClient _comentarioService;

        public List<TareaExtendidaDTO> Tareas { get; set; } = new();

        public ComentariosIndexModel(
            TareaApiClient tareaApi,
            UsuarioApiClient usuarioApi,
            ProyectoApiClient proyectoApi,
            ComentarioApiClient comentarioApi)
        {
            _tareaApiClient = tareaApi;
            _usuarioApiClient = usuarioApi;
            _proyectoApiClient = proyectoApi;
            _comentarioService = comentarioApi;
        }
        public List<ComentarioDTO> Comentarios { get; set; } = new();
        public int UsuarioActualId { get; set; }


        public async Task OnGetAsync()
        {
            var idClaimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(idClaimValue, out var userId))
                UsuarioActualId = userId;

            // Obtener todos los comentarios
            Comentarios = (await _comentarioService.GetAllAsync()).ToList();
            // Si es empleado → obtener tareas asignadas
            if (User.IsInRole("Empleado"))
            {
                string? empleadoIdClaimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(empleadoIdClaimValue, out int usuarioId))
                {
                    UsuarioActualId = usuarioId;

                    Comentarios = Comentarios
                        .Where(c =>
                            c.IdUsuario == usuarioId || // comentarios hechos por el usuario
                            c.Tarea?.Id == usuarioId // comentarios recibidos por asignación de tarea
                        )
                        .ToList();
                }
            }

            // Si es jefe o admin → todas las tareas
            var todas = await _tareaApiClient.GetAllAsync();
            Tareas = await EnriquecerTareas(todas);
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            Console.WriteLine("=== ELIMINAR COMENTARIO ID = " + id);

            // Solo jefe o admin pueden eliminar
            if (User.IsInRole("Empleado"))
            {
                TempData["ErrorMessage"] = "No estás autorizado para eliminar comentarios.";
                return RedirectToPage();
            }

            var ok = await _comentarioService.DeleteAsync(id);

            if (!ok)
            {
                TempData["ErrorMessage"] = "Error al eliminar el comentario.";
                return RedirectToPage();
            }

            TempData["SuccessMessage"] = "Comentario eliminado correctamente.";
            return RedirectToPage();
        }


        private async Task<List<TareaExtendidaDTO>> EnriquecerTareas(List<TareaDTO> tareas)
        {
            var usuarios = await _usuarioApiClient.GetAllAsync();
            var proyectos = await _proyectoApiClient.GetAllAsync();

            var lista = new List<TareaExtendidaDTO>();

            foreach (var t in tareas)
            {
                var tarea = new TareaExtendidaDTO
                {
                    Id = t.Id,
                    Titulo = t.Titulo,
                    Descripcion = t.Descripcion,
                    Prioridad = t.Prioridad,
                    Status = t.Status,
                    IdProyecto = t.IdProyecto
                };

                // nombre del proyecto
                tarea.ProyectoNombre =
                    proyectos.FirstOrDefault(p => p.IdProyecto == t.IdProyecto)?.Nombre;

                // usuario asignado
                var asignados = await _tareaApiClient.GetUsuariosAsignadosAsync(t.Id);

                if (asignados.Any())
                {
                    var u = usuarios.FirstOrDefault(us => us.Id == asignados.First());
                    tarea.UsuarioAsignadoNombre = $"{u?.Nombres} {u?.PrimerApellido}";
                }

                lista.Add(tarea);
            }

            return lista;
        }
    }
}

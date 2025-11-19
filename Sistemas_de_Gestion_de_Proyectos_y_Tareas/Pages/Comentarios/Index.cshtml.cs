using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Comentarios;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Proyectos;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios;
using System.Security.Claims;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Comentarios
{
    [Authorize]
    public class ComentariosIndexModel : PageModel
    {
        private readonly TareaApiClient _tareaApiClient;
        private readonly UsuarioApiClient _usuarioApiClient;
        private readonly ProyectoApiClient _proyectoApiClient;
        private readonly ComentarioApiClient _comentarioApi;

        public ComentariosIndexModel(
            TareaApiClient tareaApi,
            UsuarioApiClient usuarioApi,
            ProyectoApiClient proyectoApi,
            ComentarioApiClient comentarioApi)
        {
            _tareaApiClient = tareaApi;
            _usuarioApiClient = usuarioApi;
            _proyectoApiClient = proyectoApi;
            _comentarioApi = comentarioApi;
        }

        public List<ComentarioDTO> Comentarios { get; set; } = new();
        public int UsuarioActualId { get; set; }

        // =====================================================================
        //                               ON GET
        // =====================================================================
        public async Task OnGetAsync()
        {
            // 1. Obtener ID del usuario autenticado
            var idClaimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(idClaimValue, out int userId);
            UsuarioActualId = userId;

            // 2. Obtener datos base (de microservicios)
            var comentarios = (await _comentarioApi.GetAllAsync())?.ToList() ?? new();
            var usuarios = (await _usuarioApiClient.GetAllAsync())?.ToList() ?? new();
            var tareas = (await _tareaApiClient.GetAllAsync())?.ToList() ?? new();
            var proyectos = (await _proyectoApiClient.GetAllAsync())?.ToList() ?? new();

            // ===========================================================
            // 3. ENRIQUECER COMENTARIOS (Asignar Usuario, Tarea, Proyecto)
            // ===========================================================
            foreach (var c in comentarios)
            {
                // Autor
                c.Usuario = usuarios.FirstOrDefault(u => u.Id == c.IdUsuario);

                // Tarea
                if (c.IdTarea.HasValue)
                {
                    var tarea = tareas.FirstOrDefault(t => t.Id == c.IdTarea.Value);
                    if (tarea != null)
                    {
                        c.Tarea = new TareaExtendidaDTO
                        {
                            Id = tarea.Id,
                            Titulo = tarea.Titulo,
                            Descripcion = tarea.Descripcion,
                            Prioridad = tarea.Prioridad,
                            Status = tarea.Status,
                            IdProyecto = tarea.IdProyecto,
                            IdUsuarioAsignado = tarea.IdUsuarioAsignado
                        };

                        // Proyecto
                        if (tarea.IdProyecto.HasValue)
                        {
                            var proyecto = proyectos.FirstOrDefault(p => p.IdProyecto == tarea.IdProyecto);
                            c.Tarea.ProyectoNombre = proyecto?.Nombre ?? "-";
                        }

                        // Usuario asignado a la tarea
                        if (tarea.IdUsuarioAsignado.HasValue)
                        {
                            var u = usuarios.FirstOrDefault(x => x.Id == tarea.IdUsuarioAsignado.Value);
                            c.Tarea.UsuarioAsignadoNombre = $"{u?.Nombres} {u?.PrimerApellido}";
                        }
                    }
                }

                // Destinatario
                if (c.IdDestinatario.HasValue)
                {
                    c.Destinatario = usuarios.FirstOrDefault(u => u.Id == c.IdDestinatario.Value);
                }
            }

            // ===========================================================
            // 4. FILTRAR SEGÚN ROL
            // ===========================================================
            if (User.IsInRole("Empleado"))
            {
                comentarios = comentarios
                    .Where(c =>
                        c.IdUsuario == UsuarioActualId ||
                        (c.IdDestinatario.HasValue && c.IdDestinatario.Value == UsuarioActualId) ||
                        (c.Tarea != null && c.Tarea.IdUsuarioAsignado == UsuarioActualId)
                    )
                    .ToList();
            }

            Comentarios = comentarios;
        }

        // =====================================================================
        //                             DELETE
        // =====================================================================
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            Console.WriteLine("=== ELIMINAR COMENTARIO ID = " + id);

            if (User.IsInRole("Empleado"))
            {
                TempData["ErrorMessage"] = "No estás autorizado para eliminar comentarios.";
                return RedirectToPage();
            }

            var ok = await _comentarioApi.DeleteAsync(id);

            if (!ok)
            {
                TempData["ErrorMessage"] = "Error al eliminar el comentario.";
                return RedirectToPage();
            }

            TempData["SuccessMessage"] = "Comentario eliminado correctamente.";
            return RedirectToPage();
        }
    }
}


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

        public async Task OnGetAsync()
        {
            // 1. Obtener ID del usuario autenticado
            var idClaimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(idClaimValue, out int userId);
            UsuarioActualId = userId;

            // 2. Obtener datos base
            var comentarios = (await _comentarioApi.GetAllAsync())?.ToList() ?? new();
            var usuarios = (await _usuarioApiClient.GetAllAsync())?.ToList() ?? new();
            var tareas = (await _tareaApiClient.GetAllAsync())?.ToList() ?? new();
            var proyectos = (await _proyectoApiClient.GetAllAsync())?.ToList() ?? new();

            // 3. Enriquecer comentarios
            foreach (var c in comentarios)
            {
                // Autor
                c.Usuario = usuarios.FirstOrDefault(u => u.Id == c.IdUsuario);

                // Tarea asociada
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
                            IdUsuarioAsignado = tarea.IdUsuarioAsignado,
                            FechaRegistro = tarea.FechaRegistro,
                            UltimaModificacion = tarea.UltimaModificacion
                        };

                        // Proyecto
                        if (tarea.IdProyecto.HasValue)
                        {
                            c.Tarea.ProyectoNombre = proyectos
                                .FirstOrDefault(p => p.IdProyecto == tarea.IdProyecto)?.Nombre;
                        }

                        // Usuario asignado
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

            // 4. Filtrar según rol
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

        // DELETE
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
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

        // ================================================================
        // MÉTODO EXTRA (DE LA RAMA JWT) → LO CONSERVO POR SI ES ÚTIL
        // ================================================================
        private async Task<List<TareaExtendidaDTO>> EnriquecerTareas(List<TareaDTO> tareas, List<UsuarioDTO> allUsuarios)
        {
            var proyectos = (await _proyectoApiClient.GetAllAsync())?.ToList() ?? new();

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
                    IdProyecto = t.IdProyecto,
                    IdUsuarioAsignado = t.IdUsuarioAsignado,
                    FechaRegistro = t.FechaRegistro,
                    UltimaModificacion = t.UltimaModificacion
                };

                if (t.IdProyecto.HasValue)
                {
                    tarea.ProyectoNombre = proyectos
                        .FirstOrDefault(p => p.IdProyecto == t.IdProyecto.Value)?.Nombre;
                }

                if (t.IdUsuarioAsignado.HasValue)
                {
                    var u = allUsuarios.FirstOrDefault(us => us.Id == t.IdUsuarioAsignado.Value);
                    tarea.UsuarioAsignadoNombre = $"{u?.Nombres} {u?.PrimerApellido}";
                }

                lista.Add(tarea);
            }

            return lista;
        }
    }
}
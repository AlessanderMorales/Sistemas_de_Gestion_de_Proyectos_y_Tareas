using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Comentarios;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Proyectos;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public List<TareaExtendidaDTO> TareasEnriquecidas { get; set; } = new();

        public ComentariosIndexModel(
            TareaApiClient tareaApi,
            UsuarioApiClient usuarioApi,
            ProyectoApiClient proyectoApi,
            ComentarioApiClient comentarioApi)
        {
            _tareaApiClient = tareaApi;
            _usuarioApiClient = usuarioApi;
            _proyectoApiClient = proyectoApi; // Asignación corregida
            _comentarioService = comentarioApi;
        }

        public List<ComentarioDTO> Comentarios { get; set; } = new();
        public int UsuarioActualId { get; set; }


        public async Task OnGetAsync()
        {
            var idClaimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(idClaimValue, out var userId))
                UsuarioActualId = userId;

            // 1. Obtener todos los comentarios (inicialmente sin enriquecer, solo IDs)
            var allComentarios = (await _comentarioService.GetAllAsync())?.ToList(); // <-- ¡CORREGIDO!
            if (allComentarios == null || !allComentarios.Any())
            {
                Comentarios = new List<ComentarioDTO>();
                return;
            }

            // 2. Obtener *todos* los usuarios y tareas básicas para el proceso de enriquecimiento
            var allUsuarios = (await _usuarioApiClient.GetAllAsync())?.ToList(); // <-- ¡CORREGIDO!
            if (allUsuarios == null || !allUsuarios.Any())
            {
                // Si no hay usuarios, inicializa allUsuarios como una lista vacía para evitar NREs más adelante
                allUsuarios = new List<UsuarioDTO>();
            }

            var allTareas = (await _tareaApiClient.GetAllAsync())?.ToList(); // <-- ¡CORREGIDO!
            if (allTareas == null || !allTareas.Any())
            {
                // Si no hay tareas, inicializa allTareas como una lista vacía
                allTareas = new List<TareaDTO>();
            }

            // 3. Enriquecer las tareas, esto incluye nombres de proyectos y usuarios asignados
            // ¡¡CRÍTICO!! PASAR allUsuarios a EnriquecerTareas
            TareasEnriquecidas = await EnriquecerTareas(allTareas, allUsuarios);
            if (!TareasEnriquecidas.Any())
            {
                // Manejar escenario: no se pudieron enriquecer tareas.
            }

            // ===================================================================
            // 4. ¡¡CRÍTICO!! Iterar sobre CADA COMENTARIO Y ENRIQUECERLO
            // ===================================================================
            foreach (var comentario in allComentarios)
            {
                // Enriquecer el autor del comentario
                if (comentario.IdUsuario > 0 && allUsuarios.Any()) // Verifica que allUsuarios no esté vacío
                {
                    comentario.Usuario = allUsuarios.FirstOrDefault(u => u.Id == comentario.IdUsuario);
                }

                // Enriquecer la tarea relacionada al comentario
                if (comentario.IdTarea.HasValue && comentario.IdTarea.Value > 0 && TareasEnriquecidas.Any())
                {
                    comentario.Tarea = TareasEnriquecidas.FirstOrDefault(t => t.Id == comentario.IdTarea.Value);
                }

                // Enriquecer el destinatario directo del comentario (si aplica)
                if (comentario.IdDestinatario.HasValue && comentario.IdDestinatario.Value > 0 && allUsuarios.Any())
                {
                    comentario.Destinatario = allUsuarios.FirstOrDefault(u => u.Id == comentario.IdDestinatario.Value);
                }
            }


            // 5. Aplicar el filtro de roles DESPUÉS de enriquecer los comentarios
            if (User.IsInRole("Empleado"))
            {
                allComentarios = allComentarios
                    .Where(c =>
                        c.IdUsuario == UsuarioActualId ||
                        (c.IdDestinatario.HasValue && c.IdDestinatario.Value == UsuarioActualId) ||
                        (c.Tarea != null && c.Tarea.IdUsuarioAsignado.HasValue && c.Tarea.IdUsuarioAsignado.Value == UsuarioActualId)
                    )
                    .ToList();
            }

            Comentarios = allComentarios;
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            Console.WriteLine("=== ELIMINAR COMENTARIO ID = " + id);

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

        // =========================================================================
        // MÉTODO ENRIQUECERTAREAS MODIFICADO PARA RECIBIR allUsuarios
        // =========================================================================
        private async Task<List<TareaExtendidaDTO>> EnriquecerTareas(List<TareaDTO> tareas, List<UsuarioDTO> allUsuarios)
        {
            var proyectos = (await _proyectoApiClient.GetAllAsync())?.ToList(); // <-- ¡CORREGIDO!
            if (proyectos == null || !proyectos.Any())
            {
                proyectos = new List<ProyectoDTO>(); // Inicializa como lista vacía si no hay proyectos
            }

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

                // nombre del proyecto
                if (t.IdProyecto.HasValue && t.IdProyecto.Value > 0 && proyectos.Any())
                {
                    tarea.ProyectoNombre = proyectos.FirstOrDefault(p => p.IdProyecto == t.IdProyecto.Value)?.Nombre;
                }

                // usuario asignado a la tarea
                if (tarea.IdUsuarioAsignado.HasValue && tarea.IdUsuarioAsignado.Value > 0 && allUsuarios.Any())
                {
                    var u = allUsuarios.FirstOrDefault(us => us.Id == tarea.IdUsuarioAsignado.Value);
                    tarea.UsuarioAsignadoNombre = $"{u?.Nombres} {u?.PrimerApellido}";
                }

                lista.Add(tarea);
            }

            return lista;
        }
    }
}
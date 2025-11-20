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

            var allComentarios = (await _comentarioService.GetAllAsync())?.ToList();
            if (allComentarios == null || !allComentarios.Any())
            {
                Comentarios = new List<ComentarioDTO>();
                return;
            }

            var allUsuarios = (await _usuarioApiClient.GetAllAsync())?.ToList();
            if (allUsuarios == null || !allUsuarios.Any())
                allUsuarios = new List<UsuarioDTO>();

            var allTareas = (await _tareaApiClient.GetAllAsync())?.ToList();
            if (allTareas == null || !allTareas.Any())
                allTareas = new List<TareaDTO>();

            TareasEnriquecidas = await EnriquecerTareas(allTareas, allUsuarios);

            foreach (var comentario in allComentarios)
            {
                if (comentario.IdUsuario > 0 && allUsuarios.Any())
                {
                    comentario.Usuario = allUsuarios.FirstOrDefault(u => u.Id == comentario.IdUsuario);
                }

                if (comentario.IdTarea.HasValue && comentario.IdTarea.Value > 0 && TareasEnriquecidas.Any())
                {
                    comentario.Tarea = TareasEnriquecidas.FirstOrDefault(t => t.Id == comentario.IdTarea.Value);
                }

                if (comentario.IdDestinatario.HasValue && comentario.IdDestinatario.Value > 0 && allUsuarios.Any())
                {
                    comentario.Destinatario = allUsuarios.FirstOrDefault(u => u.Id == comentario.IdDestinatario.Value);
                }
            }

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

        private async Task<List<TareaExtendidaDTO>> EnriquecerTareas(List<TareaDTO> tareas, List<UsuarioDTO> allUsuarios)
        {
            var proyectos = (await _proyectoApiClient.GetAllAsync())?.ToList();
            if (proyectos == null || !proyectos.Any())
                proyectos = new List<ProyectoDTO>();

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

                if (t.IdProyecto.HasValue && t.IdProyecto.Value > 0 && proyectos.Any())
                {
                    tarea.ProyectoNombre = proyectos.FirstOrDefault(p => p.IdProyecto == t.IdProyecto.Value)?.Nombre;
                }

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

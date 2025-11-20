using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
﻿using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Proyectos;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Comentarios;
using System.Security.Claims;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Tareas
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly TareaApiClient _tareaApi;
        private readonly UsuarioApiClient _usuarioApi;
        private readonly ProyectoApiClient _proyectoApi;
        private readonly ComentarioApiClient _comentarioApi;

        public List<TareaExtendidaDTO> Tareas { get; set; } = new();

        public IndexModel(
            TareaApiClient tareaApi,
            UsuarioApiClient usuarioApi,
            ProyectoApiClient proyectoApi,
            ComentarioApiClient comentarioApi)
        {
            _tareaApi = tareaApi;
            _usuarioApi = usuarioApi;
            _proyectoApi = proyectoApi;
            _comentarioApi = comentarioApi;
        }

        public async Task OnGetAsync()
        {
            // Si es empleado → solo ver tareas asignadas
            if (User.IsInRole("Empleado"))
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(idClaim, out var usuarioId))
                {
                    var tareas = await _tareaApi.GetByUsuarioAsync(usuarioId);
                    Tareas = await EnriquecerTareas(tareas);
                    // Ordenar por ID descendente (más nuevas primero)
                    Tareas = Tareas.OrderByDescending(t => t.Id).ToList();
                    return;
                }
            }

            // Si es jefe/admin → ver todas las tareas
            var todas = await _tareaApi.GetAllAsync();
            Tareas = await EnriquecerTareas(todas);
            // Ordenar por ID descendente (más nuevas primero)
            Tareas = Tareas.OrderByDescending(t => t.Id).ToList();
        }

        // ============================================================
        // 🗑 ELIMINAR TAREA + ELIMINACIÓN EN CASCADA DE COMENTARIOS
        // ============================================================
        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (User.IsInRole("Empleado"))
            {
                TempData["ErrorMessage"] = "No estás autorizado para eliminar tareas.";
                return RedirectToPage();
            }

            bool ok = true;

            try
            {
                // 1️⃣ Obtener TODOS los comentarios para filtrarlos por IdTarea
                var todosComentarios = (await _comentarioApi.GetAllAsync())
                                        ?.ToList() ?? new List<ComentarioDTO>();

                // 2️⃣ Filtrar comentarios que dependen de esta tarea
                var comentariosDeTarea = todosComentarios
                    .Where(c => c.IdTarea == id)
                    .ToList();

                // 3️⃣ Eliminar (estado = 0) cada comentario
                foreach (var c in comentariosDeTarea)
                {
                    await _comentarioApi.DeleteAsync(c.IdComentario);
                }

                // 4️⃣ Eliminar la tarea (estado = 0)
                ok = await _tareaApi.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR en eliminación en cascada (tarea→comentarios): " + ex.Message);
                ok = false;
            }

            TempData[ok ? "SuccessMessage" : "ErrorMessage"] =
                ok
                ? "Tarea y sus comentarios fueron eliminados correctamente."
                : "Error al eliminar la tarea y sus comentarios.";

            return RedirectToPage();
        }

        // ============================================================
        // 🧠 ENRIQUECER TAREAS
        // ============================================================
        private async Task<List<TareaExtendidaDTO>> EnriquecerTareas(List<TareaDTO> tareas)
        {
            var usuarios = await _usuarioApi.GetAllAsync();
            var proyectos = await _proyectoApi.GetAllAsync();

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
                    ProyectoNombre = proyectos.FirstOrDefault(p => p.IdProyecto == t.IdProyecto)?.Nombre
                };

                // Usuarios asignados a la tarea
                var asignados = await _tareaApi.GetUsuariosAsignadosAsync(t.Id);

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
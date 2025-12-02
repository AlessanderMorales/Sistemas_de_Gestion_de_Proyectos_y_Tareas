using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Proyectos;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Comentarios;
using System.Security.Claims;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Proyectos
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ProyectoApiClient _proyectoApi;
        private readonly TareaApiClient _tareaApi;
        private readonly ComentarioApiClient _comentarioApi;

        public List<ProyectoDTO> Proyectos { get; set; } = new();

        public IndexModel(
            ProyectoApiClient proyectoApi,
            TareaApiClient tareaApi,
            ComentarioApiClient comentarioApi)
        {
            _proyectoApi = proyectoApi;
            _tareaApi = tareaApi;
            _comentarioApi = comentarioApi;
        }

        public async Task OnGetAsync()
        {
            if (User.IsInRole("Empleado"))
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(idClaim, out var usuarioId))
                {
                    Proyectos = await _proyectoApi.GetByUsuarioAsync(usuarioId);
                    // Ordenar por IdProyecto descendente (más nuevos primero)
                    Proyectos = Proyectos.OrderByDescending(p => p.IdProyecto).ToList();
                    return;
                }
            }

            Proyectos = await _proyectoApi.GetAllAsync();
            // Ordenar por IdProyecto descendente (más nuevos primero)
            Proyectos = Proyectos.OrderByDescending(p => p.IdProyecto).ToList();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (User.IsInRole("Empleado"))
            {
                TempData["ErrorMessage"] = "No estás autorizado para eliminar proyectos.";
                return RedirectToPage("./Index");
            }

            bool ok = true;

            try
            {
                // 1️⃣ Obtener TODAS las tareas del proyecto
                var tareas = await _tareaApi.GetByProyectoAsync(id)
                                ?? new List<TareaDTO>();

                // 2️⃣ Obtener TODOS los comentarios de todos
                var todosComentarios = (await _comentarioApi.GetAllAsync())?.ToList() ?? new List<ComentarioDTO>();

                foreach (var tarea in tareas)
                {
                    // 3️⃣ Filtrar comentarios de la tarea
                    var comentarios = todosComentarios
                                        .Where(c => c.IdTarea == tarea.Id)
                                        .ToList();

                    // 4️⃣ Eliminar comentarios (cambiar estado)
                    foreach (var c in comentarios)
                    {
                        await _comentarioApi.DeleteAsync(c.IdComentario);
                    }

                    // 5️⃣ Eliminar (desactivar) la tarea
                    await _tareaApi.DeleteAsync(tarea.Id);
                }

                // 6️⃣ Finalmente eliminar el proyecto
                ok = await _proyectoApi.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR en eliminación en cascada: " + ex.Message);
                ok = false;
            }

            TempData[ok ? "SuccessMessage" : "ErrorMessage"] =
                ok
                ? "Proyecto y todas sus tareas/comentarios fueron eliminados correctamente."
                : "Error al eliminar el proyecto y sus dependencias.";

            return RedirectToPage("./Index");
        }
    }
}

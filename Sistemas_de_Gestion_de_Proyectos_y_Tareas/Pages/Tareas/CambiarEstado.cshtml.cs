using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Tareas
{
    [Authorize]
    public class CambiarEstadoModel : PageModel
    {
        private readonly TareaApiClient _tareaApi;

        public CambiarEstadoModel(TareaApiClient tareaApi)
        {
            _tareaApi = tareaApi;
        }

        [BindProperty] public int TareaId { get; set; }
        [BindProperty] public string NuevoStatus { get; set; }
        public TareaDTO? Tarea { get; set; }
        public SelectList StatusDisponibles { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            TareaId = id;
            Tarea = await _tareaApi.GetByIdAsync(id);

            if (Tarea == null)
            {
                TempData["ErrorMessage"] = "Tarea no encontrada.";
                return RedirectToPage("Index");
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (User.IsInRole("Empleado"))
            {
                var asignados = await _tareaApi.GetUsuariosAsignadosAsync(id);
                if (!asignados.Contains(userId))
                {
                    TempData["ErrorMessage"] = "No tienes permiso para cambiar el estado.";
                    return RedirectToPage("Index");
                }
            }

            StatusDisponibles = new SelectList(new[]
            {
                new { Value = "SinIniciar", Text = "Sin Iniciar" },
                new { Value = "EnProgreso", Text = "En Progreso" },
                new { Value = "Completada", Text = "Completada" }
            }, "Value", "Text", Tarea.Estado);

            NuevoStatus = Tarea.Estado.ToString();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var tarea = await _tareaApi.GetByIdAsync(TareaId);
            if (tarea == null)
            {
                TempData["ErrorMessage"] = "Tarea no encontrada.";
                return RedirectToPage("Index");
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (User.IsInRole("Empleado"))
            {
                var asignados = await _tareaApi.GetUsuariosAsignadosAsync(TareaId);
                if (!asignados.Contains(userId))
                {
                    TempData["ErrorMessage"] = "No tienes permiso para cambiar el estado.";
                    return RedirectToPage("Index");
                }
            }

            var ok = await _tareaApi.CambiarEstadoAsync(TareaId,
                new CambiarEstadoTareaDTO { NuevoEstado = NuevoStatus });

            if (!ok)
            {
                TempData["ErrorMessage"] = "No se pudo actualizar el estado.";
                return RedirectToPage("Index");
            }

            TempData["SuccessMessage"] = "Estado actualizado correctamente.";
            return RedirectToPage("Index");
        }
    }
}

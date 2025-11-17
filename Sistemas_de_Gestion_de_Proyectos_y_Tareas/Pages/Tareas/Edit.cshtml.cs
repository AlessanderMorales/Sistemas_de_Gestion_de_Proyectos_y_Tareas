using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Proyectos;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Tareas
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly TareaApiClient _tareaApi;
        private readonly ProyectoApiClient _proyectoApi;

        [BindProperty]
        public TareaDTO Tarea { get; set; } = new();

        public List<SelectListItem> ProyectosDisponibles { get; set; } = new();

        public EditModel(TareaApiClient tareaApi, ProyectoApiClient proyectoApi)
        {
            _tareaApi = tareaApi;
            _proyectoApi = proyectoApi;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // 1. Obtener tarea desde API
            var t = await _tareaApi.GetAsync(id);
            if (t == null)
                return RedirectToPage("Index");

            Tarea = t;

            // 2. Obtener proyectos desde API
            var proyectos = await _proyectoApi.GetAllAsync();
            ProyectosDisponibles = proyectos
                .Select(p => new SelectListItem
                {
                    Value = p.IdProyecto.ToString(),
                    Text = p.Nombre,
                    Selected = (p.IdProyecto == Tarea.IdProyecto)
                })
                .ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var proyectos = await _proyectoApi.GetAllAsync();
                ProyectosDisponibles = proyectos
                    .Select(p => new SelectListItem
                    {
                        Value = p.IdProyecto.ToString(),
                        Text = p.Nombre,
                        Selected = (p.IdProyecto == Tarea.IdProyecto)
                    })
                    .ToList();

                return Page();
            }

            // limpiar espacios
            Tarea.Titulo = Trim(Tarea.Titulo);
            Tarea.Descripcion = Trim(Tarea.Descripcion);

            var ok = await _tareaApi.UpdateAsync(Tarea.Id, Tarea);

            if (!ok)
            {
                TempData["ErrorMessage"] = "Error al actualizar la tarea.";
                return RedirectToPage("Index");
            }

            TempData["SuccessMessage"] = "Tarea actualizada correctamente.";
            return RedirectToPage("Index");
        }

        private string Trim(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            input = input.Trim();
            return System.Text.RegularExpressions.Regex.Replace(input, @"\s+", " ");
        }
    }
}

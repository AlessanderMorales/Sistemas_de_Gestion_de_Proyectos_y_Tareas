using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Proyectos;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Proyectos
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ProyectoApiClient _proyectoApi;

        [BindProperty]
        public ProyectoDTO Proyecto { get; set; } = new();

        public EditModel(ProyectoApiClient proyectoApi)
        {
            _proyectoApi = proyectoApi;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var proyecto = await _proyectoApi.GetByIdAsync(id.Value);

            if (proyecto == null)
                return NotFound();

            Proyecto = proyecto;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Trim automático
            if (!string.IsNullOrEmpty(Proyecto.Nombre))
                Proyecto.Nombre = TrimExtraSpaces(Proyecto.Nombre);

            if (!string.IsNullOrEmpty(Proyecto.Descripcion))
                Proyecto.Descripcion = TrimExtraSpaces(Proyecto.Descripcion);

            var ok = await _proyectoApi.UpdateAsync(Proyecto.IdProyecto, Proyecto);

            if (!ok)
            {
                TempData["ErrorMessage"] = "No se pudo actualizar el proyecto.";
                return Page();
            }

            TempData["SuccessMessage"] = "Proyecto actualizado correctamente.";
            return RedirectToPage("./Index");
        }

        private string TrimExtraSpaces(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            input = input.Trim();
            return System.Text.RegularExpressions.Regex.Replace(input, @"\s+", " ");
        }
    }
}

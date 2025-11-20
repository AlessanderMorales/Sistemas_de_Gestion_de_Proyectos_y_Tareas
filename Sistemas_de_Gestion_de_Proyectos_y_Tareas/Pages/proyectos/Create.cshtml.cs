using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Proyectos;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Proyectos
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ProyectoApiClient _proyectoApi;

        [BindProperty]
        public ProyectoDTO Proyecto { get; set; } = new();

        [TempData]
        public string? MensajeExito { get; set; }

        [TempData]
        public string? MensajeError { get; set; }

        public CreateModel(ProyectoApiClient proyectoApi)
        {
            _proyectoApi = proyectoApi;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                if (!string.IsNullOrEmpty(Proyecto.Nombre))
                    Proyecto.Nombre = TrimExtraSpaces(Proyecto.Nombre);

                if (!string.IsNullOrEmpty(Proyecto.Descripcion))
                    Proyecto.Descripcion = TrimExtraSpaces(Proyecto.Descripcion);

                var ok = await _proyectoApi.CreateAsync(Proyecto);

                if (!ok)
                {
                    TempData["ErrorMessage"] = "Error al comunicarse con el servidor.";
                    return Page();
                }

                TempData["SuccessMessage"] = "Proyecto creado exitosamente.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al crear el proyecto: {ex.Message}";
                return RedirectToPage("./Index");
            }
        }

        private string TrimExtraSpaces(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            input = input.Trim();
            return System.Text.RegularExpressions.Regex.Replace(input, @"\s+", " ");
        }
    }
}

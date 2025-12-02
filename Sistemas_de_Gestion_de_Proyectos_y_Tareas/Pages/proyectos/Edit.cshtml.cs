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
            {
                return Page();
            }

            try
            {
                // Validar y sanitizar nombre
                if (!string.IsNullOrEmpty(Proyecto.Nombre))
                {
                    Proyecto.Nombre = TrimExtraSpaces(Proyecto.Nombre);

                    if (ContienePatroneseligrosos(Proyecto.Nombre))
                    {
                        ModelState.AddModelError("Proyecto.Nombre", "?? El nombre contiene caracteres no permitidos. Evite usar: $ % ^ & * ( ) { } [ ] \\ | < > ' \" ; etc.");
                        return Page();
                    }
                }

                // Validar y sanitizar descripción
                if (!string.IsNullOrEmpty(Proyecto.Descripcion))
                {
                    Proyecto.Descripcion = TrimExtraSpaces(Proyecto.Descripcion);

                    if (ContienePatroneseligrosos(Proyecto.Descripcion))
                    {
                        ModelState.AddModelError("Proyecto.Descripcion", "?? La descripción contiene caracteres no permitidos. Evite usar: $ % ^ & * ( ) { } [ ] \\ | < > ' \" ; etc.");
                        return Page();
                    }
                }

                // Validar fechas
                if (Proyecto.FechaInicio.HasValue && Proyecto.FechaFin.HasValue)
                {
                    if (Proyecto.FechaInicio.Value.Date < DateTime.Now.Date)
                    {
                        ModelState.AddModelError("Proyecto.FechaInicio", "?? La fecha de inicio no puede ser anterior a hoy.");
                        return Page();
                    }

                    if (Proyecto.FechaFin.Value.Date < DateTime.Now.Date)
                    {
                        ModelState.AddModelError("Proyecto.FechaFin", "?? La fecha de finalización no puede ser anterior a hoy.");
                        return Page();
                    }

                    if (Proyecto.FechaFin.Value.Date < Proyecto.FechaInicio.Value.Date)
                    {
                        ModelState.AddModelError("Proyecto.FechaFin", "?? La fecha de finalización debe ser posterior a la fecha de inicio.");
                        return Page();
                    }

                    if (Proyecto.FechaFin.Value.Date == Proyecto.FechaInicio.Value.Date)
                    {
                        ModelState.AddModelError("Proyecto.FechaFin", "?? Las fechas de inicio y fin no pueden ser iguales.");
                        return Page();
                    }
                }

                // Llamar a la API con el nuevo método que retorna errores específicos
                var (success, errorMessage) = await _proyectoApi.UpdateAsync(Proyecto.IdProyecto, Proyecto);

                if (!success)
                {
                    // Mostrar el error específico en la página
                    ModelState.AddModelError(string.Empty, errorMessage ?? "? Error al actualizar el proyecto.");
                    return Page(); // NO redirige, se queda en la página para mostrar el error
                }

                TempData["SuccessMessage"] = "? Proyecto actualizado correctamente.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                // Capturar excepciones locales y mostrarlas en la UI
                ModelState.AddModelError(string.Empty, $"? Error inesperado: {ex.Message}");
                return Page();
            }
        }

        private string TrimExtraSpaces(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            input = input.Trim();
            return System.Text.RegularExpressions.Regex.Replace(input, @"\s+", " ");
        }

        private bool ContienePatroneseligrosos(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            var patronesPeligrosos = new[]
            {
                @"(\bOR\b|\bAND\b).*=",
                @"['""`;]|--|\/\*|\*\/",
                @"\b(EXEC|EXECUTE|DROP|DELETE|UPDATE|INSERT|SELECT.*FROM|UNION.*SELECT)\b",
                @"<script",
                @"javascript:",
                @"onerror\s*=",
                @"onload\s*=",
                @"<iframe",
                @"[$%^&*(){}[\]\\|]"  // Caracteres especiales peligrosos
            };

            foreach (var patron in patronesPeligrosos)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(input, patron, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

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
                TempData["ErrorMessage"] = "? Por favor corrija los errores en el formulario antes de continuar.";
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
                        ModelState.AddModelError("Proyecto.Nombre", "?? El nombre contiene caracteres no permitidos.");
                        TempData["ErrorMessage"] = "El nombre del proyecto contiene caracteres o patrones no permitidos.";
                        return Page();
                    }
                }

                // Validar y sanitizar descripción
                if (!string.IsNullOrEmpty(Proyecto.Descripcion))
                {
                    Proyecto.Descripcion = TrimExtraSpaces(Proyecto.Descripcion);

                    if (ContienePatroneseligrosos(Proyecto.Descripcion))
                    {
                        ModelState.AddModelError("Proyecto.Descripcion", "?? La descripción contiene caracteres no permitidos.");
                        TempData["ErrorMessage"] = "La descripción contiene caracteres o patrones no permitidos.";
                        return Page();
                    }
                }

                // Validar fechas
                if (Proyecto.FechaInicio.HasValue && Proyecto.FechaFin.HasValue)
                {
                    if (Proyecto.FechaInicio.Value.Date < DateTime.Now.Date)
                    {
                        ModelState.AddModelError("Proyecto.FechaInicio", "? La fecha de inicio no puede ser anterior a hoy.");
                        TempData["ErrorMessage"] = "La fecha de inicio no puede ser anterior a la fecha actual.";
                        return Page();
                    }

                    if (Proyecto.FechaFin.Value.Date < DateTime.Now.Date)
                    {
                        ModelState.AddModelError("Proyecto.FechaFin", "? La fecha de finalización no puede ser anterior a hoy.");
                        TempData["ErrorMessage"] = "La fecha de finalización no puede ser anterior a la fecha actual.";
                        return Page();
                    }

                    if (Proyecto.FechaFin.Value.Date < Proyecto.FechaInicio.Value.Date)
                    {
                        ModelState.AddModelError("Proyecto.FechaFin", "? La fecha de finalización debe ser posterior a la fecha de inicio.");
                        TempData["ErrorMessage"] = "La fecha de finalización no puede ser anterior a la fecha de inicio.";
                        return Page();
                    }

                    if (Proyecto.FechaFin.Value.Date == Proyecto.FechaInicio.Value.Date)
                    {
                        ModelState.AddModelError("Proyecto.FechaFin", "? Las fechas de inicio y fin no pueden ser iguales.");
                        TempData["ErrorMessage"] = "La fecha de finalización debe ser diferente a la fecha de inicio.";
                        return Page();
                    }
                }

                var ok = await _proyectoApi.UpdateAsync(Proyecto.IdProyecto, Proyecto);

                if (!ok)
                {
                    TempData["ErrorMessage"] = "? No se pudo actualizar el proyecto.";
                    return Page();
                }

                TempData["SuccessMessage"] = "? Proyecto actualizado correctamente.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.ToLower();

                if (errorMsg.Contains("fecha") || errorMsg.Contains("date"))
                {
                    TempData["ErrorMessage"] = $"? Error con las fechas: {ex.Message}";
                }
                else if (errorMsg.Contains("caracteres") || errorMsg.Contains("patrones") || errorMsg.Contains("sql"))
                {
                    TempData["ErrorMessage"] = $"?? Validación de seguridad: {ex.Message}";
                }
                else
                {
                    TempData["ErrorMessage"] = $"? Error al actualizar el proyecto: {ex.Message}";
                }

                return RedirectToPage("./Index");
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

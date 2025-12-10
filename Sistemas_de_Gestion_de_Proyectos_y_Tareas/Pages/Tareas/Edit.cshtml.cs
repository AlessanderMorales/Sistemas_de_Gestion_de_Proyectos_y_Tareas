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
            var t = await _tareaApi.GetByIdAsync(id);
            if (t == null)
                return RedirectToPage("Index");

            Tarea = t;

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
                TempData["ErrorMessage"] = "❌ Por favor corrija los errores en el formulario antes de continuar.";
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

            try
            {
                Tarea.Titulo = Trim(Tarea.Titulo);
                Tarea.Descripcion = Trim(Tarea.Descripcion ?? "");

                // Validar título
                if (!string.IsNullOrEmpty(Tarea.Titulo) && ContienePatroneseligrosos(Tarea.Titulo))
                {
                    ModelState.AddModelError("Tarea.Titulo", "⚠️ El título contiene caracteres no permitidos.");
                    TempData["ErrorMessage"] = "El título contiene caracteres o patrones no permitidos.";
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

                // Validar descripción
                if (!string.IsNullOrEmpty(Tarea.Descripcion) && ContienePatroneseligrosos(Tarea.Descripcion))
                {
                    ModelState.AddModelError("Tarea.Descripcion", "⚠️ La descripción contiene caracteres no permitidos.");
                    TempData["ErrorMessage"] = "La descripción contiene caracteres o patrones no permitidos.";
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

                var ok = await _tareaApi.UpdateAsync(Tarea.Id, Tarea);

                if (!ok)
                {
                    TempData["ErrorMessage"] = "❌ Error al actualizar la tarea.";
                    return RedirectToPage("Index");
                }

                TempData["SuccessMessage"] = "✅ Tarea actualizada correctamente.";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.ToLower();

                if (errorMsg.Contains("caracteres") || errorMsg.Contains("patrones") || errorMsg.Contains("sql"))
                {
                    TempData["ErrorMessage"] = $"⚠️ Validación de seguridad: {ex.Message}";
                }
                else
                {
                    TempData["ErrorMessage"] = $"❌ Error al actualizar la tarea: {ex.Message}";
                }

                return RedirectToPage("Index");
            }
        }

        private string Trim(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

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

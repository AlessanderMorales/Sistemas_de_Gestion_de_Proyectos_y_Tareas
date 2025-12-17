using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Proyectos;
using System.Linq;
using System.Threading.Tasks;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Tareas
{
    [Authorize(Policy = "OnlyJefe")]
    public class CreateModel : PageModel
    {
        private readonly TareaApiClient _tareaApi;
        private readonly ProyectoApiClient _proyectoApi;

        public CreateModel(TareaApiClient tareaApi, ProyectoApiClient proyectoApi)
        {
            _tareaApi = tareaApi;
            _proyectoApi = proyectoApi;
        }

        [BindProperty]
        public TareaDTO Tarea { get; set; } = new();

        public List<ProyectoDTO> ProyectosDisponibles { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            ProyectosDisponibles = await _proyectoApi.GetAllAsync();

            if (!ProyectosDisponibles.Any())
            {
                TempData["ErrorMessage"] = "No hay proyectos disponibles.";
                return RedirectToPage("Index");
            }

            Tarea.IdProyecto = ProyectosDisponibles.First().IdProyecto;
            Tarea.Prioridad = "Media";
            Tarea.Status = "SinIniciar";

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Por favor corrija los errores en el formulario antes de continuar.";
                ProyectosDisponibles = await _proyectoApi.GetAllAsync();
                return Page();
            }

            try
            {
                if (!string.IsNullOrEmpty(Tarea.Titulo))
                {
                    Tarea.Titulo = TrimExtraSpaces(Tarea.Titulo);

                    if (ContienePatroneseligrosos(Tarea.Titulo))
                    {
                        ModelState.AddModelError("Tarea.Titulo", "El título contiene caracteres no permitidos.");
                        TempData["ErrorMessage"] = "El título contiene caracteres o patrones no permitidos.";
                        ProyectosDisponibles = await _proyectoApi.GetAllAsync();
                        return Page();
                    }
                }

                if (!string.IsNullOrEmpty(Tarea.Descripcion))
                {
                    Tarea.Descripcion = TrimExtraSpaces(Tarea.Descripcion);

                    if (ContienePatroneseligrosos(Tarea.Descripcion))
                    {
                        ModelState.AddModelError("Tarea.Descripcion", "La descripción contiene caracteres no permitidos.");
                        TempData["ErrorMessage"] = "La descripción contiene caracteres o patrones no permitidos.";
                        ProyectosDisponibles = await _proyectoApi.GetAllAsync();
                        return Page();
                    }
                }

                Tarea.Status = "SinIniciar";

                var ok = await _tareaApi.CreateAsync(Tarea);

                if (!ok)
                {
                    TempData["ErrorMessage"] = "Error al comunicarse con el servidor. Por favor, intente nuevamente.";
                    ProyectosDisponibles = await _proyectoApi.GetAllAsync();
                    return Page();
                }

                TempData["SuccessMessage"] = "Tarea creada exitosamente.";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.ToLower();

                if (errorMsg.Contains("caracteres") || errorMsg.Contains("patrones") || errorMsg.Contains("sql"))
                {
                    TempData["ErrorMessage"] = $"Validación de seguridad: {ex.Message}";
                }
                else
                {
                    TempData["ErrorMessage"] = $"Error al crear la tarea: {ex.Message}";
                }

                ProyectosDisponibles = await _proyectoApi.GetAllAsync();
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
                @"[$%^&*(){}[\]\\|]"
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

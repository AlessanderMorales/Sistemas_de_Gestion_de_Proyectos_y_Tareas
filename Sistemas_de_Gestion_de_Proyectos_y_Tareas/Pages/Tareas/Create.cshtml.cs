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

        // 🔹 DTO correcto
        [BindProperty]
        public TareaDTO Tarea { get; set; } = new();

        public List<ProyectoDTO> ProyectosDisponibles { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ProyectosDisponibles = await _proyectoApi.GetAllAsync();

            if (!ProyectosDisponibles.Any())
            {
                TempData["ErrorMessage"] = "No hay proyectos disponibles.";
                return RedirectToPage("Index");
            }

            // Valores por defecto
            Tarea.IdProyecto = ProyectosDisponibles.First().IdProyecto;
            Tarea.Prioridad = "Media";
            Tarea.Status = "SinIniciar";

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ProyectosDisponibles = await _proyectoApi.GetAllAsync();
                return Page();
            }

            // Siempre inicia "SinIniciar"
            Tarea.Status = "SinIniciar";

            var ok = await _tareaApi.CreateAsync(Tarea);

            if (!ok)
            {
                TempData["ErrorMessage"] = "Error al crear la tarea.";
                return RedirectToPage("Index");
            }

            TempData["SuccessMessage"] = "Tarea creada exitosamente.";
            return RedirectToPage("Index");
        }
    }
}

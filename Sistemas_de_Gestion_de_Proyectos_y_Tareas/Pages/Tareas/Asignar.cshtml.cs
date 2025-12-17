using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Tareas
{
    [Authorize(Policy = "OnlyJefe")]
    public class AsignarModel : PageModel
    {
        private readonly TareaApiClient _tareaApi;
        private readonly UsuarioApiClient _usuarioApi;

        public List<UsuarioDTO> UsuariosActualmenteAsignados { get; set; } = new();
        public List<UsuarioDTO> UsuariosDisponibles { get; set; } = new();

        [BindProperty]
        public int TareaId { get; set; }

        [BindProperty]
        public int UsuarioId { get; set; }

        public string NombreTarea { get; set; } = "";

        public AsignarModel(TareaApiClient tareaApi, UsuarioApiClient usuarioApi)
        {
            _tareaApi = tareaApi;
            _usuarioApi = usuarioApi;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            TareaId = id;

            var tarea = await _tareaApi.GetByIdAsync(id);
            NombreTarea = tarea.Titulo ?? "Tarea desconocida";

            var asignadosIds = await _tareaApi.GetUsuariosAsignadosAsync(id);

            var todos = await _usuarioApi.GetAllAsync();

            UsuariosActualmenteAsignados = todos
                .Where(u => asignadosIds.Contains(u.Id))
                .ToList();

            var todasLasTareas = await _tareaApi.GetAllAsync();

            var usuariosOcupados = new HashSet<int>();

            foreach (var t in todasLasTareas)
            {
                if (t.Id == id)
                    continue;

                bool tareaCompletada = (t.Status == "Completada");

                if (tareaCompletada)
                    continue;

                var uids = await _tareaApi.GetUsuariosAsignadosAsync(t.Id);
                foreach (var uid in uids)
                    usuariosOcupados.Add(uid);
            }

            UsuariosDisponibles = todos
                .Where(u => u.Rol != "SuperAdmin")
                .Where(u => !usuariosOcupados.Contains(u.Id))
                .ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (UsuarioId <= 0)
            {
                TempData["ErrorMessage"] = "Debe seleccionar un usuario.";
                return RedirectToPage(new { id = TareaId });
            }

            var dto = new AsignarUsuariosDTO
            {
                TareaId = TareaId,
                UsuariosIds = new List<int> { UsuarioId }
            };

            var ok = await _tareaApi.AsignarUsuariosAsync(dto);

            if (!ok)
            {
                TempData["ErrorMessage"] = "Error al asignar usuario.";
                return RedirectToPage(new { id = TareaId });
            }

            TempData["SuccessMessage"] = "Usuario asignado correctamente.";
            return RedirectToPage("Index");
        }
    }

    public class UsuarioViewModel
    {
        public int Id { get; set; }
        public string Nombres { get; set; } = "";
        public string PrimerApellido { get; set; } = "";
    }
}

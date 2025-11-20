using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas;
using System.Security.Claims;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Tareas
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly TareaApiClient _tareaApi;
        private readonly UsuarioApiClient _usuarioApi;
        private readonly ProyectoApiClient _proyectoApi;

        public List<TareaExtendidaDTO> Tareas { get; set; } = new();

        public IndexModel(
            TareaApiClient tareaApi,
            UsuarioApiClient usuarioApi,
            ProyectoApiClient proyectoApi)
        {
            _tareaApi = tareaApi;
            _usuarioApi = usuarioApi;
            _proyectoApi = proyectoApi;
        }

        public async Task OnGetAsync()
        {
            if (User.IsInRole("Empleado"))
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(idClaim, out var usuarioId))
                {
                    var tareas = await _tareaApi.GetByUsuarioAsync(usuarioId);
                    Tareas = await EnriquecerTareas(tareas);
                    return;
                }
            }

            var todas = await _tareaApi.GetAllAsync();
            Tareas = await EnriquecerTareas(todas);
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (User.IsInRole("Empleado"))
            {
                TempData["ErrorMessage"] = "No estás autorizado para eliminar tareas.";
                return RedirectToPage();
            }

            var ok = await _tareaApi.DeleteAsync(id);

            if (!ok)
            {
                TempData["ErrorMessage"] = "Error al eliminar la tarea.";
                return RedirectToPage();
            }

            TempData["SuccessMessage"] = "Tarea eliminada correctamente.";
            return RedirectToPage();
        }

        private async Task<List<TareaExtendidaDTO>> EnriquecerTareas(List<TareaDTO> tareas)
        {
            var usuarios = await _usuarioApi.GetAllAsync();
            var proyectos = await _proyectoApi.GetAllAsync();
            var lista = new List<TareaExtendidaDTO>();

            foreach (var t in tareas)
            {
                var tarea = new TareaExtendidaDTO
                {
                    Id = t.Id,
                    Titulo = t.Titulo,
                    Descripcion = t.Descripcion,
                    Prioridad = t.Prioridad,
                    Status = t.Status,
                    IdProyecto = t.IdProyecto,
                    ProyectoNombre = proyectos.FirstOrDefault(p => p.IdProyecto == t.IdProyecto)?.Nombre
                };

                var asignados = await _tareaApi.GetUsuariosAsignadosAsync(t.Id);
                if (asignados.Any())
                {
                    var u = usuarios.FirstOrDefault(us => us.Id == asignados.First());
                    tarea.UsuarioAsignadoNombre = $"{u?.Nombres} {u?.PrimerApellido}";
                }

                lista.Add(tarea);
            }

            return lista;
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Proyectos;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Proyectos
{
    [Authorize(Policy = "OnlyJefe")]
    public class GestionarUsuariosModel : PageModel
    {
        private readonly ProyectoApiClient _proyectoApi;
        private readonly UsuarioApiClient _usuarioApi;

        public ProyectoDTO Proyecto { get; set; } = default!;
        public List<UsuarioDTO> UsuariosAsignados { get; set; } = new();
        public List<UsuarioDTO> UsuariosDisponibles { get; set; } = new();

        public GestionarUsuariosModel(
            ProyectoApiClient proyectoApi,
            UsuarioApiClient usuarioApi)
        {
            _proyectoApi = proyectoApi;
            _usuarioApi = usuarioApi;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Proyecto = await _proyectoApi.GetByIdAsync(id);

            if (Proyecto == null)
            {
                TempData["ErrorMessage"] = "Proyecto no encontrado.";
                return RedirectToPage("./Index");
            }

            await CargarUsuarios(id);
            return Page();
        }

        public async Task<IActionResult> OnPostAsignarAsync(int idProyecto, int idUsuario)
        {
            var dto = new ProyectoUsuarioDTO
            {
                IdProyecto = idProyecto,
                IdUsuario = idUsuario
            };

            var ok = await _proyectoApi.AsignarUsuarioAsync(dto);

            TempData[ok ? "SuccessMessage" : "ErrorMessage"] =
                ok ? "Usuario asignado correctamente."
                   : "Error al asignar usuario.";

            return RedirectToPage(new { id = idProyecto });
        }

        public async Task<IActionResult> OnPostDesasignarAsync(int idProyecto, int idUsuario)
        {
            var dto = new ProyectoUsuarioDTO
            {
                IdProyecto = idProyecto,
                IdUsuario = idUsuario
            };

            var ok = await _proyectoApi.DesasignarUsuarioAsync(dto);

            TempData[ok ? "SuccessMessage" : "ErrorMessage"] =
                ok ? "Usuario desasignado correctamente."
                   : "Error al desasignar usuario.";

            return RedirectToPage(new { id = idProyecto });
        }

        private async Task CargarUsuarios(int idProyecto)
        {
            UsuariosAsignados = await _proyectoApi.GetUsuariosAsignadosAsync(idProyecto);

            var todos = await _usuarioApi.GetAllAsync();

            UsuariosDisponibles = todos
                .Where(u => !UsuariosAsignados.Any(a => a.Id == u.Id) && u.Rol != "SuperAdmin")
                .OrderBy(u => u.PrimerApellido)
                .ToList();
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Usuarios
{
    [Authorize(Policy = "SoloAdmin")]
    public class IndexModel : PageModel
    {
        private readonly UsuarioApiClient _api;

        public IndexModel(UsuarioApiClient api)
        {
            _api = api;
        }

        public List<UsuarioDTO> Usuarios { get; set; } = new();

        [TempData] public string? MensajeExito { get; set; }
        [TempData] public string? MensajeError { get; set; }

        [BindProperty]
        public int DeleteId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var lista = await _api.GetAllAsync();

            if (lista == null)
            {
                MensajeError = "Error al obtener usuarios desde el servicio.";
                return Page();
            }
            Usuarios = lista;
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            bool ok = await _api.DeleteAsync(DeleteId);

            if (!ok)
                MensajeError = "Error al eliminar el usuario.";
            else
                MensajeExito = "Usuario eliminado correctamente.";

            return RedirectToPage("Index");
        }
    }
}

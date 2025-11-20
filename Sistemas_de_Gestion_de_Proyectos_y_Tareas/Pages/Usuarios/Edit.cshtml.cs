using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Usuarios
{
    [Authorize(Policy = "SoloAdmin")]
    public class EditModel : PageModel
    {
        private readonly UsuarioApiClient _api;

        public EditModel(UsuarioApiClient api)
        {
            _api = api;
        }

        [BindProperty]
        public UsuarioActualizarDTO UsuarioActualizar { get; set; } = new();

        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;

        [TempData] public string? SuccessMessage { get; set; }
        [TempData] public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var usuario = await _api.GetByIdAsync(id);

            if (usuario == null)
            {
                ErrorMessage = "Usuario no encontrado";
                return RedirectToPage("./Index");
            }

            UsuarioId = usuario.Id;
            NombreUsuario = usuario.NombreUsuario;
            UsuarioActualizar = new UsuarioActualizarDTO
            {
                Nombres = usuario.Nombres,
                PrimerApellido = usuario.PrimerApellido,
                SegundoApellido = usuario.SegundoApellido,
                Email = usuario.Email,
                Rol = usuario.Rol
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            UsuarioId = id;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var (success, errorMessage) = await _api.UpdateAsync(id, UsuarioActualizar);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Error al actualizar usuario.");
                return Page();
            }

            SuccessMessage = "Usuario actualizado correctamente.";
            return RedirectToPage("./Index");
        }
    }
}

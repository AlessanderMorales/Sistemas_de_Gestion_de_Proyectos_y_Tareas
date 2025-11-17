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
        public UsuarioDTO Usuario { get; set; } = new();

        [TempData] public string? SuccessMessage { get; set; }
        [TempData] public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var usuario = await _api.GetById(id);

            if (usuario == null)
            {
                ErrorMessage = "Usuario no encontrado";
                return RedirectToPage("./Index");
            }

            Usuario = usuario;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine("=== POST EDITAR ===");
            Console.WriteLine("Id recibido: " + Usuario.Id);
            Console.WriteLine("Nombres: " + Usuario.Nombres);
            Console.WriteLine("Email: " + Usuario.Email);
            Console.WriteLine("====================");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("MODEL STATE INVALIDO");
                foreach (var err in ModelState)
                {
                    foreach (var e in err.Value.Errors)
                        Console.WriteLine($"ERROR {err.Key}: {e.ErrorMessage}");
                }
                return Page();
            }

            bool ok = await _api.Update(Usuario);

            if (!ok)
            {
                ErrorMessage = "Error al actualizar usuario.";
                return RedirectToPage("./Index");
            }

            SuccessMessage = "Usuario actualizado correctamente.";
            return RedirectToPage("./Index");
        }
    }
}

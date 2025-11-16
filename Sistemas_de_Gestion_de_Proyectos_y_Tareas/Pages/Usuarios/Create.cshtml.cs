using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Usuarios
{
    [Authorize(Policy = "SoloAdmin")]
    public class CreateModel : PageModel
    {
        private readonly UsuarioApiClient _api;

        public CreateModel(UsuarioApiClient api)
        {
            _api = api;
        }

        [BindProperty]
        public UsuarioDTO Usuario { get; set; } = new UsuarioDTO { Rol = "Empleado" };

        [TempData] public string? MensajeExito { get; set; }
        [TempData] public string? MensajeError { get; set; }

        public void OnGet()
        {
            Usuario = new UsuarioDTO { Rol = "Empleado" };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            bool creado = await _api.CrearUsuario(Usuario);

            if (!creado)
            {
                MensajeError = "No se pudo crear el usuario. Revisa el microservicio.";
                return RedirectToPage("./Index");
            }

            MensajeExito = "Usuario creado exitosamente.";
            return RedirectToPage("./Index");
        }
    }
}

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
        public UsuarioCrearDTO Usuario { get; set; } = new() { Rol = "Empleado" };

        [TempData] public string? MensajeExito { get; set; }
        [TempData] public string? MensajeError { get; set; }

        public void OnGet()
        {
            Usuario = new UsuarioCrearDTO { Rol = "Empleado" };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // 🔥 Esto evita que Razor valide esos dos campos (porque los llenas tú mismo)
            ModelState.Remove("Usuario.Contraseña");
            ModelState.Remove("Usuario.NombreUsuario");

            if (!ModelState.IsValid)
                return Page();

            // Generación automática
            Usuario.NombreUsuario = Usuario.Email.Split('@')[0];
            Usuario.Contraseña = Guid.NewGuid().ToString().Substring(0, 8);

            bool creado = await _api.CrearUsuarioAsync(Usuario);

            if (!creado)
            {
                MensajeError = "Error al crear el usuario.";
                return RedirectToPage("./Index");
            }

            MensajeExito = "Usuario creado exitosamente.";
            return RedirectToPage("./Index");

        }

    }
}

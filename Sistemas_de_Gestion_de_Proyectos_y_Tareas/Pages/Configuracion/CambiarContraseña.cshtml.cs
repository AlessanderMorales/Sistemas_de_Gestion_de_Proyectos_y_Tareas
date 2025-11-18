using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Configuracion
{
    [Authorize]
    public class CambiarContraseñaModel : PageModel
    {
        private readonly UsuarioApiClient _usuarioApi;

        public CambiarContraseñaModel(UsuarioApiClient usuarioApi)
        {
            _usuarioApi = usuarioApi;
        }

        [BindProperty]
        public CambiarContraseñaInput Input { get; set; } = new();

        [TempData] public string? MensajeExito { get; set; }
        [TempData] public string? MensajeError { get; set; }

        public class CambiarContraseñaInput
        {
            [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
            [DataType(DataType.Password)]
            public string ContraseñaActual { get; set; } = "";

            [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
            [DataType(DataType.Password)]
            [StringLength(15, MinimumLength = 8,
                ErrorMessage = "Debe tener entre 8 y 15 caracteres.")]
            public string NuevaContraseña { get; set; } = "";

            [Required(ErrorMessage = "Debe confirmar la contraseña.")]
            [DataType(DataType.Password)]
            [Compare("NuevaContraseña", ErrorMessage = "No coinciden.")]
            public string ConfirmarContraseña { get; set; } = "";
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            int usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (Input.ContraseñaActual == Input.NuevaContraseña)
            {
                ModelState.AddModelError("Input.NuevaContraseña",
                    "La nueva contraseña no puede ser igual a la actual.");
                return Page();
            }

            if (!ValidarContraseña(Input.NuevaContraseña))
            {
                ModelState.AddModelError("Input.NuevaContraseña",
                    "Debe contener mayúscula, minúscula, número y símbolo.");
                return Page();
            }

            bool ok = await _usuarioApi.CambiarContraseñaAsync(
                usuarioId,
                Input.ContraseñaActual,
                Input.NuevaContraseña
            );

            if (!ok)
            {
                ModelState.AddModelError("Input.ContraseñaActual",
                    "La contraseña actual no es correcta.");
                return Page();
            }

            MensajeExito = "Contraseña cambiada exitosamente.";
            return RedirectToPage("/Configuracion/CambiarContraseña");
        }

        private bool ValidarContraseña(string c)
        {
            if (string.IsNullOrWhiteSpace(c)) return false;
            if (c.Length < 8 || c.Length > 15) return false;
            if (!System.Text.RegularExpressions.Regex.IsMatch(c, @"[A-Z]")) return false;
            if (!System.Text.RegularExpressions.Regex.IsMatch(c, @"[a-z]")) return false;
            if (!System.Text.RegularExpressions.Regex.IsMatch(c, @"\d")) return false;
            if (!System.Text.RegularExpressions.Regex.IsMatch(c, @"[\W_]")) return false;
            return true;
        }
    }
}

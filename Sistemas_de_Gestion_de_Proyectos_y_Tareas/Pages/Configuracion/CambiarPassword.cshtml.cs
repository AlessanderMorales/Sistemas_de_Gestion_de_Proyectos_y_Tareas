using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Configuracion
{
    [Authorize]
    public class CambiarPasswordModel : PageModel
    {
        private readonly UsuarioApiClient _usuarioApi;
        private readonly ILogger<CambiarPasswordModel> _logger;

        public CambiarPasswordModel(UsuarioApiClient usuarioApi, ILogger<CambiarPasswordModel> logger)
        {
            _usuarioApi = usuarioApi;
            _logger = logger;
        }

        [BindProperty]
        public CambiarContraseñaInput Input { get; set; } = new();

        public class CambiarContraseñaInput
        {
            [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña Actual")]
            public string ContraseñaActual { get; set; } = "";

            [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
            [DataType(DataType.Password)]
            [StringLength(15, MinimumLength = 8, ErrorMessage = "Debe tener entre 8 y 15 caracteres.")]
            [Display(Name = "Nueva Contraseña")]
            public string NuevaContraseña { get; set; } = "";

            [Required(ErrorMessage = "Debe confirmar la contraseña.")]
            [DataType(DataType.Password)]
            [Compare("NuevaContraseña", ErrorMessage = "Las contraseñas no coinciden.")]
            [Display(Name = "Confirmar Contraseña")]
            public string ConfirmarContraseña { get; set; } = "";
        }

        public void OnGet()
        {
            _logger.LogInformation($"Usuario {User.Identity?.Name} accedió a cambiar contraseña");

            var requiereCambio = User.FindFirst("RequiereCambioContraseña")?.Value == "True";
            if (requiereCambio)
            {
                _logger.LogInformation("Usuario requiere cambio de contraseña obligatorio");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger.LogInformation($"Iniciando cambio de contraseña para usuario {User.Identity?.Name}");

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState inválido en cambio de contraseña");
                    return Page();
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int usuarioId))
                {
                    TempData["MensajeError"] = "Error al identificar el usuario. Por favor, inicie sesión nuevamente.";
                    return Page();
                }

                if (Input.ContraseñaActual == Input.NuevaContraseña)
                {
                    TempData["MensajeError"] = "La nueva contraseña no puede ser igual a la actual.";
                    return Page();
                }

                if (!ValidarContraseña(Input.NuevaContraseña))
                {
                    TempData["MensajeError"] = "La contraseña debe contener: mayúscula, minúscula, número y símbolo especial.";
                    return Page();
                }

                _logger.LogInformation($"Llamando a la API para cambiar contraseña del usuario {usuarioId}");

                bool ok = await _usuarioApi.CambiarContraseñaAsync(
                    usuarioId,
                    Input.ContraseñaActual,
                    Input.NuevaContraseña
                );

                if (!ok)
                {
                    _logger.LogWarning($"Fallo al cambiar contraseña para usuario {usuarioId}");
                    TempData["MensajeError"] = "La contraseña actual es incorrecta.";
                    return Page();
                }

                _logger.LogInformation($"Contraseña cambiada exitosamente para usuario {usuarioId}");

                var identity = (ClaimsIdentity)User.Identity;
                var requiereCambioClaim = identity.FindFirst("RequiereCambioContraseña");
                if (requiereCambioClaim != null)
                {
                    identity.RemoveClaim(requiereCambioClaim);
                }
                identity.AddClaim(new Claim("RequiereCambioContraseña", "False"));

                await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(identity));

                TempData["SuccessMessage"] = "Contraseña cambiada exitosamente";

                if (User.IsInRole("SuperAdmin"))
                {
                    return RedirectToPage("/Usuarios/Index");
                }
                else if (User.IsInRole("JefeDeProyecto") || User.IsInRole("Empleado"))
                {
                    return RedirectToPage("/Index");
                }

                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña");
                TempData["MensajeError"] = "Ocurrió un error al cambiar la contraseña. Por favor, intente nuevamente.";
                return Page();
            }
        }

        private bool ValidarContraseña(string c)
        {
            if (string.IsNullOrWhiteSpace(c)) return false;
            if (c.Length < 8 || c.Length > 15) return false;
            if (!Regex.IsMatch(c, @"[A-Z]")) return false; 
            if (!Regex.IsMatch(c, @"[a-z]")) return false;
            if (!Regex.IsMatch(c, @"\d")) return false;
            if (!Regex.IsMatch(c, @"[\W_]")) return false;
            return true;
        }
    }
}

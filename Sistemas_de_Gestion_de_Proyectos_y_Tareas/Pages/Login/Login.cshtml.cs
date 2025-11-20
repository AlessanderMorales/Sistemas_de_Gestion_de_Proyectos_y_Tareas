using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.Security;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages
{
    public class LoginModel : PageModel
    {
        private readonly UsuarioApiClient _usuarioApi;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(UsuarioApiClient usuarioApi, ILogger<LoginModel> logger)
        {
            _usuarioApi = usuarioApi;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El email o nombre de usuario es obligatorio.")]
            public string EmailOrUsername { get; set; }

            [Required(ErrorMessage = "La contraseña es obligatoria.")]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            _logger.LogInformation("Página de login cargada");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
                return Page();

            try
            {
                UsuarioLoginDTO loginDto = new UsuarioLoginDTO
                {
                    EmailOrUser = Input.EmailOrUsername,
                    Password = Input.Password
                };

                _logger.LogInformation($"Intentando login para: {Input.EmailOrUsername}");

                LoginResponseDTO? loginResponse = await _usuarioApi.LoginAsync(loginDto);

                if (loginResponse == null || loginResponse.Error)
                {
                    _logger.LogWarning($"Login fallido para: {Input.EmailOrUsername}");
                    TempData["ErrorMessage"] = "Email/usuario o contraseña incorrectos.";
                    return Page();
                }

                _logger.LogInformation($"Login exitoso para: {loginResponse.NombreUsuario}");

                HttpContext.Session.SetString("JwtToken", loginResponse.Token);
                _logger.LogInformation($"Token JWT almacenado en sesión para: {loginResponse.NombreUsuario}");

                string normalizedRole = loginResponse.Rol switch
                {
                    var r when r.Contains("super", StringComparison.OrdinalIgnoreCase) => Roles.SuperAdmin,
                    var r when r.Contains("jefe", StringComparison.OrdinalIgnoreCase) => Roles.JefeDeProyecto,
                    _ => Roles.Empleado
                };

                string fullName = $"{loginResponse.Nombres} {loginResponse.PrimerApellido} {loginResponse.SegundoApellido}".Trim();
                string requiereCambioString = loginResponse.RequiereCambioContraseña ? "True" : "False";

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, loginResponse.Id_Usuario.ToString()),
                    new Claim(ClaimTypes.Name, loginResponse.Email ?? loginResponse.NombreUsuario),
                    new Claim("Username", loginResponse.NombreUsuario ?? loginResponse.Email),
                    new Claim("FullName", fullName),
                    new Claim(ClaimTypes.Role, normalizedRole),
                    new Claim("RequiereCambioContraseña", requiereCambioString)
                };

                var identity = new ClaimsIdentity(claims, "MyCookieAuth");
                await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(identity));

                if (loginResponse.RequiereCambioContraseña)
                {
                    return RedirectToPage("/Configuracion/CambiarPassword");
                }

                return normalizedRole switch
                {
                    Roles.SuperAdmin => RedirectToPage("/Usuarios/Index"),
                    _ => LocalRedirect(returnUrl)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en proceso de login");
                TempData["ErrorMessage"] = "Ocurrió un error al iniciar sesión. Por favor, intente nuevamente.";
                return Page();
            }
        }
    }
}

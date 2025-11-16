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

        public LoginModel(UsuarioApiClient usuarioApi)
        {
            _usuarioApi = usuarioApi;
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
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
                return Page();

            // 🔥 1. Obtener usuario por login desde API
            UsuarioLoginDTO loginDto = new UsuarioLoginDTO
            {
                EmailOrUser = Input.EmailOrUsername,
                Password = Input.Password
            };

            UsuarioDTO usuario = await _usuarioApi.LoginAsync(loginDto);

            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "Email/usuario o contraseña incorrectos.");
                return Page();
            }

            // 🔥 2. Normalizar rol
            string normalizedRole = usuario.Rol switch
            {
                var r when r.Contains("super", StringComparison.OrdinalIgnoreCase) => Roles.SuperAdmin,
                var r when r.Contains("jefe", StringComparison.OrdinalIgnoreCase) => Roles.JefeDeProyecto,
                _ => Roles.Empleado
            };

            string fullName = $"{usuario.Nombres} {usuario.PrimerApellido} {usuario.SegundoApellido}".Trim();

            // 🔥 3. Crear claims de autenticación
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Email ?? usuario.NombreUsuario),
                new Claim("Username", usuario.NombreUsuario ?? usuario.Email),
                new Claim("FullName", fullName),
                new Claim(ClaimTypes.Role, normalizedRole),
                new Claim("RequiereCambioContraseña", usuario.RequiereCambioContraseña.ToString())
            };

            var identity = new ClaimsIdentity(claims, "MyCookieAuth");
            await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(identity));

            // 🔥 4. Si requiere cambiar contraseña → redirigir
            if (usuario.RequiereCambioContraseña)
            {
                TempData["RequiereCambioContraseña"] = "Por seguridad, debes cambiar tu contraseña temporal.";
                return RedirectToPage("/Configuracion/CambiarContraseña");
            }

            // 🔥 5. Redirigir según rol
            return normalizedRole switch
            {
                Roles.SuperAdmin => RedirectToPage("/Usuarios/Index"),
                _ => LocalRedirect(returnUrl)
            };
        }
    }
}

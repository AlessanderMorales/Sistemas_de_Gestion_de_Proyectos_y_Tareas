using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.Service;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Usuarios
{
    [Authorize(Policy = "SoloAdmin")]
    public class CreateModel : PageModel
    {
        private readonly UsuarioApiClient _api;
        private readonly EmailService _emailService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(
            UsuarioApiClient api,
            EmailService emailService,
            ILogger<CreateModel> logger)
        {
            _api = api;
            _emailService = emailService;
            _logger = logger;
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
            ModelState.Remove("Usuario.Contraseña");
            ModelState.Remove("Usuario.NombreUsuario");

            if (!ModelState.IsValid)
                return Page();

            Usuario.NombreUsuario = Usuario.Email.Split('@')[0];
            Usuario.Contraseña = GenerarContraseñaSegura();

            bool creado = await _api.CrearUsuarioAsync(Usuario);

            if (!creado)
            {
                MensajeError = "Error al crear el usuario.";
                return RedirectToPage("./Index");
            }

            try
            {
                var nombreCompleto = $"{Usuario.Nombres} {Usuario.PrimerApellido}";

                bool emailEnviado = await _emailService.EnviarEmailContraseña(
                    Usuario.Email,
                    nombreCompleto,
                    Usuario.NombreUsuario,
                    Usuario.Contraseña
                );

                if (emailEnviado)
                {
                    _logger.LogInformation($"Email enviado exitosamente a {Usuario.Email}");
                    MensajeExito = $"Usuario creado exitosamente. Se ha enviado un email con las credenciales a {Usuario.Email}";
                }
                else
                {
                    _logger.LogWarning($"No se pudo enviar el email a {Usuario.Email}");
                    MensajeExito = $"Usuario creado. Sin embargo, no se pudo enviar el email. Credenciales: Usuario={Usuario.NombreUsuario}, Contraseña={Usuario.Contraseña}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar email a {Usuario.Email}");
                MensajeExito = $"Usuario creado. Error al enviar email. Credenciales: Usuario={Usuario.NombreUsuario}, Contraseña={Usuario.Contraseña}";
            }

            return RedirectToPage("./Index");
        }

        private string GenerarContraseñaSegura()
        {
            const string mayusculas = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string minusculas = "abcdefghijklmnopqrstuvwxyz";
            const string numeros = "0123456789";
            const string simbolos = "@#$%&*";

            var random = new Random();
            var password = new char[10];

            password[0] = mayusculas[random.Next(mayusculas.Length)];
            password[1] = minusculas[random.Next(minusculas.Length)];
            password[2] = numeros[random.Next(numeros.Length)];
            password[3] = simbolos[random.Next(simbolos.Length)];

            string todosLosCaracteres = mayusculas + minusculas + numeros + simbolos;
            for (int i = 4; i < 10; i++)
            {
                password[i] = todosLosCaracteres[random.Next(todosLosCaracteres.Length)];
            }

            return new string(password.OrderBy(x => random.Next()).ToArray());
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.Service;
using System.Text;
using System.Text.RegularExpressions;

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

            // Generar nombre de usuario basado en nombre y apellido
            Usuario.NombreUsuario = await GenerarNombreUsuarioUnicoAsync(Usuario.Nombres, Usuario.PrimerApellido);
            Usuario.Contraseña = GenerarContraseñaSegura();

            // Llamar a la API con el nuevo método que retorna errores específicos
            var (success, errorMessage) = await _api.CrearUsuarioAsync(Usuario);

            if (!success)
            {
                // Mostrar el error específico en la página
                ModelState.AddModelError(string.Empty, errorMessage ?? "Error al crear el usuario.");
                return Page(); // NO redirige, se queda en la página para mostrar el error
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

        /// <summary>
        /// Genera un nombre de usuario único en formato NombreApellido
        /// Si ya existe, agrega un número incremental (NombreApellido2, NombreApellido3, etc.)
        /// </summary>
        private async Task<string> GenerarNombreUsuarioUnicoAsync(string nombres, string primerApellido)
        {
            // Tomar el primer nombre (si hay varios nombres separados por espacio)
            var primerNombre = nombres.Split(' ')[0].Trim();

            // Remover acentos y caracteres especiales
            var nombreLimpio = RemoverAcentos(primerNombre);
            var apellidoLimpio = RemoverAcentos(primerApellido);

            // Generar nombre de usuario base: NombreApellido
            var nombreUsuarioBase = $"{nombreLimpio}{apellidoLimpio}";

            // Obtener todos los usuarios existentes
            var usuariosExistentes = await _api.GetAllAsync();
            var nombresUsuarioExistentes = usuariosExistentes
                .Select(u => u.NombreUsuario.ToLower())
                .ToHashSet();

            // Si no existe, retornar el nombre base
            var nombreUsuarioFinal = nombreUsuarioBase;
            if (!nombresUsuarioExistentes.Contains(nombreUsuarioFinal.ToLower()))
            {
                return nombreUsuarioFinal;
            }

            // Si existe, agregar números incrementales hasta encontrar uno disponible
            int contador = 2;
            while (nombresUsuarioExistentes.Contains($"{nombreUsuarioBase}{contador}".ToLower()))
            {
                contador++;
            }

            nombreUsuarioFinal = $"{nombreUsuarioBase}{contador}";

            _logger.LogInformation($"Nombre de usuario generado: {nombreUsuarioFinal} (había {contador - 1} duplicados)");

            return nombreUsuarioFinal;
        }

        /// <summary>
        /// Remueve acentos y caracteres especiales de un texto
        /// </summary>
        private string RemoverAcentos(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            // Normalizar y remover acentos
            var textoNormalizado = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in textoNormalizado)
            {
                // Solo mantener letras y números
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
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

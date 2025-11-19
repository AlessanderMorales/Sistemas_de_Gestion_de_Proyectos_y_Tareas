using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Comentarios;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios;
using System.Security.Claims;
using System.Collections.Generic; // Asegúrate de que este using esté presente
using System.Linq; // Asegúrate de que este using esté presente

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Comentarios
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ComentarioApiClient _comentarioApi;
        private readonly UsuarioApiClient _usuarioApi;
        private readonly TareaApiClient _tareaApi;

        // 🔹 Datos para la vista
        public List<TareaDTO> Tareas { get; set; } = new();
        public Dictionary<int, List<UsuarioDTO>> TareaUsuariosMap { get; set; } = new();

        [BindProperty]
        public ComentarioDTO Comentario { get; set; } = new ComentarioDTO();

        [BindProperty]
        public int DirigidoAUsuarioId { get; set; }

        public int UsuarioActualId { get; set; }

        public CreateModel(
            ComentarioApiClient comentarioApi,
            UsuarioApiClient usuarioApi,
            TareaApiClient tareaApi)
        {
            _comentarioApi = comentarioApi;
            _usuarioApi = usuarioApi;
            _tareaApi = tareaApi;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            int idTemp;
            if (!int.TryParse(idClaim, out idTemp))
                return Unauthorized();

            UsuarioActualId = idTemp;

            if (User.IsInRole("Empleado"))
            {
                Tareas = (await _tareaApi.GetByUsuarioAsync(UsuarioActualId)).ToList();
            }
            else
            {
                Tareas = (await _tareaApi.GetAllAsync()).ToList();
            }

            TareaUsuariosMap = new Dictionary<int, List<UsuarioDTO>>();

            foreach (var t in Tareas)
            {
                // Obtener IDs de usuarios asignados desde el microservicio tareas
                // t.Id es int (no nullable) por la definición de TareaDTO, así que esto es seguro
                var usuariosIds = await _tareaApi.GetUsuariosAsignadosAsync(t.Id);

                var usuarios = new List<UsuarioDTO>();

                foreach (var uid in usuariosIds)
                {
                    var u = await _usuarioApi.GetAsync(uid);
                    if (u != null && u.Id != UsuarioActualId && u.Rol != "SuperAdmin")
                        usuarios.Add(u);
                }

                TareaUsuariosMap[t.Id] = usuarios;
            }

            if (Tareas.Any())
            {
                var firstTarea = Tareas.First();

                if (TareaUsuariosMap.ContainsKey(firstTarea.Id) &&
                    TareaUsuariosMap[firstTarea.Id].Any())
                {
                    DirigidoAUsuarioId = TareaUsuariosMap[firstTarea.Id].First().Id;
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            int idTemp;
            if (!int.TryParse(idClaim, out idTemp))
                return Unauthorized();

            UsuarioActualId = idTemp;

            // ================================
            // 📌 Validación básica
            // ================================
            // Primero, validamos IdTarea antes de intentar usarlo
            // <-- ¡CORRECCIÓN CLAVE AQUÍ! -->
            if (!Comentario.IdTarea.HasValue || Comentario.IdTarea.Value <= 0) // Verifica si tiene valor y es válido
            {
                TempData["ErrorMessage"] = "Debes seleccionar una tarea válida.";
                // Recargar datos para la vista si hay un error de validación
                await OnGetAsync();
                return Page(); // Devuelve la página con el error y los datos precargados
            }


            if (DirigidoAUsuarioId <= 0)
            {
                TempData["ErrorMessage"] = "Debes seleccionar un destinatario.";
                await OnGetAsync(); 
                return Page();
            }

            if (DirigidoAUsuarioId == UsuarioActualId)
            {
                TempData["ErrorMessage"] = "No puedes comentarte a ti mismo.";
                await OnGetAsync();
                return Page();
            }

            // ================================
            // 📌 Validar tarea (ahora es seguro usar .Value)
            // ================================
            // <-- ¡CORRECCIÓN CLAVE AQUÍ! -->
            var tarea = await _tareaApi.GetAsync(Comentario.IdTarea.Value); // Usamos .Value porque ya verificamos HasValue

            if (tarea == null)
            {
                TempData["ErrorMessage"] = "La tarea seleccionada no existe.";
                await OnGetAsync(); // Recargar datos para la vista
                return Page();
            }

            // ================================
            // 📌 Validar destinatario
            // ================================
            var destinatario = await _usuarioApi.GetAsync(DirigidoAUsuarioId);

            if (destinatario == null)
            {
                TempData["ErrorMessage"] = "El destinatario no existe.";
                await OnGetAsync(); // Recargar datos para la vista
                return Page();
            }

            if (destinatario.Rol == "SuperAdmin")
            {
                TempData["ErrorMessage"] = "No puedes enviar comentarios al administrador.";
                await OnGetAsync(); // Recargar datos para la vista
                return Page();
            }

            // ================================
            // 📌 Construir comentario
            // ================================
            Comentario.IdUsuario = UsuarioActualId;           // autor
            Comentario.IdDestinatario = DirigidoAUsuarioId;  // destinatario
            Comentario.Estado = 1;
            Comentario.Fecha = DateTime.Now;

            // ================================
            // 📌 Guardar usando el API Client
            // ================================
            var ok = await _comentarioApi.CreateAsync(Comentario);

            if (!ok)
            {
                TempData["ErrorMessage"] = "No se pudo guardar el comentario.";
                await OnGetAsync(); // Recargar datos para la vista
                return Page();
            }

            TempData["MensajeExito"] =
                $"Comentario enviado a {destinatario.Nombres} {destinatario.PrimerApellido}.";

            return RedirectToPage("Index");
        }
    }
}
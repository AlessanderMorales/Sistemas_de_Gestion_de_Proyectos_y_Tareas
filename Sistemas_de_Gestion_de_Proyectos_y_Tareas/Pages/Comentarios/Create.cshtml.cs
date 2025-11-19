using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Comentarios;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System; // Asegúrate de que este using esté presente

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

            // ================================
            // 📌 Obtener tareas según rol
            // ================================
            if (User.IsInRole("Empleado"))
            {
                Tareas = (await _tareaApi.GetByUsuarioAsync(UsuarioActualId))?.ToList() ?? new List<TareaDTO>();
            }
            else
            {
                Tareas = (await _tareaApi.GetAllAsync())?.ToList() ?? new List<TareaDTO>();
            }

            // ================================
            // 📌 Construir mapa tarea → usuarios asignados
            // ================================
            TareaUsuariosMap = new Dictionary<int, List<UsuarioDTO>>();
            var allUsuarios = (await _usuarioApi.GetAllAsync())?.ToList() ?? new List<UsuarioDTO>(); // Obtener todos los usuarios de una vez

            foreach (var t in Tareas)
            {
                // Obtener IDs de usuarios asignados desde el microservicio tareas
                // t.Id es int (no nullable) por la definición de TareaDTO, así que esto es seguro
                var usuariosIds = await _tareaApi.GetUsuariosAsignadosAsync(t.Id); // Esto devuelve List<int>

                var usuariosElegibles = new List<UsuarioDTO>();

                foreach (var uid in usuariosIds)
                {
                    var u = allUsuarios.FirstOrDefault(usuario => usuario.Id == uid); // Buscar en la lista completa
                    // Filtrar usuarios: no el actual, no SuperAdmin, y el usuario debe existir
                    if (u != null && u.Id != UsuarioActualId && u.Rol != "SuperAdmin")
                        usuariosElegibles.Add(u);
                }

                TareaUsuariosMap[t.Id] = usuariosElegibles;
            }

            // ================================
            // 📌 Preseleccionar primer destinatario si existe (para el caso de un Post fallido)
            // Esto es solo para inicializar, la lógica JS gestiona la preselección visible.
            // ================================
            if (DirigidoAUsuarioId == 0 && Tareas.Any())
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
            // 📌 Validación de ID de Tarea
            // ================================
            // Comentario.IdTarea es int? en ComentarioDTO, así que usamos .HasValue y .Value
            if (!Comentario.IdTarea.HasValue || Comentario.IdTarea.Value <= 0)
            {
                TempData["ErrorMessage"] = "Debes seleccionar una tarea válida.";
                await OnGetAsync(); // Recargar datos para la vista
                return Page();
            }

            // ================================
            // 📌 Validación de Destinatario
            // ================================
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
            // 📌 Validar existencia de la tarea seleccionada
            // ================================
            var tarea = await _tareaApi.GetAsync(Comentario.IdTarea.Value); // Usamos .Value
            if (tarea == null)
            {
                TempData["ErrorMessage"] = "La tarea seleccionada no existe.";
                await OnGetAsync();
                return Page();
            }

            // ================================
            // 📌 Validar existencia y rol del destinatario
            // ================================
            var destinatario = await _usuarioApi.GetAsync(DirigidoAUsuarioId);

            if (destinatario == null)
            {
                TempData["ErrorMessage"] = "El destinatario no existe.";
                await OnGetAsync();
                return Page();
            }

            if (destinatario.Rol == "SuperAdmin")
            {
                TempData["ErrorMessage"] = "No puedes enviar comentarios al administrador.";
                await OnGetAsync();
                return Page();
            }

            // ================================
            // 📌 Construir comentario
            // ================================
            Comentario.IdUsuario = UsuarioActualId;
            Comentario.IdDestinatario = DirigidoAUsuarioId;
            Comentario.Estado = 1;
            Comentario.Fecha = DateTime.Now;

            // ================================
            // 📌 Guardar usando el API Client
            // ================================
            var ok = await _comentarioApi.CreateAsync(Comentario);

            if (!ok)
            {
                TempData["ErrorMessage"] = "No se pudo guardar el comentario.";
                await OnGetAsync();
                return Page();
            }

            TempData["MensajeExito"] =
                $"Comentario enviado a {destinatario.Nombres} {destinatario.PrimerApellido}.";

            return RedirectToPage("Index");
        }
    }
}
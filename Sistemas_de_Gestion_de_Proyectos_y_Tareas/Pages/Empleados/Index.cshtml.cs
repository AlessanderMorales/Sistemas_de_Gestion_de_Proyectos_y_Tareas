using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Proyectos;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Empleados
{
    [Authorize(Policy = "OnlyJefe")]
    public class IndexModel : PageModel
    {
        private readonly UsuarioApiClient _usuarioApi;
        private readonly TareaApiClient _tareaApi;
        private readonly ProyectoApiClient _proyectoApi;

        public IndexModel(
            UsuarioApiClient usuarioApi,
            TareaApiClient tareaApi,
            ProyectoApiClient proyectoApi)
        {
            _usuarioApi = usuarioApi;
            _tareaApi = tareaApi;
            _proyectoApi = proyectoApi;
        }

        public class EmpleadoInfo
        {
            public UsuarioDTO Usuario { get; set; }
            public List<string> ProyectosAsignados { get; set; } = new();
            public List<string> TareasAsignadas { get; set; } = new();
            public int TotalProyectos { get; set; }
            public int TotalTareas { get; set; }
        }

        public List<EmpleadoInfo> Empleados { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Obtener todos los usuarios excepto SuperAdmin
            var usuarios = (await _usuarioApi.GetAllAsync())
                .Where(u => u.Rol != "SuperAdmin")
                .OrderBy(u => u.Rol)
                .ThenBy(u => u.PrimerApellido)
                .ToList();

            foreach (var u in usuarios)
            {
                var info = new EmpleadoInfo { Usuario = u };

                // ---------------------------
                // PROYECTOS ASIGNADOS
                // ---------------------------
                var proyectos = await _proyectoApi.GetByUsuarioAsync(u.Id);
                info.ProyectosAsignados = proyectos.Select(p => p.Nombre).ToList();
                info.TotalProyectos = proyectos.Count;

                // ---------------------------
                // TAREAS ASIGNADAS
                // ---------------------------
                var tareas = await _tareaApi.GetByUsuarioAsync(u.Id);
                info.TareasAsignadas = tareas.Select(t => t.Titulo).ToList();
                info.TotalTareas = tareas.Count;

                Empleados.Add(info);
            }
        }
    }
}

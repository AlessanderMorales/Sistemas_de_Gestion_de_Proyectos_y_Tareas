using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Proyectos;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Application.Facades
{
    public class GestionProyectosFacade
    {
        private readonly ProyectoApiClient _proyectoApi;
        private readonly TareaApiClient _tareaApi;
        private readonly UsuarioApiClient _usuarioApi;

        public GestionProyectosFacade(
            ProyectoApiClient proyectoApi,
            TareaApiClient tareaApi,
            UsuarioApiClient usuarioApi)
        {
            _proyectoApi = proyectoApi;
            _tareaApi = tareaApi;
            _usuarioApi = usuarioApi;
        }

        // ---------------------------------------------------------
        // 📌 ESTADÍSTICAS GENERALES
        // ---------------------------------------------------------
        public async Task<EstadisticasGeneralesViewModel> ObtenerEstadisticasGeneralesAsync()
        {
            var proyectos = await _proyectoApi.GetAllAsync();
            var tareas = await _tareaApi.GetAllAsync();
            var usuarios = await _usuarioApi.GetAllAsync();

            usuarios = usuarios.Where(u => u.Rol != "SuperAdmin").ToList();

            return new EstadisticasGeneralesViewModel
            {
                TotalProyectos = proyectos.Count,
                ProyectosActivos = proyectos.Count(p => p.Estado == 1),
                TotalTareas = tareas.Count,
                TareasCompletadas = tareas.Count(t => t.Status == "Completada"),
                TareasEnProgreso = tareas.Count(t => t.Status == "EnProgreso"),
                TareasPendientes = tareas.Count(t => t.Status == "SinIniciar"),
                TotalUsuarios = usuarios.Count,
                Empleados = usuarios.Count(u => u.Rol == "Empleado"),
                JefesDeProyecto = usuarios.Count(u => u.Rol == "JefeDeProyecto")
            };
        }

        // ---------------------------------------------------------
        // 📌 DASHBOARD DE USUARIO
        // ---------------------------------------------------------
        public async Task<DashboardUsuarioViewModel?> ObtenerDashboardUsuarioAsync(int idUsuario)
        {
            var usuario = await _usuarioApi.GetByIdAsync(idUsuario);
            if (usuario == null) return null;

            var proyectos = await _proyectoApi.GetByUsuarioAsync(idUsuario);
            var tareas = await _tareaApi.GetByUsuarioAsync(idUsuario);

            return new DashboardUsuarioViewModel
            {
                Usuario = usuario,
                Proyectos = proyectos,
                TotalProyectos = proyectos.Count,
                Tareas = tareas,
                TotalTareas = tareas.Count,
                TareasCompletadas = tareas.Count(t => t.Status == "Completada"),
                TareasEnProgreso = tareas.Count(t => t.Status == "EnProgreso"),
                TareasPendientes = tareas.Count(t => t.Status == "SinIniciar"),
                ProyectosActivos = proyectos.Count(p => p.Estado == 1),
            };
        }
    }

    // ======================================================
    // VIEWMODELS LOS MISMOS QUE USABAS
    // ======================================================

    public class EstadisticasGeneralesViewModel
    {
        public int TotalProyectos { get; set; }
        public int ProyectosActivos { get; set; }
        public int TotalTareas { get; set; }
        public int TareasCompletadas { get; set; }
        public int TareasEnProgreso { get; set; }
        public int TareasPendientes { get; set; }
        public int TotalUsuarios { get; set; }
        public int Empleados { get; set; }
        public int JefesDeProyecto { get; set; }
    }

    public class DashboardUsuarioViewModel
    {
        public UsuarioDTO Usuario { get; set; }
        public List<ProyectoDTO> Proyectos { get; set; } = new();
        public int TotalProyectos { get; set; }
        public List<TareaDTO> Tareas { get; set; } = new();
        public int TotalTareas { get; set; }
        public int TareasCompletadas { get; set; }
        public int TareasEnProgreso { get; set; }
        public int TareasPendientes { get; set; }
        public int ProyectosActivos { get; set; }
    }
}

using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Proyectos;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios;
using System.Net.Http.Json;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients
{
    public class ProyectoApiClient
    {
        private readonly HttpClient _http;
        private const string BasePath = "api/Proyecto";

        public ProyectoApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<ProyectoDTO?> GetByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<ProyectoDTO>($"{BasePath}/{id}");
        }

        // ======================================================
        // ✔ CRUD PROYECTOS
        // ======================================================
        public async Task<List<ProyectoDTO>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<ProyectoDTO>>(BasePath)
                ?? new List<ProyectoDTO>();
        }

        public async Task<bool> CreateAsync(ProyectoDTO dto)
        {
            var response = await _http.PostAsJsonAsync(BasePath, dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(ProyectoDTO dto)
        {
            var response = await _http.PutAsJsonAsync($"{BasePath}/{dto.IdProyecto}", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(int id, ProyectoDTO dto)
        {
            var response = await _http.PutAsJsonAsync($"{BasePath}/{id}", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync($"{BasePath}/{id}");
            return response.IsSuccessStatusCode;
        }

        // ======================================================
        // ✔ OBTENER PROYECTOS POR USUARIO
        // ======================================================
        // Ruta real según tu Microservicio:
        // GET /api/proyecto/usuario/{idUsuario}
        public async Task<List<ProyectoDTO>> GetByUsuarioAsync(int usuarioId)
        {
            return await _http.GetFromJsonAsync<List<ProyectoDTO>>(
                $"{BasePath}/por-usuario/{usuarioId}")
                ?? new List<ProyectoDTO>();
        }


        // ======================================================
        // ✔ OBTENER USUARIOS ASIGNADOS A UN PROYECTO
        // ======================================================
        // GET /api/proyecto/{id}/usuarios
        public async Task<List<UsuarioDTO>> GetUsuariosAsignadosAsync(int idProyecto)
        {
            return await _http.GetFromJsonAsync<List<UsuarioDTO>>(
                $"{BasePath}/{idProyecto}/usuarios")
                ?? new List<UsuarioDTO>();
        }

        // ======================================================
        // ✔ ASIGNAR UN USUARIO A UN PROYECTO
        // ======================================================
        // POST /api/proyecto/{id}/usuarios/{usuarioId}
        public async Task<bool> AsignarUsuarioAsync(ProyectoUsuarioDTO dto)
        {
            var response = await _http.PostAsync(
                $"{BasePath}/{dto.IdProyecto}/usuarios/{dto.IdUsuario}",
                null);

            return response.IsSuccessStatusCode;
        }

        // ======================================================
        // ✔ DESASIGNAR UN USUARIO DE UN PROYECTO
        // ======================================================
        // DELETE /api/proyecto/{id}/usuarios/{usuarioId}
        public async Task<bool> DesasignarUsuarioAsync(ProyectoUsuarioDTO dto)
        {
            var response = await _http.DeleteAsync(
                $"{BasePath}/{dto.IdProyecto}/usuarios/{dto.IdUsuario}");

            return response.IsSuccessStatusCode;
        }

        // ======================================================
        // ✔ OBTENER TAREAS DE UN PROYECTO
        // ======================================================
        // OJO: Esto lo maneja el microservicio Tareas
        // GET /api/tarea/proyecto/{idProyecto}
        public async Task<List<TareaDTO>> GetTareasByProyectoAsync(int idProyecto)
        {
            var response = await _http.GetAsync($"api/tarea/Proyecto/{idProyecto}");

            if (!response.IsSuccessStatusCode)
                return new List<TareaDTO>();

            return await response.Content.ReadFromJsonAsync<List<TareaDTO>>()
                   ?? new List<TareaDTO>();
        }

    }
}

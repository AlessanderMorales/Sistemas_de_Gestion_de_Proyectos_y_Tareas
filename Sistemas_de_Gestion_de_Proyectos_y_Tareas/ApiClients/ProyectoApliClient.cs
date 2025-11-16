using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Proyectos;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients
{
    public class ProyectoApiClient
    {
        private readonly HttpClient _http;

        public ProyectoApiClient(HttpClient http)
        {
            _http = http;
        }

        // ================================
        // ✔ Obtener todos los proyectos
        // ================================
        public async Task<List<ProyectoDTO>> GetAllAsync()
        {
            var result = await _http.GetFromJsonAsync<List<ProyectoDTO>>("api/proyecto");
            return result ?? new List<ProyectoDTO>();
        }

        // ================================
        // ✔ Obtener por ID
        // ================================
        public async Task<ProyectoDTO?> GetAsync(int id)
        {
            return await _http.GetFromJsonAsync<ProyectoDTO>($"api/proyecto/{id}");
        }

        // ================================
        // ✔ Crear proyecto
        // ================================
        public async Task<bool> CreateAsync(ProyectoDTO dto)
        {
            var response = await _http.PostAsJsonAsync("api/proyecto", dto);
            return response.IsSuccessStatusCode;
        }

        // ================================
        // ✔ Actualizar proyecto
        // ================================
        public async Task<bool> UpdateAsync(ProyectoDTO dto)
        {
            var response = await _http.PutAsJsonAsync($"api/proyecto/{dto.Id}", dto);
            return response.IsSuccessStatusCode;
        }

        // ================================
        // ✔ Eliminar proyecto
        // ================================
        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/proyecto/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<ProyectoDTO?> GetByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<ProyectoDTO>($"api/proyectos/{id}");
        }

        public async Task<bool> UpdateAsync(int id, ProyectoDTO dto)
        {
            var response = await _http.PutAsJsonAsync($"api/proyectos/{id}", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<UsuarioDTO>> GetUsuariosAsignadosAsync(int idProyecto)
        {
            return await _http.GetFromJsonAsync<List<UsuarioDTO>>(
                $"proyectos/{idProyecto}/usuarios-asignados");
        }
        public async Task<bool> DesasignarUsuarioAsync(ProyectoUsuarioDTO dto)
        {
            var response = await _http.PostAsJsonAsync(
                "proyectos/desasignar-usuario", dto);

            return response.IsSuccessStatusCode;
        }
        public async Task<bool> AsignarUsuarioAsync(ProyectoUsuarioDTO dto)
        {
            var response = await _http.PostAsJsonAsync(
                "proyectos/asignar-usuario", dto);

            return response.IsSuccessStatusCode;
        }
        public async Task<List<ProyectoDTO>> GetByUsuarioAsync(int usuarioId)
        {
            return await _http.GetFromJsonAsync<List<ProyectoDTO>>($"proyectos/usuario/{usuarioId}");
        }
        public async Task<List<TareaDTO>> GetTareasByProyectoAsync(int idProyecto)
        {
            var response = await _http.GetAsync($"api/proyectos/{idProyecto}/tareas");

            if (!response.IsSuccessStatusCode)
                return new List<TareaDTO>();

            return await response.Content.ReadFromJsonAsync<List<TareaDTO>>();
        }
    }
}

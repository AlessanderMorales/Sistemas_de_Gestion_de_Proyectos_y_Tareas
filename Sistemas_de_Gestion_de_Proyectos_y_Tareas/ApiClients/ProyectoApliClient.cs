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
            => await _http.GetFromJsonAsync<ProyectoDTO>($"{BasePath}/{id}");

        public async Task<List<ProyectoDTO>> GetAllAsync()
            => await _http.GetFromJsonAsync<List<ProyectoDTO>>(BasePath) ?? new List<ProyectoDTO>();

        public async Task<bool> CreateAsync(ProyectoDTO dto)
            => (await _http.PostAsJsonAsync(BasePath, dto)).IsSuccessStatusCode;

        public async Task<bool> UpdateAsync(ProyectoDTO dto)
            => (await _http.PutAsJsonAsync($"{BasePath}/{dto.IdProyecto}", dto)).IsSuccessStatusCode;

        public async Task<bool> UpdateAsync(int id, ProyectoDTO dto)
            => (await _http.PutAsJsonAsync($"{BasePath}/{id}", dto)).IsSuccessStatusCode;

        public async Task<bool> DeleteAsync(int id)
            => (await _http.DeleteAsync($"{BasePath}/{id}")).IsSuccessStatusCode;

        public async Task<List<ProyectoDTO>> GetByUsuarioAsync(int usuarioId)
            => await _http.GetFromJsonAsync<List<ProyectoDTO>>($"{BasePath}/por-usuario/{usuarioId}")
               ?? new List<ProyectoDTO>();

        public async Task<List<UsuarioDTO>> GetUsuariosAsignadosAsync(int idProyecto)
            => await _http.GetFromJsonAsync<List<UsuarioDTO>>($"{BasePath}/{idProyecto}/usuarios")
               ?? new List<UsuarioDTO>();

        public async Task<bool> AsignarUsuarioAsync(ProyectoUsuarioDTO dto)
            => (await _http.PostAsync($"{BasePath}/{dto.IdProyecto}/usuarios/{dto.IdUsuario}", null)).IsSuccessStatusCode;

        public async Task<bool> DesasignarUsuarioAsync(ProyectoUsuarioDTO dto)
            => (await _http.DeleteAsync($"{BasePath}/{dto.IdProyecto}/usuarios/{dto.IdUsuario}")).IsSuccessStatusCode;

        public async Task<List<TareaDTO>> GetTareasByProyectoAsync(int idProyecto)
        {
            var response = await _http.GetAsync($"api/tarea/Proyecto/{idProyecto}");
            if (!response.IsSuccessStatusCode)
                return new List<TareaDTO>();

            return await response.Content.ReadFromJsonAsync<List<TareaDTO>>() ?? new List<TareaDTO>();
        }
    }
}

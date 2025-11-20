using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios;
using System.Net;
using System.Net.Http.Json;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients
{
    public class UsuarioApiClient
    {
        private readonly HttpClient _http;
        private const string BasePath = "api/usuario";

        public UsuarioApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<UsuarioDTO>> GetAllAsync()
            => await _http.GetFromJsonAsync<List<UsuarioDTO>>(BasePath) ?? new List<UsuarioDTO>();

        public async Task<UsuarioDTO?> GetByIdAsync(int id)
        {
            var response = await _http.GetAsync($"{BasePath}/{id}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<UsuarioDTO>();
        }

        public async Task<bool> CrearUsuarioAsync(UsuarioCrearDTO dto)
            => (await _http.PostAsJsonAsync(BasePath, dto)).IsSuccessStatusCode;

        public async Task<bool> UpdateAsync(UsuarioDTO dto)
            => (await _http.PutAsJsonAsync($"{BasePath}/{dto.Id}", dto)).IsSuccessStatusCode;

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync($"{BasePath}/{id}");
            return response.StatusCode == HttpStatusCode.OK
                || response.StatusCode == HttpStatusCode.NoContent;
        }

        public async Task<LoginResponseDTO?> LoginAsync(UsuarioLoginDTO dto)
        {
            var response = await _http.PostAsJsonAsync($"{BasePath}/login", dto);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<LoginResponseDTO>();
        }

        public async Task<bool> CambiarContraseñaAsync(int id, string actual, string nueva)
        {
            var payload = new
            {
                ContraseñaActual = actual,
                NuevaContraseña = nueva
            };
            return (await _http.PutAsJsonAsync($"{BasePath}/cambiar-contraseña/{id}", payload)).IsSuccessStatusCode;
        }
    }
}

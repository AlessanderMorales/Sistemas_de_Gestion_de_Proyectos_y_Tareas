using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients
{
    public class UsuarioApiClient
    {
         readonly HttpClient _http;

        public UsuarioApiClient(IHttpClientFactory f)
        {
            _http = f.CreateClient("usuarioApi");
        }

        public async Task<List<UsuarioDTO>> GetAll()
        {
            return await _http.GetFromJsonAsync<List<UsuarioDTO>>("/api/Usuario");
        }

        public async Task<bool> CrearUsuario(UsuarioDTO dto)
        {
            var response = await _http.PostAsJsonAsync("/api/Usuario", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<UsuarioDTO?> GetById(int id)
        {
            return await _http.GetFromJsonAsync<UsuarioDTO>($"/api/Usuario/{id}");
        }

        public async Task<bool> Update(UsuarioDTO dto)
        {
            var response = await _http.PutAsJsonAsync("/api/Usuario", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> Delete(int id)
        {
            var response = await _http.DeleteAsync($"/api/Usuario/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<UsuarioDTO?> GetAsync(int id)
        {
            return await _http.GetFromJsonAsync<UsuarioDTO>($"/api/usuario/{id}");
        }

        public async Task<List<UsuarioDTO>> GetAllAsync()
        {
            var result = await _http.GetFromJsonAsync<List<UsuarioDTO>>("api/usuario");
            return result ?? new List<UsuarioDTO>();
        }
        public async Task<UsuarioDTO?> GetByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<UsuarioDTO>($"api/usuarios/{id}");
        }
    }
}

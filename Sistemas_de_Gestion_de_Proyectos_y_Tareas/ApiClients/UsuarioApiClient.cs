using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios;
using System.Net;
using System.Net.Http.Json;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients
{
    public class UsuarioApiClient
    {
        private readonly HttpClient _http;
        private const string BasePath = "api/usuario";

        // 🔹 Constructor correcto para Typed HttpClient
        public UsuarioApiClient(HttpClient http)
        {
            _http = http;
        }

        // =========================================================
        //  MÉTODOS PRINCIPALES ASYNC
        // =========================================================

        public async Task<List<UsuarioDTO>> GetAllAsync()
        {
            var result = await _http.GetFromJsonAsync<List<UsuarioDTO>>(BasePath);
            return result ?? new List<UsuarioDTO>();
        }

        public async Task<UsuarioDTO?> GetByIdAsync(int id)
        {
            var response = await _http.GetAsync($"{BasePath}/{id}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[WARN] Usuario {id} no encontrado. Status: {response.StatusCode}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<UsuarioDTO>();
        }

        public async Task<bool> UpdateAsync(UsuarioDTO dto)
        {
            var response = await _http.PutAsJsonAsync($"{BasePath}/{dto.Id}", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync($"{BasePath}/{id}");
            var raw = await response.Content.ReadAsStringAsync();
            return response.StatusCode == HttpStatusCode.OK
                || response.StatusCode == HttpStatusCode.NoContent;
        }

        // =========================================================
        //  WRAPPERS COMPATIBLES CON TU CÓDIGO EXISTENTE
        //  (para que no den error tus Pages)
        // =========================================================

        // Index.cshtml.cs -> _usuarioApi.GetAll()
        public Task<List<UsuarioDTO>> GetAll() => GetAllAsync();

        // Edit.cshtml.cs -> _usuarioApi.GetById(id)
        public Task<UsuarioDTO?> GetById(int id) => GetByIdAsync(id);

        // Create.cshtml.cs -> _usuarioApi.CrearUsuario(dto)
        public Task<bool> CrearUsuario(UsuarioCrearDTO dto) => CrearUsuarioAsync(dto);

        // Edit.cshtml.cs -> _usuarioApi.Update(dto)
        public Task<bool> Update(UsuarioDTO dto) => UpdateAsync(dto);

        // Index.cshtml.cs -> _usuarioApi.Delete(id)
        public Task<bool> Delete(int id) => DeleteAsync(id);

        // Comentarios/Create.cshtml.cs -> _usuarioApi.GetAsync(id)
        public Task<UsuarioDTO?> GetAsync(int id) => GetByIdAsync(id);
        public async Task<UsuarioDTO?> LoginAsync(UsuarioLoginDTO dto)
        {
            var response = await _http.PostAsJsonAsync("/api/usuario/login", dto);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<UsuarioDTO>();
        }
        public async Task<bool> CrearUsuarioAsync(UsuarioCrearDTO dto)
        {
            var response = await _http.PostAsJsonAsync("api/usuario", dto);

            string log = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CambiarContraseñaAsync(int id, string actual, string nueva)
        {
            var payload = new
            {
                ContraseñaActual = actual,
                NuevaContraseña = nueva
            };

            var response = await _http.PutAsJsonAsync($"api/usuario/cambiar-contraseña/{id}", payload);

            return response.IsSuccessStatusCode;
        }


    }
}

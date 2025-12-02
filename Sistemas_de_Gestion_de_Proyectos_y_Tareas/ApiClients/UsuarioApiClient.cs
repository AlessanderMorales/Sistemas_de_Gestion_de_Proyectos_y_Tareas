using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

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

        public async Task<(bool success, string? errorMessage)> CrearUsuarioAsync(UsuarioCrearDTO dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync(BasePath, dto);

                if (response.IsSuccessStatusCode)
                    return (true, null);

                // Capturar el mensaje de error de la API
                var errorContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var errorObj = JsonSerializer.Deserialize<Dictionary<string, object>>(errorContent);
                    if (errorObj != null && errorObj.ContainsKey("message"))
                    {
                        return (false, errorObj["message"].ToString());
                    }
                }
                catch
                {
                }

                return (false, $"Error al crear usuario: {errorContent}");
            }
            catch (Exception ex)
            {
                return (false, $"Error de conexión: {ex.Message}");
            }
        }

        public async Task<(bool success, string? errorMessage)> UpdateAsync(int id, UsuarioActualizarDTO dto)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"{BasePath}/{id}", dto);

                if (response.IsSuccessStatusCode)
                    return (true, null);

                var errorContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var errorObj = JsonSerializer.Deserialize<Dictionary<string, object>>(errorContent);
                    if (errorObj != null && errorObj.ContainsKey("message"))
                    {
                        return (false, errorObj["message"].ToString());
                    }
                }
                catch
                {
                }

                return (false, $"Error al actualizar: {errorContent}");
            }
            catch (Exception ex)
            {
                return (false, $"Error de conexión: {ex.Message}");
            }
        }

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

using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Comentarios;
using System.Net.Http.Json;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients
{
    public class ComentarioApiClient
    {
        private readonly HttpClient _http;

        public ComentarioApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<IEnumerable<ComentarioDTO>> GetAllAsync()
            => await _http.GetFromJsonAsync<IEnumerable<ComentarioDTO>>("api/comentario")
               ?? new List<ComentarioDTO>();

        public async Task<ComentarioDTO?> GetByIdAsync(int id)
        {
            var response = await _http.GetAsync($"api/comentario/{id}");
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<ComentarioDTO>()
                : null;
        }

        public async Task<IEnumerable<ComentarioDTO>> GetByTareaAsync(int idTarea)
            => await _http.GetFromJsonAsync<IEnumerable<ComentarioDTO>>($"api/comentario/tarea/{idTarea}")
               ?? new List<ComentarioDTO>();

        public async Task<IEnumerable<ComentarioDTO>> GetByDestinatarioAsync(int idUsuario)
            => await _http.GetFromJsonAsync<IEnumerable<ComentarioDTO>>($"api/comentario/destinatario/{idUsuario}")
               ?? new List<ComentarioDTO>();

        public async Task<bool> CreateAsync(ComentarioDTO dto)
            => (await _http.PostAsJsonAsync("api/comentario", dto)).IsSuccessStatusCode;

        public async Task<bool> UpdateAsync(int id, ComentarioDTO dto)
            => (await _http.PutAsJsonAsync($"api/comentario/{id}", dto)).IsSuccessStatusCode;

        public async Task<bool> DeleteAsync(int id)
            => (await _http.DeleteAsync($"api/comentario/{id}")).IsSuccessStatusCode;
    }
}

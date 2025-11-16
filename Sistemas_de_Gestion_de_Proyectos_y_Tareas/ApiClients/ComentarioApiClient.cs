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

        // ✔ Listar todos
        public async Task<IEnumerable<ComentarioDTO>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<IEnumerable<ComentarioDTO>>("/api/comentarios")
                   ?? new List<ComentarioDTO>();
        }

        // ✔ Obtener por id
        public async Task<ComentarioDTO?> GetAsync(int id)
        {
            return await _http.GetFromJsonAsync<ComentarioDTO>($"/api/comentarios/{id}");
        }

        // ✔ Comentarios por tarea
        public async Task<IEnumerable<ComentarioDTO>> GetByTareaAsync(int idTarea)
        {
            return await _http.GetFromJsonAsync<IEnumerable<ComentarioDTO>>(
                $"/api/comentarios/tarea/{idTarea}"
            ) ?? new List<ComentarioDTO>();
        }

        // ✔ Comentarios destinados a un usuario
        public async Task<IEnumerable<ComentarioDTO>> GetByDestinatarioAsync(int idUsuario)
        {
            return await _http.GetFromJsonAsync<IEnumerable<ComentarioDTO>>(
                $"/api/comentarios/destinatario/{idUsuario}"
            ) ?? new List<ComentarioDTO>();
        }

        // ✔ Crear comentario
        public async Task<bool> CreateAsync(ComentarioDTO dto)
        {
            var res = await _http.PostAsJsonAsync("/api/comentarios", dto);
            return res.IsSuccessStatusCode;
        }

        // ✔ Editar comentario
        public async Task<bool> UpdateAsync(int id, ComentarioDTO dto)
        {
            var res = await _http.PutAsJsonAsync($"/api/comentarios/{id}", dto);
            return res.IsSuccessStatusCode;
        }

        // ✔ Eliminar comentario
        public async Task<bool> DeleteAsync(int id)
        {
            var res = await _http.DeleteAsync($"/api/comentarios/{id}");
            return res.IsSuccessStatusCode;
        }

        public async Task<ComentarioDTO?> GetByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<ComentarioDTO>($"api/comentarios/{id}");
        }
    }
}

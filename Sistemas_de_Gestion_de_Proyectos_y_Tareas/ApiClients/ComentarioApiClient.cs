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
            return await _http.GetFromJsonAsync<IEnumerable<ComentarioDTO>>("api/comentario")
                   ?? new List<ComentarioDTO>();
        }

        // ✔ Obtener por ID (alias corregido)
        public async Task<ComentarioDTO?> GetByIdAsync(int id)
        {
            var response = await _http.GetAsync($"api/comentario/{id}");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<ComentarioDTO>();
        }

        // ✔ Obtener por tarea
        public async Task<IEnumerable<ComentarioDTO>> GetByTareaAsync(int idTarea)
        {
            return await _http.GetFromJsonAsync<IEnumerable<ComentarioDTO>>(
                $"api/comentario/tarea/{idTarea}"
            ) ?? new List<ComentarioDTO>();
        }

        // ✔ Obtener por destinatario
        public async Task<IEnumerable<ComentarioDTO>> GetByDestinatarioAsync(int idUsuario)
        {
            return await _http.GetFromJsonAsync<IEnumerable<ComentarioDTO>>(
                $"api/comentario/destinatario/{idUsuario}"
            ) ?? new List<ComentarioDTO>();
        }

        // ✔ Crear
        public async Task<bool> CreateAsync(ComentarioDTO dto)
        {
            var res = await _http.PostAsJsonAsync("api/comentario", dto);
            return res.IsSuccessStatusCode;
        }

        // ✔ Actualizar
        public async Task<bool> UpdateAsync(int id, ComentarioDTO dto)
        {
            var res = await _http.PutAsJsonAsync($"api/comentario/{id}", dto);

            Console.WriteLine($"PUT → api/comentario/{id}");
            Console.WriteLine($"STATUS → {res.StatusCode}");

            if (!res.IsSuccessStatusCode)
            {
                var error = await res.Content.ReadAsStringAsync();
                Console.WriteLine($"ERROR BODY → {error}");
            }

            return res.IsSuccessStatusCode;
        }

        // ✔ Eliminar
        public async Task<bool> DeleteAsync(int id)
        {
            var res = await _http.DeleteAsync($"api/comentario/{id}");
            return res.IsSuccessStatusCode;
        }
    }
}

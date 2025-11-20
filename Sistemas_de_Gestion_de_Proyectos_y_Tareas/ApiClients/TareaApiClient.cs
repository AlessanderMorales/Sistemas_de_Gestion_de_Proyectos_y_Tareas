using System.Net.Http.Json;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas;

public class TareaApiClient
{
    private readonly HttpClient _http;

    public TareaApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<TareaDTO>> GetAllAsync()
        => await _http.GetFromJsonAsync<List<TareaDTO>>("api/tarea") ?? new List<TareaDTO>();

    public async Task<TareaDTO?> GetByIdAsync(int id)
        => await _http.GetFromJsonAsync<TareaDTO>($"api/tarea/{id}");

    public async Task<List<TareaDTO>> GetByUsuarioAsync(int idUsuario)
        => await _http.GetFromJsonAsync<List<TareaDTO>>($"api/tarea/usuario/{idUsuario}") ?? new List<TareaDTO>();

    public async Task<List<TareaDTO>> GetByProyectoAsync(int idProyecto)
        => await _http.GetFromJsonAsync<List<TareaDTO>>($"api/tarea/proyecto/{idProyecto}") ?? new List<TareaDTO>();

    public async Task<List<int>> GetUsuariosAsignadosAsync(int tareaId)
    {
        var response = await _http.GetAsync($"api/tarea/{tareaId}/usuarios");
        if (!response.IsSuccessStatusCode) return new List<int>();

        var data = await response.Content.ReadFromJsonAsync<List<UsuarioTareaDTO>>();
        return data?.Select(x => x.IdUsuario).ToList() ?? new List<int>();
    }

    public async Task<bool> CambiarEstadoAsync(int idTarea, CambiarEstadoTareaDTO dto)
        => (await _http.PutAsJsonAsync($"api/tarea/{idTarea}/estado", dto)).IsSuccessStatusCode;

    public async Task<bool> AsignarUsuariosAsync(AsignarUsuariosDTO dto)
        => (await _http.PostAsJsonAsync($"api/tarea/{dto.TareaId}/usuarios", dto.UsuariosIds)).IsSuccessStatusCode;

    public async Task<bool> CreateAsync(TareaDTO dto)
    {
        dto.Estado = 1;
        return (await _http.PostAsJsonAsync("api/tarea", dto)).IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(int id, TareaDTO dto)
        => (await _http.PutAsJsonAsync($"api/tarea/{id}", dto)).IsSuccessStatusCode;

    public async Task<bool> DeleteAsync(int id)
        => (await _http.DeleteAsync($"api/tarea/{id}")).IsSuccessStatusCode;
}

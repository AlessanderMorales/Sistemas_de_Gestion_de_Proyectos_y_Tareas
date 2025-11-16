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
    {
        var data = await _http.GetFromJsonAsync<List<TareaDTO>>("api/tarea");
        return data ?? new List<TareaDTO>();
    }

    // ✔ Obtener una tarea por ID
    public async Task<TareaDTO?> GetAsync(int id)
    {
        return await _http.GetFromJsonAsync<TareaDTO>($"api/tarea/{id}");
    }

    // ✔ Obtener ids de usuarios asignados
    public async Task<List<int>> GetUsuariosAsignadosAsync(int tareaId)
    {
        var response = await _http.GetAsync($"api/tarea/{tareaId}/usuarios");

        if (!response.IsSuccessStatusCode)
            return new List<int>();

        var data = await response.Content.ReadFromJsonAsync<List<UsuarioTareaDTO>>();

        return data?.Select(x => x.IdTarea).ToList() ?? new List<int>();
    }

    // ✔ Cambiar estado
    public async Task<bool> CambiarEstadoAsync(int idTarea, CambiarEstadoTareaDTO dto)
    {
        var response = await _http.PutAsJsonAsync($"api/tarea/{idTarea}/estado", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> AsignarUsuariosAsync(AsignarUsuariosDTO dto)
    {
        var response = await _http.PostAsJsonAsync(
            $"api/tarea/{dto.TareaId}/usuarios",
            dto.UsuariosIds
        );

        return response.IsSuccessStatusCode;
    }

    public async Task<List<TareaDTO>> GetByUsuarioAsync(int idUsuario)
    {
        var data = await _http.GetFromJsonAsync<List<TareaDTO>>(
            $"api/tarea/usuario/{idUsuario}");

        return data ?? new List<TareaDTO>();
    }

    public async Task<bool> CreateAsync(TareaDTO dto)
    {
        var response = await _http.PostAsJsonAsync("api/tarea", dto);
        return response.IsSuccessStatusCode;
    }
    public async Task<bool> UpdateAsync(int id, TareaDTO dto)
    {
        var response = await _http.PutAsJsonAsync($"api/tarea/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var res = await _http.DeleteAsync($"api/tarea/{id}");
        return res.IsSuccessStatusCode;
    }

    public async Task<List<int>> GetUsuariosAsignados(int id)
    {
        return await _http.GetFromJsonAsync<List<int>>($"api/tarea/{id}/usuarios");
    }
    public async Task<TareaDTO?> GetByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<TareaDTO>($"api/tareas/{id}");
    }

    // ✔ Obtener tareas por proyecto
    public async Task<List<TareaDTO>> GetByProyectoAsync(int idProyecto)
    {
        return await _http.GetFromJsonAsync<List<TareaDTO>>(
            $"api/tareas/proyecto/{idProyecto}")
            ?? new List<TareaDTO>();
    }
}

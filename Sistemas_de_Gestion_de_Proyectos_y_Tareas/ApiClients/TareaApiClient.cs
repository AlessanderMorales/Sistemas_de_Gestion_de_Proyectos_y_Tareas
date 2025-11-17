using System.Net.Http.Json;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas;

public class TareaApiClient
{
    private readonly HttpClient _http;

    public TareaApiClient(HttpClient http)
    {
        _http = http;
    }

    // ===============================
    // ✔ LISTAR TODAS LAS TAREAS
    // ===============================
    public async Task<List<TareaDTO>> GetAllAsync()
    {
        var data = await _http.GetFromJsonAsync<List<TareaDTO>>("api/tarea");
        return data ?? new List<TareaDTO>();
    }

    // ===============================
    // ✔ OBTENER UNA TAREA POR ID
    // ===============================
    public async Task<TareaDTO?> GetAsync(int id)
    {
        return await _http.GetFromJsonAsync<TareaDTO>($"api/tarea/{id}");
    }

    // ===============================
    // ✔ GET USUARIOS ASIGNADOS (TU MÉTODO VIEJO — AHORA CORREGIDO)
    // ===============================
    public async Task<List<int>> GetUsuariosAsignados(int id)
    {
        var data = await _http.GetFromJsonAsync<List<UsuarioTareaDTO>>(
            $"api/tarea/{id}/usuarios");

        // Tu método original devolvía ints → ahora corregido
        return data?.Select(x => x.IdUsuario).ToList() ?? new List<int>();
    }

    // ===============================
    // ✔ VERSIÓN NUEVA MÁS ROBUSTA
    // ===============================
    public async Task<List<int>> GetUsuariosAsignadosAsync(int tareaId)
    {
        var response = await _http.GetAsync($"api/tarea/{tareaId}/usuarios");
        if (!response.IsSuccessStatusCode)
            return new List<int>();

        var data = await response.Content.ReadFromJsonAsync<List<UsuarioTareaDTO>>();
        return data?.Select(x => x.IdUsuario).ToList() ?? new List<int>();
    }

    // ===============================
    // ✔ CAMBIAR ESTADO DE TAREA
    // ===============================
    public async Task<bool> CambiarEstadoAsync(int idTarea, CambiarEstadoTareaDTO dto)
    {
        var response = await _http.PutAsJsonAsync($"api/tarea/{idTarea}/estado", dto);
        return response.IsSuccessStatusCode;
    }

    // ===============================
    // ✔ ASIGNAR USUARIOS A UNA TAREA
    // ===============================
    public async Task<bool> AsignarUsuariosAsync(AsignarUsuariosDTO dto)
    {
        var response = await _http.PostAsJsonAsync(
            $"api/tarea/{dto.TareaId}/usuarios",
            dto.UsuariosIds
        );

        return response.IsSuccessStatusCode;
    }

    // ===============================
    // ✔ GET TAREAS POR USUARIO
    // ===============================
    public async Task<List<TareaDTO>> GetByUsuarioAsync(int idUsuario)
    {
        return await _http.GetFromJsonAsync<List<TareaDTO>>(
            $"api/tarea/usuario/{idUsuario}"
        ) ?? new List<TareaDTO>();
    }

    // ===============================
    // ✔ CREAR TAREA
    // ===============================
    public async Task<bool> CreateAsync(TareaDTO dto)
    {
        dto.Estado = 1;
        var response = await _http.PostAsJsonAsync("api/tarea", dto);
        return response.IsSuccessStatusCode;
    }

    // ===============================
    // ✔ ACTUALIZAR TAREA
    // ===============================
    public async Task<bool> UpdateAsync(int id, TareaDTO dto)
    {
        var response = await _http.PutAsJsonAsync($"api/tarea/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    // ===============================
    // ✔ DELETE (CORREGIDO)
    // ===============================
    public async Task<bool> DeleteAsync(int id)
    {
        var res = await _http.DeleteAsync($"api/tarea/{id}");

        // Por si tu API devuelve 204 sin contenido
        if (!res.IsSuccessStatusCode)
            return false;

        return true;
    }

    // ===============================
    // ✔ GET POR ID (OTRA VERSIÓN — UNIFICADA)
    // ===============================
    public async Task<TareaDTO?> GetByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<TareaDTO>($"api/tarea/{id}");
    }

    // ===============================
    // ✔ GET TAREAS POR PROYECTO
    // ===============================
    public async Task<List<TareaDTO>> GetByProyectoAsync(int idProyecto)
    {
        return await _http.GetFromJsonAsync<List<TareaDTO>>(
            $"api/tarea/proyecto/{idProyecto}"
        ) ?? new List<TareaDTO>();
    }
}

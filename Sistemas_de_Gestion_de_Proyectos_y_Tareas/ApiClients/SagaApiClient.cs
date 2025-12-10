using System.Text;
using System.Text.Json;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients
{
    public class SagaApiClient
    {
        private readonly HttpClient _httpClient;
        private const string BASE_URL = "http://localhost:5005/api/saga";

        public SagaApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Crear tarea con empleados asignados usando Saga
        /// </summary>
        public async Task<(bool success, string message)> CrearTareaConEmpleadosSagaAsync(
            string titulo,
            string? descripcion,
            string prioridad,
            int idProyecto,
            List<int> empleadosIds)
        {
            try
            {
                var request = new
                {
                    titulo,
                    descripcion,
                    prioridad,
                    idProyecto,
                    status = "SinIniciar",
                    empleadosIds
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{BASE_URL}/crear-tarea-con-empleados", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Tarea creada y empleados asignados exitosamente.");
                }
                else
                {
                    // Intentar extraer el mensaje de error
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
                        if (errorResponse != null && errorResponse.ContainsKey("message"))
                        {
                            return (false, errorResponse["message"].ToString() ?? "Error desconocido");
                        }
                    }
                    catch
                    {
                        // Si falla el parseo, devolver el contenido crudo
                    }

                    return (false, responseContent);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error al comunicarse con el orquestador de Sagas: {ex.Message}");
            }
        }
    }
}

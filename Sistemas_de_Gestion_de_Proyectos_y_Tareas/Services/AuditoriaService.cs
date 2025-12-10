using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Services
{
    /// <summary>
    /// Servicio para registrar operaciones de auditoría en el sistema
    /// </summary>
    public class AuditoriaService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuditoriaService> _logger;
        private const string AUDITORIA_API_URL = "http://localhost:5001/api/auditoria"; // MicroservicioUsuario

        public AuditoriaService(HttpClient httpClient, ILogger<AuditoriaService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Registra una operación CRUD en la auditoría
        /// </summary>
        public async Task<bool> RegistrarOperacionAsync(AuditoriaRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(AUDITORIA_API_URL, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Auditoría registrada: {request.Operacion} en {request.Tabla} por usuario {request.IdUsuario}");
                    return true;
                }
                else
                {
                    _logger.LogWarning($"Error al registrar auditoría: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Excepción al registrar auditoría: {ex.Message}");
                // No lanzamos excepción para no interrumpir la operación principal
                return false;
            }
        }

        /// <summary>
        /// Registra una operación INSERT
        /// </summary>
        public async Task RegistrarCreacionAsync(string tabla, int registroId, int idUsuario, string nombreUsuario, object datosNuevos, string ipUsuario = null)
        {
            var request = new AuditoriaRequest
            {
                Tabla = tabla,
                RegistroId = registroId,
                Operacion = "INSERT",
                IdUsuario = idUsuario,
                NombreUsuario = nombreUsuario,
                DatosAnteriores = null,
                DatosNuevos = JsonSerializer.Serialize(datosNuevos),
                IpUsuario = ipUsuario ?? "N/A"
            };

            await RegistrarOperacionAsync(request);
        }

        /// <summary>
        /// Registra una operación UPDATE
        /// </summary>
        public async Task RegistrarActualizacionAsync(string tabla, int registroId, int idUsuario, string nombreUsuario, object datosAnteriores, object datosNuevos, string ipUsuario = null)
        {
            var request = new AuditoriaRequest
            {
                Tabla = tabla,
                RegistroId = registroId,
                Operacion = "UPDATE",
                IdUsuario = idUsuario,
                NombreUsuario = nombreUsuario,
                DatosAnteriores = JsonSerializer.Serialize(datosAnteriores),
                DatosNuevos = JsonSerializer.Serialize(datosNuevos),
                IpUsuario = ipUsuario ?? "N/A"
            };

            await RegistrarOperacionAsync(request);
        }

        /// <summary>
        /// Registra una operación DELETE
        /// </summary>
        public async Task RegistrarEliminacionAsync(string tabla, int registroId, int idUsuario, string nombreUsuario, object datosAnteriores, string ipUsuario = null)
        {
            var request = new AuditoriaRequest
            {
                Tabla = tabla,
                RegistroId = registroId,
                Operacion = "DELETE",
                IdUsuario = idUsuario,
                NombreUsuario = nombreUsuario,
                DatosAnteriores = JsonSerializer.Serialize(datosAnteriores),
                DatosNuevos = null,
                IpUsuario = ipUsuario ?? "N/A"
            };

            await RegistrarOperacionAsync(request);
        }

        /// <summary>
        /// Obtiene el historial de auditoría de un usuario
        /// </summary>
        public async Task<List<AuditoriaResponse>> ObtenerAuditoriaPorUsuarioAsync(int idUsuario, int limite = 50)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{AUDITORIA_API_URL}/usuario/{idUsuario}?limite={limite}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<AuditoriaResponse>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<AuditoriaResponse>();
                }
                
                return new List<AuditoriaResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener auditoría por usuario: {ex.Message}");
                return new List<AuditoriaResponse>();
            }
        }

        /// <summary>
        /// Obtiene el historial de auditoría de una tabla
        /// </summary>
        public async Task<List<AuditoriaResponse>> ObtenerAuditoriaPorTablaAsync(string tabla, int limite = 50)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{AUDITORIA_API_URL}/tabla/{tabla}?limite={limite}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<AuditoriaResponse>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<AuditoriaResponse>();
                }
                
                return new List<AuditoriaResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener auditoría por tabla: {ex.Message}");
                return new List<AuditoriaResponse>();
            }
        }
    }

    /// <summary>
    /// Request para registrar auditoría
    /// </summary>
    public class AuditoriaRequest
    {
        public string Tabla { get; set; } = string.Empty;
        public int RegistroId { get; set; }
        public string Operacion { get; set; } = string.Empty;
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string? DatosAnteriores { get; set; }
        public string? DatosNuevos { get; set; }
        public string IpUsuario { get; set; } = "N/A";
    }

    /// <summary>
    /// Response de consulta de auditoría
    /// </summary>
    public class AuditoriaResponse
    {
        public int Id { get; set; }
        public string Tabla { get; set; } = string.Empty;
        public int RegistroId { get; set; }
        public string Operacion { get; set; } = string.Empty;
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string? DatosAnteriores { get; set; }
        public string? DatosNuevos { get; set; }
        public string? IpUsuario { get; set; }

        public string OperacionIcono => Operacion switch
        {
            "INSERT" => "? Crear",
            "UPDATE" => "?? Editar",
            "DELETE" => "??? Eliminar",
            _ => Operacion
        };
    }
}

using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas;
using System.Text.Json.Serialization;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Proyectos
{
    public class ProyectoDTO
    {
        [JsonPropertyName("idProyecto")]
        public int IdProyecto { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }

        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        public int Estado { get; set; }
        public int CantidadUsuarios { get; set; }
        public List<int>? UsuariosIds { get; set; }
        public List<string>? UsuariosNombres { get; set; }

        public List<TareaDTO> Tareas { get; set; } = new();
    }
}

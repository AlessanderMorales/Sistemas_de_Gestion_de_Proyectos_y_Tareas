using System.Text.Json.Serialization;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas
{
    public class UsuarioTareaDTO
    {
        [JsonPropertyName("idUsuario")]
        public int IdUsuario { get; set; }
    }
}

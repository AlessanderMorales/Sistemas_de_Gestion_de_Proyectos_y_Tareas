using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios;
using System.Text.Json.Serialization;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Comentarios
{
    public class ComentarioDTO
    {
        [JsonPropertyName("IdComentario")]
        public int IdComentario { get; set; }
        public string Contenido { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }

        public int IdTarea { get; set; }
        public int IdUsuario { get; set; }          // autor
        public int? IdDestinatario { get; set; }     // opcional
        public int Estado { get; set; }

        [ValidateNever]
        public UsuarioDTO Usuario { get; set; }

        [ValidateNever]
        public TareaDTO Tarea { get; set; }
    }
}

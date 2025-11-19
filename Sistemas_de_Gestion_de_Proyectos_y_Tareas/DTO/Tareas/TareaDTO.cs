// EN TU PROYECTO RAZOR PAGES: Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas/TareaDTO.cs
using System;
using System.Text.Json.Serialization;

// NO LO PONGAS FUERA DE UN NAMESPACE. DEBERÍA ESTAR DENTRO DE Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas
namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas
{
    public class TareaDTO
    {
        [JsonPropertyName("idTarea")]
        public int Id { get; set; }

        [JsonPropertyName("titulo")]
        public string Titulo { get; set; } = "";

        [JsonPropertyName("descripcion")]
        public string? Descripcion { get; set; }

        [JsonPropertyName("estado")]
        public int Estado { get; set; }

        [JsonPropertyName("fechaRegistro")] // <-- ¡CRÍTICO! Cambiado para coincidir con el JSON y evitar el error de compilación
        public DateTime FechaRegistro { get; set; }

        [JsonPropertyName("ultimaModificacion")] // <-- ¡CRÍTICO! Añadida para coincidir con el JSON
        public DateTime UltimaModificacion { get; set; }

        [JsonPropertyName("idProyecto")]
        public int? IdProyecto { get; set; } // <-- ¡CRÍTICO! Hecho nullable para mayor seguridad y evitar errores de conversión 'int?' a 'int'

        [JsonPropertyName("proyectoNombre")]
        public string? ProyectoNombre { get; set; }

        [JsonPropertyName("prioridad")]
        public string? Prioridad { get; set; } = ""; // Nullable por consistencia

        [JsonPropertyName("status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("usuarioAsignadoNombre")]
        public string? UsuarioAsignadoNombre { get; set; }

        [JsonPropertyName("idUsuarioAsignado")]
        public int? IdUsuarioAsignado { get; set; } // Ya era nullable, ¡excelente!
    }
}
using System.Text.Json.Serialization;

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

    [JsonPropertyName("fechaCreacion")]
    public DateTime FechaCreacion { get; set; }

    [JsonPropertyName("idProyecto")]
    public int IdProyecto { get; set; }

    [JsonPropertyName("proyectoNombre")]
    public string? ProyectoNombre { get; set; }

    [JsonPropertyName("prioridad")]
    public string Prioridad { get; set; } = "";

    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    [JsonPropertyName("usuarioAsignadoNombre")]
    public string? UsuarioAsignadoNombre { get; set; }

    [JsonPropertyName("idUsuarioAsignado")]
    public int? IdUsuarioAsignado { get; set; }
}

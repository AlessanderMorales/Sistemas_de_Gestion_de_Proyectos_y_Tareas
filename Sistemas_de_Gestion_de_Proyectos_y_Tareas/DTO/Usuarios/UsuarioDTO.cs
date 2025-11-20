using System.Text.Json.Serialization;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios
{
    public class UsuarioDTO
    {
        [JsonPropertyName("id_Usuario")]
        public int Id { get; set; }
        public string Nombres { get; set; }
        public string PrimerApellido { get; set; }
        public string? SegundoApellido { get; set; }
        public string Email { get; set; }
        public string NombreUsuario { get; set; }
        public string Rol { get; set; }
        public bool RequiereCambioContraseña { get; internal set; }

        [JsonPropertyName("estado")]
        public int Estado { get; set; }
    }
}

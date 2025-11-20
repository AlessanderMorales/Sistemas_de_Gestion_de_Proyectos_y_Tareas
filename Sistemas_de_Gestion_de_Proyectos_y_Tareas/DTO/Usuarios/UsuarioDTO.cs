using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios
{
    public class UsuarioDTO
    {
        [JsonPropertyName("id_Usuario")]
        public int Id { get; set; }
        
        [Display(Name = "Nombres")]
        public string Nombres { get; set; }
        
        [Display(Name = "Primer Apellido")]
        public string PrimerApellido { get; set; }
        
        [Display(Name = "Segundo Apellido")]
        public string? SegundoApellido { get; set; }
      
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Display(Name = "Nombre de Usuario")]
        public string NombreUsuario { get; set; }
        
        [Display(Name = "Rol")]
        public string Rol { get; set; }
        
        [Display(Name = "Requiere Cambio de Contraseña")]
        public bool RequiereCambioContraseña { get; internal set; }

        [JsonPropertyName("estado")]
        [Display(Name = "Estado")]
        public int Estado { get; set; }
    }
}

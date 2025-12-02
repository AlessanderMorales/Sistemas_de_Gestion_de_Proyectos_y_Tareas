using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios
{
    public class UsuarioCrearDTO
    {
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

        [Display(Name = "Contraseña")]
        public string Contraseña { get; set; }
    }
}

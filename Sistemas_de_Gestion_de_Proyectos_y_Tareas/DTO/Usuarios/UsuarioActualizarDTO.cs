using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios
{

    public class UsuarioActualizarDTO
    {
        [Required(ErrorMessage = "Los nombres son requeridos")]
        [Display(Name = "Nombres")]
      public string Nombres { get; set; } = "";
        
        [Required(ErrorMessage = "El primer apellido es requerido")]
        [Display(Name = "Primer Apellido")]
        public string PrimerApellido { get; set; } = "";
        
        [Display(Name = "Segundo Apellido")]
        public string? SegundoApellido { get; set; }
  
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";
      
        [Required(ErrorMessage = "El rol es requerido")]
        [Display(Name = "Rol")]
        public string Rol { get; set; } = "";
  }
}

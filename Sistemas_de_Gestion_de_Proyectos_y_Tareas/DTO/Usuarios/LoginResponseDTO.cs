using System.Text.Json.Serialization;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios
{
    public class LoginResponseDTO
    {
  [JsonPropertyName("error")]
  public bool Error { get; set; }
        
        [JsonPropertyName("message")]
     public string Message { get; set; } = string.Empty;
        
      [JsonPropertyName("token")]
 public string Token { get; set; } = string.Empty;
    
    [JsonPropertyName("id_Usuario")]
   public int Id_Usuario { get; set; }
        
        [JsonPropertyName("nombres")]
        public string Nombres { get; set; } = string.Empty;
     
  [JsonPropertyName("primerApellido")]
  public string PrimerApellido { get; set; } = string.Empty;
    
   [JsonPropertyName("segundoApellido")]
   public string? SegundoApellido { get; set; }
      
        [JsonPropertyName("nombreUsuario")]
  public string NombreUsuario { get; set; } = string.Empty;
        
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        
  [JsonPropertyName("rol")]
 public string Rol { get; set; } = string.Empty;
   
   [JsonPropertyName("requiereCambioContraseña")]
     public bool RequiereCambioContraseña { get; set; }
    }
}

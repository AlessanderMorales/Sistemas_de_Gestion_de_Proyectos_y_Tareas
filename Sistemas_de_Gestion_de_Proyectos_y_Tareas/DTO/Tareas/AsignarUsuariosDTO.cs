namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas
{
    public class AsignarUsuariosDTO
    {
        public int TareaId { get; set; }
        public List<int> UsuariosIds { get; set; } = new();
    }
}

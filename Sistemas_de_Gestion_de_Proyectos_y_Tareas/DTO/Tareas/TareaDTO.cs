namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas
{
    public class TareaDTO
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = "";
        public string? Descripcion { get; set; }
        public int Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int IdProyecto { get; set; }
        public string? ProyectoNombre { get; set; }
        public string Prioridad { get; set; }
        public string Status { get; set; }
        public string UsuarioAsignadoNombre { get; set; }
        public int? IdUsuarioAsignado { get; set; }

    }
}

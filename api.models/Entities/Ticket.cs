namespace api.models.Entities
{
    public class Ticket
    {
        public int Id { get; set; }
        public string FechaAlta { get; set; } = string.Empty;
        public int Autor { get; set; }
        public int Status { get; set; }
        public string? Asignado { get; set; }
        public string Progress { get; set; } = "0%";
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int AreaAutor { get; set; }
        public int AreaTicket { get; set; }
        public string? FechaUpdate { get; set; }
        public bool Visible { get; set; } = true;
    }
}

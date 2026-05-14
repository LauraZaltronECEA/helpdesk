namespace api.models.DTO
{
    public class TicketDTO
    {
        public int Autor { get; set; }
        public int Status { get; set; }
        public string? Asignado { get; set; }
        public string? Progress { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int AreaAutor { get; set; }
        public int AreaTicket { get; set; }
    }
}

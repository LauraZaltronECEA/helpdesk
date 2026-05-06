
namespace api.models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Fullname { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Active { get; set; } = "T";
        public DateTime Last_Login { get; set; } = DateTime.Now;//cambiar
    }
}

using Microsoft.AspNetCore.Identity;

namespace api.models;

public class AppUser : IdentityUser<int>
{
    public string Fullname { get; set; } = string.Empty;
    public string Active { get; set; } = "T";
    public string Role { get; set; } = "user";
    public DateTime Last_Login { get; set; } = DateTime.Now;
}

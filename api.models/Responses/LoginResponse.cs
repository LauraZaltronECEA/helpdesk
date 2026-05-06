namespace api.models.Responses
{
    public class LoginResponse : GeneralResponse
    {
        public string Token { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
    }
}

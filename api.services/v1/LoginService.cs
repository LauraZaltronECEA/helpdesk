using api.models.DTO;
using api.models.Entities;
using api.models.Responses;
using api.services.Handlers;
using api.services.Repositores;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace api.services.v1
{
    public class LoginService : IUserRepository
    {
        private readonly IConfiguration _configuration;

        public LoginService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<LoginResponse> Login(UserLoginDTO user)
        {
            string username = Escape(user.Username);
            string query = $"select * from users where username = '{username}'";
            string json = SqliteHandler.GetJson(query);

            LoginResponse result = new();

            if (json == "[]")
            {
                result.Estado = false;
                result.Codigo = 0;
                result.Mensaje = "Credenciales invalidas";
                result.Token = string.Empty;

                return await Task.FromResult(result);
            }

            var userList = JsonConvert.DeserializeObject<List<User>>(json);
            var userDb = userList?.FirstOrDefault();

            if (userDb == null || !PasswordMatches(user.Password, userDb.Password))
            {
                result.Estado = false;
                result.Codigo = 0;
                result.Mensaje = "Credenciales invalidas";
                result.Token = string.Empty;

                return await Task.FromResult(result);
            }

            if (userDb.Active != "T")
            {
                result.Estado = false;
                result.Codigo = 0;
                result.Mensaje = "Usuario inactivo";
                result.Active = userDb.Active;
                result.Token = string.Empty;

                return await Task.FromResult(result);
            }

            string fechaLogin = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            SqliteHandler.Exec($"update users set last_login = '{fechaLogin}' where id = {userDb.Id}");

            JwtHandler jwt = new(_configuration);

            result.Estado = true;
            result.Codigo = 1;
            result.Mensaje = "Login exitoso";
            result.UserId = userDb.Id;
            result.Username = userDb.Username;
            result.Role = userDb.Role;
            result.Active = userDb.Active;
            result.Token = jwt.CrearJWT(userDb.Username, userDb.Id, userDb.Fullname, userDb.Role);

            return await Task.FromResult(result);
        }

        public async Task<GeneralResponse> Register(UserRegisterDTO user)
        {
            GeneralResponse result = new();

            string username = Escape(user.Username);
            string checkQuery = $"select * from users where username = '{username}'";
            string checkJson = SqliteHandler.GetJson(checkQuery);

            if (checkJson != "[]")
            {
                result.Estado = false;
                result.Codigo = 0;
                result.Mensaje = "El nombre de usuario ya existe";

                return await Task.FromResult(result);
            }

            string passwordHash = EncriptHandler.Hash(user.Password);
            string fullname = Escape(user.Fullname);
            string email = Escape(user.Email);

            string query = "insert into users (username, password, fullname, email, role, active, last_login) " +
                $"values ('{username}', '{passwordHash}', '{fullname}', '{email}', 'user', 'T', '')";

            bool success = SqliteHandler.Exec(query);

            result.Estado = success;
            result.Codigo = success ? 1 : 0;
            result.Mensaje = success ? "Usuario registrado correctamente" : "Error al registrar usuario";

            return await Task.FromResult(result);
        }

        private static bool PasswordMatches(string password, string storedPassword)
        {
            return EncriptHandler.Verify(password, storedPassword) || password == storedPassword;
        }

        private static string Escape(string value)
        {
            return value.Replace("'", "''");
        }
    }
}

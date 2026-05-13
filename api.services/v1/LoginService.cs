using api.models;
using api.models.DTO;
using api.models.Responses;
using api.services.Handlers;
using api.services.Repositores;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace api.services.v1
{
    public class LoginService : IUserRepository
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;

        public LoginService(UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<LoginResponse> Login(UserLoginDTO user)
        {
            LoginResponse result = new();

            var appUser = await _userManager.FindByNameAsync(user.Username);

            if (appUser == null)
            {
                result.Estado = false;
                result.Codigo = 0;
                result.Mensaje = "Credenciales invalidas";
                result.Token = string.Empty;
                return result;
            }

            if (!await _userManager.CheckPasswordAsync(appUser, user.Password))
            {
                result.Estado = false;
                result.Codigo = 0;
                result.Mensaje = "Credenciales invalidas";
                result.Token = string.Empty;
                return result;
            }

            if (!await _userManager.IsEmailConfirmedAsync(appUser))
            {
                result.Estado = false;
                result.Codigo = 0;
                result.Mensaje = "Debes confirmar tu correo electronico antes de iniciar sesion";
                result.Token = string.Empty;
                return result;
            }

            if (appUser.Active != "T")
            {
                result.Estado = false;
                result.Codigo = 0;
                result.Mensaje = "Usuario inactivo";
                result.Active = appUser.Active;
                result.Token = string.Empty;
                return result;
            }

            appUser.Last_Login = DateTime.Now;
            await _userManager.UpdateAsync(appUser);

            JwtHandler jwt = new(_configuration);

            result.Estado = true;
            result.Codigo = 1;
            result.Mensaje = "Login exitoso";
            result.UserId = appUser.Id;
            result.Username = appUser.UserName;
            result.Role = appUser.Role;
            result.Active = appUser.Active;
            result.Token = jwt.CrearJWT(appUser.UserName, appUser.Id, appUser.Fullname, appUser.Role);

            return result;
        }
    }
}
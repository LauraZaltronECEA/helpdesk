using api.models;
using api.models.DTO;
using api.models.Responses;
using api.services.Repositores;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace api.helpdesk.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailSender<AppUser> _emailSender;
    private readonly IConfiguration _configuration;

    public UserController(
        IUserRepository userRepository,
        UserManager<AppUser> userManager,
        IEmailSender<AppUser> emailSender,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _emailSender = emailSender;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] UserLoginDTO user)
    {
        var result = await _userRepository.Login(user);
        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult<GeneralResponse>> Register([FromBody] UserRegisterDTO user)
    {
        GeneralResponse result = new();

        var existingUser = await _userManager.FindByNameAsync(user.Username);
        if (existingUser != null)
        {
            result.Estado = false;
            result.Codigo = 0;
            result.Mensaje = "El nombre de usuario ya existe";
            return Ok(result);
        }

        var existingEmail = await _userManager.FindByEmailAsync(user.Email);
        if (existingEmail != null)
        {
            result.Estado = false;
            result.Codigo = 0;
            result.Mensaje = "El correo electronico ya esta registrado";
            return Ok(result);
        }

        var appUser = new AppUser
        {
            UserName = user.Username,
            Email = user.Email,
            Fullname = user.Fullname,
            Role = "user",
            Active = "T"
        };

        var createResult = await _userManager.CreateAsync(appUser, user.Password);

        if (!createResult.Succeeded)
        {
            result.Estado = false;
            result.Codigo = 0;
            result.Mensaje = string.Join(". ", createResult.Errors.Select(e => e.Description));
            return Ok(result);
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
        var encodedToken = Uri.EscapeDataString(token);

        var request = HttpContext.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}";
        var confirmationLink = $"{baseUrl}/api/v1/user/confirm-email?userId={appUser.Id}&token={encodedToken}";

        await _emailSender.SendConfirmationLinkAsync(appUser, user.Email, confirmationLink);

        result.Estado = true;
        result.Codigo = 1;
        result.Mensaje = "Usuario registrado correctamente. Revisa tu correo para confirmar la cuenta.";

        return Ok(result);
    }

    [HttpGet("confirm-email")]
    public async Task<ActionResult<GeneralResponse>> ConfirmEmail([FromQuery] int userId, [FromQuery] string token)
    {
        GeneralResponse result = new();

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            result.Estado = false;
            result.Codigo = 0;
            result.Mensaje = "Usuario no encontrado";
            return Ok(result);
        }

        var decodedToken = Uri.UnescapeDataString(token);
        var confirmResult = await _userManager.ConfirmEmailAsync(user, decodedToken);

        if (!confirmResult.Succeeded)
        {
            result.Estado = false;
            result.Codigo = 0;
            result.Mensaje = "El enlace de confirmacion es invalido o ha expirado";
            return Ok(result);
        }

        result.Estado = true;
        result.Codigo = 1;
        result.Mensaje = "Correo electronico confirmado exitosamente. Ya puedes iniciar sesion.";

        return Ok(result);
    }
}

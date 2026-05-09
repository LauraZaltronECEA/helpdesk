using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace api.services.Handlers
{
    public class JwtHandler
    {
        private readonly IConfiguration _configuration;

        public JwtHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CrearJWT(string usuario, int? idUsuario, string nombre, string? role = null)
        {
            var jwt = _configuration.GetSection("Jwt");
            var secret = jwt["Secret"] ?? throw new InvalidOperationException("Jwt: Secret no configurado");
            var issuer = jwt["Issuer"] ?? "api.helpdesk";
            var audience = jwt["Audience"] ?? "helpdesk";
            var minutesConfig = jwt["ExpirationMinutes"] ?? jwt["ExpireMinutes"];
            var minutes = int.TryParse(minutesConfig, out var parsedMinutes) ? parsedMinutes : 120;

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, usuario),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.Name, usuario)
            };

            if (idUsuario.HasValue)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, idUsuario.Value.ToString()));
            }

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                claims.Add(new Claim(ClaimTypes.GivenName, nombre));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(minutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

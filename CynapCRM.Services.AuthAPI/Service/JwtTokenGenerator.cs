using CynapCRM.Services.AuthAPI.Models;
using CynapCRM.Services.AuthAPI.Service.IService;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CynapCRM.Services.AuthAPI.Service
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtOptions _jwtOptions;

        public JwtTokenGenerator(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
        }


        public string GenerateToken(Utilisateur user, IEnumerable<string> roles)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            // FIX 11: env var takes precedence over appsettings secret
            var secret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? _jwtOptions.Secret;
            var key = Encoding.UTF8.GetBytes(secret);

            var claims = new List<Claim>
    {
        // Identity
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),

        // Email 
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(ClaimTypes.Email, user.Email), // 

        // Unique token identifier
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

        // Issued at
        new Claim(JwtRegisteredClaimNames.Iat,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
            ClaimValueTypes.Integer64)
    };

            // Rôles (Authorize(Roles = "..."))
            claims.AddRange(
                roles.Select(role =>
                    new Claim(ClaimTypes.Role, role.ToUpper()))
            );

            var now = DateTime.UtcNow;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience,

                // Temporal security
                NotBefore = now,
                Expires = now.AddMinutes(_jwtOptions.ExpiryMinutes),

                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}

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

            // On convertit la clé secrète en tableau d'octets (Bytes)
            var key = Encoding.ASCII.GetBytes(_jwtOptions.Secret);

            // 1. On définit le contenu du Token (Les Claims)
            var claimList = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.Name)
            };

            // 2. On ajoute les rôles de l'utilisateur dans le Token
            claimList.AddRange(roles.Select(role => new Claim("role", role.ToUpper())));
            // 3. Configuration du Token (Durée, Signature, Cible)
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = _jwtOptions.Audience,
                Issuer = _jwtOptions.Issuer,
                Subject = new ClaimsIdentity(claimList),
                Expires = DateTime.UtcNow.AddDays(7), // Le token expire après 7 jours
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            // 4. Création et écriture du Token final
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

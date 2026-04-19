using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CynapCRM.Services.AuthAPI.Extensions
{
    public static class WebApplicationBuilderExtensions
    {
        public static WebApplicationBuilder AddAppAuthentication(
            this WebApplicationBuilder builder)
        {
            // ✅ Nettoyage des claims par défaut
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            var settingsSection = builder.Configuration.GetSection("ApiSettings:JwtOptions");

            var secret = settingsSection.GetValue<string>("Secret");
            var issuer = settingsSection.GetValue<string>("Issuer");
            var audience = settingsSection.GetValue<string>("Audience");

            var key = Encoding.UTF8.GetBytes(secret);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // ✅ Sécurité clé
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),

                        // ✅ Émetteur / audience
                        ValidateIssuer = true,
                        ValidIssuer = issuer,

                        ValidateAudience = true,
                        ValidAudience = audience,

                        // ✅ EXPIRATION DU TOKEN (IMPORTANT)
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,

                        // ✅ RÔLES COMPATIBLES AVEC [Authorize(Roles = "...")]
                        RoleClaimType = ClaimTypes.Role
                    };
                });

            // ✅ OBLIGATOIRE
            builder.Services.AddAuthorization();

            return builder;
        }
    }
}
using CynapCRM.Services.AuthAPI.Models;

namespace CynapCRM.Services.AuthAPI.Service.IService
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(Utilisateur user, IEnumerable<string> roles);
    }
}

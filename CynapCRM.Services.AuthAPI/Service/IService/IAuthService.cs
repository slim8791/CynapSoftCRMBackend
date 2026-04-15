using CynapCRM.Services.AuthAPI.Models;
using CynapCRM.Services.AuthAPI.Models.Dto;

namespace CynapCRM.Services.AuthAPI.Service.IService
{
    public interface IAuthService
    {
        Task<string> Register(RegistrationRequestDto registrationRequestDto);

        Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto);
        //Task<bool> EnsureRoleExistsAndAssign(Utilisateur user, string roleName);
        Task<bool> AssignRole(string email, string roleName);
        Task<bool> AddRole(string email, string roleName);
        Task<bool> ChangePassword(ChangePasswordDto model);
        Task<ResponseDto> GeneratePasswordResetToken(string email);
        //Task<ResponseDto> ResetPassword(string email, string token, string newPassword);
        Task<bool> ChangeRole(ChangeRoleDto model);
        Task<bool> EnableUser(string email);

        Task<bool> DeleteUser(string email);
    }

    
}

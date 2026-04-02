using CynapCRM.Services.AuthAPI.Models.Dto;

namespace CynapCRM.Services.AuthAPI.Service.IService
{
    public interface IAuthService
    {
        Task<string> Register(RegistrationRequestDto registrationRequestDto);

        Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto);

        Task<bool> AssignRole(string email, string roleName);

        Task<bool> ChangePassword(ChangePasswordDto model);
        Task<bool> ForgotPassword(ForgotPasswordDto model);
        Task<bool> ChangeRole(ChangeRoleDto model);
        Task<bool> DeleteUser(string email);

    }

    
}

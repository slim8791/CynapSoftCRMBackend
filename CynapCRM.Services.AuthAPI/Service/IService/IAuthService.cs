using CynapCRM.Services.AuthAPI.Models;
using CynapCRM.Services.AuthAPI.Models.Dto;

namespace CynapCRM.Services.AuthAPI.Service.IService
{
    public interface IAuthService
    {
        Task<ResponseDto> Register(RegistrationRequestDto model);

        Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<bool> AssignRole(string email, string roleName);
        Task<bool> AddRole(string email, string roleName);
        Task<bool> ChangePassword(ChangePasswordDto model);
        Task<ResponseDto> GeneratePasswordResetToken(string email);
        Task<LoginResponseDto> ChangeRole(ChangeRoleDto model);
        Task<bool> EnableUser(string email);
        Task<bool> DisableUser(string email);
        Task<IEnumerable<UserDto>> GetDisabledUsersAsync();
    }

    
}

using CynapCRM.Services.AuthAPI.Models;
using CynapCRM.Services.AuthAPI.Models.Dto;

namespace CynapCRM.Services.AuthAPI.Service.IService
{
    public interface IAuthService
    {
        Task<ResponseDto> Register(RegistrationRequestDto model);
        Task<ResponseDto> UpdateProfileAsync(UpdateProfileDto model);
        Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<bool> AssignRole(string email, UserRole role);
        Task<bool> AddRole(string email, UserRole role);
        Task<bool> ChangePassword(ChangePasswordDto model);
Task<ResponseDto> GeneratePasswordResetToken(string email);
        Task<ResponseDto> ResetPassword(ResetPasswordDto model);
        Task<LoginResponseDto> ChangeRole(ChangeRoleDto model);
        Task<bool> EnableUser(string email);
        Task<bool> DisableUser(string email);
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<IEnumerable<UserDto>> GetDisabledUsersAsync();
        Task<IEnumerable<UserDto>> SearchUsersAsync(string keyword, bool? isActive = null);
    }

    
}

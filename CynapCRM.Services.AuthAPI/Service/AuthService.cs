using CynapCRM.Services.AuthAPI.Data;
using CynapCRM.Services.AuthAPI.Models;
using CynapCRM.Services.AuthAPI.Models.Dto;
using CynapCRM.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace CynapCRM.Services.AuthAPI.Service
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<Utilisateur> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IEmailService _emailService;
        public AuthService(AppDbContext db,
            UserManager<Utilisateur> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            IJwtTokenGenerator jwtTokenGenerator,
            IEmailService emailService)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _emailService = emailService;
        }
        public async Task<string> Register(RegistrationRequestDto registrationRequestDto)
        {
            Utilisateur user = new()
            {
                UserName = registrationRequestDto.Email,
                Email = registrationRequestDto.Email,
                NormalizedEmail = registrationRequestDto.Email.ToUpper(),
                Name = registrationRequestDto.Name,
                PhoneNumber = registrationRequestDto.PhoneNumber,
                Adresse = registrationRequestDto.Adresse
            };
            try
            {
                var result = await _userManager.CreateAsync(user, registrationRequestDto.Password);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(registrationRequestDto.Role))
                    {
                        await _userManager.AddToRoleAsync(user, registrationRequestDto.Role);
                    }
                    return ""; 
                }
                return result.Errors.FirstOrDefault()?.Description ?? "Erreur d'inscription";
            }
            catch (Exception ex)
            {
                return "Erreur : " + ex.Message;
            }
        }
        
        

        public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
        {
            var user = _db.Utilisateurs.FirstOrDefault(u => u.UserName.ToLower() == loginRequestDto.UserName.ToLower());

            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);

            if (user == null || user.IsDeleted == true)
            {
                return new LoginResponseDto() { User = null, Token = "" };
            }

            var roles = await _userManager.GetRolesAsync(user);

            var token = _jwtTokenGenerator.GenerateToken(user, roles);

            UserDto userDto = new()
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
                Adresse = user.Adresse,
                Role = roles.FirstOrDefault() ?? ""
            };

            return new LoginResponseDto { User = userDto, Token = token };
        }
        public async Task<bool> AssignRole(string userId, string role)
        {
            var user = await _userManager.FindByEmailAsync(userId);
            if (user == null)
            {
                return false;
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                return false;
            }

            var result = await _userManager.AddToRoleAsync(user, role);
            return result.Succeeded;
        }
        public async Task<bool> AddRole(string email, string roleName)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole<int> { Name = roleName });
                if (!roleResult.Succeeded)
                {
                    return false;
                }
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded;
        }
        public async Task<bool> ChangeRole(ChangeRoleDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || user.IsDeleted)
            {
                return false;
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!await _roleManager.RoleExistsAsync(model.NewRole))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole<int> { Name = model.NewRole });
                if (!roleResult.Succeeded) return false;
            }

            var result = await _userManager.AddToRoleAsync(user, model.NewRole);
            return result.Succeeded;
        }



        public async Task<bool> ChangePassword(ChangePasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || user.IsDeleted)
            {
                return false;
            }
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            return result.Succeeded;
        }

        
        public async Task<ResponseDto> GeneratePasswordResetToken(string email)
        {
            var normalizedEmail = _userManager.NormalizeEmail(email);
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);

            if (user == null || user.IsDeleted)
            {
                return new ResponseDto
                {
                    IsSuccess = false,
                    Message = "Aucun compte n'est associé à cet e-mail."
                };
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return new ResponseDto
            {
                IsSuccess = true,
                Result = token
            };
        }

        public async Task<ResponseDto> ResetPassword(string email, string token, string newPassword)
        {
            var normalizedEmail = _userManager.NormalizeEmail(email);
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);

            if (user == null || user.IsDeleted)
            {
                return new ResponseDto
                {
                    IsSuccess = false,
                    Message = "Utilisateur introuvable ou supprimé."
                };
            }

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            return new ResponseDto
            {
                IsSuccess = result.Succeeded,
                Message = result.Succeeded
                    ? "Mot de passe réinitialisé avec succès."
                    : "Le jeton est invalide ou le mot de passe ne respecte pas les règles.",
                Errors = result.Errors.Select(e => e.Description)
            };

        }

        public async Task<bool> EnableUser(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return false;
            }
            user.IsDeleted = false;
            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded;
        }

        public async Task<bool> DeleteUser(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return false;
            }
            user.IsDeleted = true;
            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded;
        }

        
    }
}

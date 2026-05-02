using AutoMapper;
using CynapCRM.Services.AuthAPI.Data;
using CynapCRM.Services.AuthAPI.Models;
using CynapCRM.Services.AuthAPI.Models.Dto;
using CynapCRM.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto;
using System.Data;


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
            IEmailService emailService
            )
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _emailService = emailService;
        }
        public async Task<ResponseDto> Register(RegistrationRequestDto model)
        {

            // verify email 
            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                return new ResponseDto
                {
                    IsSuccess = false,
                    Message = "Un compte avec cet email existe déjà."
                };
            }

            Utilisateur user;

            if (model.Role == UserRole.CLIENT)
            {
                user = model.UserType switch
                {
                    UserType.PHARMACIEN => new Pharmacien
                    {
                        Name = model.Name,
                        Email = model.Email,
                        UserName = model.Email,
                        Adresse = model.Adresse,
                        NomOfficine = model.NomOfficine,
                        TypePharmacie = model.TypePharmacie,
                        IsDeleted = false
                    },
                    UserType.GROSSISTE => new Grossiste
                    {
                        Name = model.Name,
                        Email = model.Email,
                        UserName = model.Email,
                        Adresse = model.Adresse,
                        RaisonSociale = model.RaisonSociale,
                        IsDeleted = false
                    },

                    _ => throw new ArgumentException("UserType invalide pour un Client.")
                };
            }

            else
            {
                // ADMIN / SUPERVISEUR / DELEGUE / MEDECIN 
                user = new Utilisateur
                {
                    Name = model.Name,
                    Email = model.Email,
                    UserName = model.Email,
                    Adresse = model.Adresse,
                    IsDeleted = false
                };
            }

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return new ResponseDto
                {
                    IsSuccess = false,
                    Message = result.Errors.FirstOrDefault()?.Description
                };
            }
            var role = model.Role.ToString().ToUpper();
            // Rôle Identity
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole<int> { Name = role });
            }

            await _userManager.AddToRoleAsync(user, role);

            return new ResponseDto
            {
                IsSuccess = true,
                Message = $"Inscription réussie avec le rôle {role} "
            };
        }

        public async Task<LoginResponseDto> Login(LoginRequestDto model)
        {
            // User search via Identity
            var user = await _userManager.FindByNameAsync(model.UserName);

            if (user == null || user.IsDeleted)
            {
                return new LoginResponseDto
                {
                    User = null,
                    Token = ""
                };
            }

            // Password verification
            var isValidPassword =
                await _userManager.CheckPasswordAsync(user, model.Password);

            if (!isValidPassword)
            {
                return new LoginResponseDto
                {
                    User = null,
                    Token = ""
                };
            }

            // Retrieving roles
            var roles = await _userManager.GetRolesAsync(user);

            // a user MUST have a role
            if (!roles.Any())
            {
                return new LoginResponseDto
                {
                    User = null,
                    Token = ""
                };
            }

            // JWT Generation
            var token = _jwtTokenGenerator.GenerateToken(user, roles);

            // User DTO Construction
            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
                Adresse = user.Adresse,

                Role = roles.First().ToUpper()
            };

            return new LoginResponseDto
            {
                User = userDto,
                Token = token
            };
        }

        public async Task<bool> AssignRole(string email, UserRole role)
        {

            var roleName = role.ToString();
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null || user.IsDeleted)
                return false;


            // Create the role if it does not exist
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await _roleManager.CreateAsync(
                    new IdentityRole<int> { Name = roleName });

                if (!roleResult.Succeeded)
                    return false;
            }

            // Avoid duplicates
            if (await _userManager.IsInRoleAsync(user, roleName))
                return true;

            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded;
        }
        public async Task<bool> AddRole(string email, UserRole role)
        {

            var roleName = role.ToString();


            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || user.IsDeleted)
                return false;

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await _roleManager.CreateAsync(
                    new IdentityRole<int> { Name = roleName });

                if (!roleResult.Succeeded)
                    return false;
            }

            if (await _userManager.IsInRoleAsync(user, roleName))
                return true;

            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded;
        }
        public async Task<LoginResponseDto> ChangeRole(ChangeRoleDto model)
        {


            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || user.IsDeleted)
                return null;

            // Delete old roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            var roleName = model.NewRole.ToString();

            // Create the role if necessary
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await _roleManager.CreateAsync(
                    new IdentityRole<int> { Name = roleName });

                if (!roleResult.Succeeded)
                    return null;
            }

            // Add the new role
            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
                return null;

            // Retrieve updated roles
            var updatedRoles = await _userManager.GetRolesAsync(user);

            // GENERATE A NEW TOKEN
            var newToken = _jwtTokenGenerator.GenerateToken(user, updatedRoles);

            return new LoginResponseDto
            {
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    PhoneNumber = user.PhoneNumber,
                    Adresse = user.Adresse,
                    Role = updatedRoles.FirstOrDefault() ?? ""
                },
                Token = newToken
            };

        }
        public async Task<bool> ChangePassword(ChangePasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || user.IsDeleted)
            {
                return false;
            }
            var result = await _userManager.ChangePasswordAsync(
                user, model.CurrentPassword, model.NewPassword);
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

        public async Task<ResponseDto> ResetPassword(ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || user.IsDeleted)
            {
                return new ResponseDto
                {
                    IsSuccess = false,
                    Message = "Lien de réinitialisation invalide ou expiré."
                };
            }

            // ✅ NOUVEAU: Vérifier si le nouveau mot de passe est différent de l'ancien
            var isSameAsOld = await _userManager.CheckPasswordAsync(user, model.NewPassword);
            if (isSameAsOld)
            {
                return new ResponseDto
                {
                    IsSuccess = false,
                    Message = "Le nouveau mot de passe doit être différent de l'ancien mot de passe."
                };
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (!result.Succeeded)
            {
                return new ResponseDto
                {
                    IsSuccess = false,
                    Message = "Lien de réinitialisation invalide ou expiré."
                };
            }

            return new ResponseDto
            {
                IsSuccess = true,
                Message = "Mot de passe réinitialisé avec succès."
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
        public async Task<bool> DisableUser(string email)
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
        public async Task<IEnumerable<UserDto>> GetDisabledUsersAsync()
        {
            var users = await _userManager.Users
                .Where(u => u.IsDeleted)
                .AsNoTracking()
                .ToListAsync();

            return users.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                Name = u.Name,
                PhoneNumber = u.PhoneNumber,
                Adresse = u.Adresse,
                Role = "" 
            });
        }
        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            // Get all users
            var users = await _userManager.Users.ToListAsync();

            var result = new List<UserDto>();

            // MANUAL Mapping, Role Retrieval
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    PhoneNumber = user.PhoneNumber,
                    Adresse = user.Adresse,
                    Role = roles.FirstOrDefault() ?? "" ,
                    IsDeleted = user.IsDeleted

                });
            }
            return result;
        }
    }
}

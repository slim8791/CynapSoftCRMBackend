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
        private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
            {
            "ADMIN",
            "SUPERVISEUR",
            "DELEGUE",
            "MEDECIN",
            "CLIENT"
            };
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
            _emailService = emailService;
        }



        public async Task<ResponseDto> Register(RegistrationRequestDto model)
        {
            // ✅ 1️⃣ Validation stricte du rôle
            if (string.IsNullOrWhiteSpace(model.Role) ||
                !AllowedRoles.Contains(model.Role))
            {
                return new ResponseDto
                {
                    IsSuccess = false,
                    Message = "Rôle invalide. Rôles autorisés : ADMIN, SUPERVISEUR, DELEGUE, MEDECIN, CLIENT."
                };
            }

            // ✅ 2️⃣ Vérifier si l’email existe déjà (UX + sécurité)
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return new ResponseDto
                {
                    IsSuccess = false,
                    Message = "Un compte avec cet email existe déjà."
                };
            }

            // ✅ 3️⃣ Normalisation du rôle (cohérence interne)
            var role = model.Role.ToUpper();

            // ✅ 4️⃣ Création de l’utilisateur
            var user = new Utilisateur
            {
                UserName = model.Email,
                Email = model.Email,
                NormalizedEmail = model.Email.ToUpper(),
                Name = model.Name,
                PhoneNumber = model.PhoneNumber,
                Adresse = model.Adresse,
                IsDeleted = false
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return new ResponseDto
                {
                    IsSuccess = false,
                    Message = result.Errors.FirstOrDefault()?.Description
                              ?? "Erreur lors de l’inscription."
                };
            }

            // ✅ 5️⃣ Création du rôle s’il n’existe pas
            if (!await _roleManager.RoleExistsAsync(role))
            {
                var roleCreationResult =
                    await _roleManager.CreateAsync(new IdentityRole<int> { Name = role });

                if (!roleCreationResult.Succeeded)
                {
                    return new ResponseDto
                    {
                        IsSuccess = false,
                        Message = "Erreur lors de la création du rôle."
                    };
                }
            }

            // ✅ 6️⃣ Affectation du rôle à l’utilisateur
            var addRoleResult = await _userManager.AddToRoleAsync(user, role);

            if (!addRoleResult.Succeeded)
            {
                return new ResponseDto
                {
                    IsSuccess = false,
                    Message = "Erreur lors de l’affectation du rôle."
                };
            }

            // ✅ 7️⃣ Succès
            return new ResponseDto
            {
                IsSuccess = true,
                Message = $"Inscription réussie avec le rôle {role}."
            };
        }
        public async Task<LoginResponseDto> Login(LoginRequestDto model)
        {
            // ✅ 1️⃣ Recherche utilisateur via Identity (plus propre)
            var user = await _userManager.FindByNameAsync(model.UserName);

            if (user == null || user.IsDeleted)
            {
                return new LoginResponseDto
                {
                    User = null,
                    Token = ""
                };
            }

            // ✅ 2️⃣ Vérification du mot de passe
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

            // ✅ 3️⃣ Récupération des rôles
            var roles = await _userManager.GetRolesAsync(user);

            // ✅ Sécurité métier : un utilisateur DOIT avoir un rôle
            if (!roles.Any())
            {
                return new LoginResponseDto
                {
                    User = null,
                    Token = ""
                };
            }

            // ✅ 4️⃣ Génération du JWT (rôles synchronisés)
            var token = _jwtTokenGenerator.GenerateToken(user, roles);

            // ✅ 5️⃣ Construction du DTO utilisateur
            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
                Adresse = user.Adresse,

                // ✅ rôle normalisé
                Role = roles.First().ToUpper()
            };

            return new LoginResponseDto
            {
                User = userDto,
                Token = token
            };
        }

        public async Task<bool> AssignRole(string email, string role)
        {
            // ✅ Validation stricte du rôle
            if (string.IsNullOrWhiteSpace(role) ||
                !AllowedRoles.Contains(role))
                return false;

            var normalizedRole = role.ToUpper();

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || user.IsDeleted)
                return false;

            // ✅ Créer le rôle s’il n’existe pas
            if (!await _roleManager.RoleExistsAsync(normalizedRole))
            {
                var roleResult = await _roleManager.CreateAsync(
                    new IdentityRole<int> { Name = normalizedRole });

                if (!roleResult.Succeeded)
                    return false;
            }

            // ✅ Éviter doublon
            if (await _userManager.IsInRoleAsync(user, normalizedRole))
                return true;

            var result = await _userManager.AddToRoleAsync(user, normalizedRole);
            return result.Succeeded;
        }
        public async Task<bool> AddRole(string email, string roleName)
        {
            // ✅ Validation stricte
            if (string.IsNullOrWhiteSpace(roleName) ||
                !AllowedRoles.Contains(roleName))
                return false;

            var normalizedRole = roleName.ToUpper();

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || user.IsDeleted)
                return false;

            if (!await _roleManager.RoleExistsAsync(normalizedRole))
            {
                var roleResult = await _roleManager.CreateAsync(
                    new IdentityRole<int> { Name = normalizedRole });

                if (!roleResult.Succeeded)
                    return false;
            }

            if (await _userManager.IsInRoleAsync(user, normalizedRole))
                return true;

            var result = await _userManager.AddToRoleAsync(user, normalizedRole);
            return result.Succeeded;
        }
        public async Task<LoginResponseDto> ChangeRole(ChangeRoleDto model)
        {
            if (string.IsNullOrWhiteSpace(model.NewRole) ||!AllowedRoles.Contains(model.NewRole))
            {
                return null;
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || user.IsDeleted)
                return null;

            // 1️⃣ Supprimer anciens rôles
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // 2️⃣ Créer le rôle si nécessaire
            if (!await _roleManager.RoleExistsAsync(model.NewRole))
            {
                var roleResult = await _roleManager.CreateAsync(
                    new IdentityRole<int> { Name = model.NewRole });

                if (!roleResult.Succeeded)
                    return null;
            }

            // 3️⃣ Ajouter le nouveau rôle
            var result = await _userManager.AddToRoleAsync(user, model.NewRole);
            if (!result.Succeeded)
                return null;

            // ✅ 4️⃣ RÉCUPÉRER LES RÔLES À JOUR
            var updatedRoles = await _userManager.GetRolesAsync(user);

            // ✅ 5️⃣ GÉNÉRER UN NOUVEAU TOKEN
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
            // ✅ 1️⃣ Récupérer tous les utilisateurs
            var users = await _userManager.Users
                .AsNoTracking()
                .ToListAsync();

            var result = new List<UserDto>();

            // ✅ 2️⃣ Mapping MANUEL + récupération du rôle
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
                    Role = roles.FirstOrDefault() ?? "" // ✅ rôle principal
                });
            }

            return result;
        }
    }
}

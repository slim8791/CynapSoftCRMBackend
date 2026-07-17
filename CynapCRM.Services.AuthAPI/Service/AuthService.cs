using AutoMapper;
using CynapCRM.Services.AuthAPI.Data;
using CynapCRM.Services.AuthAPI.Models;
using CynapCRM.Services.AuthAPI.Models.Dto;
using CynapCRM.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using CynapCRM.MessageBus.Events;
using MassTransit;

namespace CynapCRM.Services.AuthAPI.Service
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<Utilisateur> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IEmailService _emailService;
        private readonly IPublishEndpoint _publishEndpoint;
        public AuthService(AppDbContext db,
            UserManager<Utilisateur> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            IJwtTokenGenerator jwtTokenGenerator,
            IEmailService emailService,
            IPublishEndpoint publishEndpoint
            )
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _emailService = emailService;
            _publishEndpoint = publishEndpoint;
        }
        public async Task<ResponseDto> Register(RegistrationRequestDto model)
        {
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
                        PhoneNumber = model.PhoneNumber,
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
                        PhoneNumber = model.PhoneNumber,
                        RaisonSociale = model.RaisonSociale,
                        IsDeleted = false
                    },
                    _ => throw new ArgumentException("UserType invalide pour un Client.")
                };
            }
            else if (model.Role == UserRole.MEDECIN)
            {
                user = new Medecin
                {
                    Name = model.Name,
                    Email = model.Email,
                    UserName = model.Email,
                    Adresse = model.Adresse,
                    PhoneNumber = model.PhoneNumber,
                    Specialite = model.Specialite ?? string.Empty,
                    TypeEtablissement = model.TypeEtablissement ?? string.Empty,
                    IsDeleted = false
                };
            }
            else
            {
                user = new Utilisateur
                {
                    Name = model.Name,
                    Email = model.Email,
                    UserName = model.Email,
                    Adresse = model.Adresse,
                    PhoneNumber = model.PhoneNumber,
                    IsDeleted = false
                };
            }

            user.IdRegion = model.IdRegion;

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

            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole<int> { Name = role });

            await _userManager.AddToRoleAsync(user, role);
            await _publishEndpoint.Publish(new UserCreatedEvent
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                Name = user.Name ?? string.Empty,
                Role = role,
                IdRegion = user.IdRegion,
                DateCreation = DateTime.UtcNow
            });
            return new ResponseDto
            {
                IsSuccess = true,
                Message = $"Inscription réussie avec le rôle {role}"
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
                Role = roles.First().ToUpper(),
                IdRegion = user.IdRegion
            };

            return new LoginResponseDto
            {
                User = userDto,
                Token = token
            };
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null || user.IsDeleted)
                return null;

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "";

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber ?? "",
                Adresse = user.Adresse ?? "",
                Role = role,
                IsDeleted = user.IsDeleted,
                TypeClient = GetTypeClient(user),
                IdRegion = user.IdRegion
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

            // ✅ retourne objet vide au lieu de null
            if (user == null || user.IsDeleted)
                return new LoginResponseDto { User = null, Token = "" };

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            var roleName = model.NewRole.ToString();

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await _roleManager.CreateAsync(
                    new IdentityRole<int> { Name = roleName });

                if (!roleResult.Succeeded)
                    return new LoginResponseDto { User = null, Token = "" }; // ✅
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
                return new LoginResponseDto { User = null, Token = "" }; // ✅

            var updatedRoles = await _userManager.GetRolesAsync(user);
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
                    Role = updatedRoles.FirstOrDefault() ?? "",
                    IdRegion = user.IdRegion
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

            // Decode the token robustly
            var decodedToken = model.Token;
            
            // 1. Unescape URL and fix spaces that might have replaced '+' signs
            decodedToken = Uri.UnescapeDataString(decodedToken).Replace(" ", "+");

            // 2. ASP.NET Core Identity tokens typically start with "CfDJ"
            if (!decodedToken.StartsWith("CfDJ"))
            {
                try 
                {
                    // Try standard Base64 decoding
                    var bytes = Convert.FromBase64String(decodedToken);
                    var decodedString = Encoding.UTF8.GetString(bytes);
                    if (decodedString.StartsWith("CfDJ"))
                    {
                        decodedToken = decodedString;
                    }
                }
                catch 
                {
                    try
                    {
                        // Try Base64Url decoding (standard ASP.NET Core approach)
                        var bytes = WebEncoders.Base64UrlDecode(decodedToken);
                        var decodedString = Encoding.UTF8.GetString(bytes);
                        if (decodedString.StartsWith("CfDJ"))
                        {
                            decodedToken = decodedString;
                        }
                    }
                    catch { }
                }
            }

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);
            if (!result.Succeeded)
            {
                // Separate token errors from password policy errors
                var passwordErrors = result.Errors
                    .Where(e => e.Code != "InvalidToken")
                    .ToList();

                if (passwordErrors.Count == 0)
                {
                    // Only the token itself was invalid
                    return new ResponseDto
                    {
                        IsSuccess = false,
                        Message = "Lien de réinitialisation invalide ou expiré."
                    };
                }

                // Password does not meet the policy — return translated messages
                var message = string.Join(" ", passwordErrors.Select(e => e.Code switch
                {
                    "PasswordTooShort"                => "Le mot de passe est trop court (minimum 6 caractères).",
                    "PasswordRequiresDigit"           => "Ajoutez au moins un chiffre (0–9).",
                    "PasswordRequiresUpper"           => "Ajoutez au moins une lettre majuscule (A–Z).",
                    "PasswordRequiresLower"           => "Ajoutez au moins une lettre minuscule (a–z).",
                    "PasswordRequiresNonAlphanumeric" => "Ajoutez au moins un caractère spécial (ex. @ # ! %).",
                    "PasswordRequiresUniqueChars"     => "Utilisez des caractères plus variés.",
                    _                                 => e.Description
                }));

                return new ResponseDto
                {
                    IsSuccess = false,
                    Message = message
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

            var result = new List<UserDto>();

            // ✅ rôle maintenant chargé pour chaque utilisateur
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
                    Role = roles.FirstOrDefault() ?? "",
                    IsDeleted = user.IsDeleted,
                    TypeClient = GetTypeClient(user),
                    IdRegion = user.IdRegion
                });
            }

            return result;
        }
        public async Task<IEnumerable<UserDto>> SearchUsersAsync(
    string keyword,
    bool? isActive = null)
        {
            var lower = keyword?.Trim().ToLower() ?? string.Empty;

            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(lower))
            {
                query = query.Where(u =>
                    u.Name.ToLower().Contains(lower) ||
                    u.Email.ToLower().Contains(lower) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(lower)));
            }

            if (isActive.HasValue)
                query = query.Where(u => u.IsDeleted == !isActive.Value);

            var users = await query.AsNoTracking().ToListAsync();
            var result = new List<UserDto>();

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
                    Role = roles.FirstOrDefault() ?? "",
                    IsDeleted = user.IsDeleted,
                    TypeClient = GetTypeClient(user),
                    IdRegion = user.IdRegion
                });
            }

            return result;
        }

        public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role)
        {
            var users = await _userManager.GetUsersInRoleAsync(role);
            var result = new List<UserDto>();

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
                    Role = roles.FirstOrDefault() ?? "",
                    IsDeleted = user.IsDeleted,
                    TypeClient = GetTypeClient(user),
                    IdRegion = user.IdRegion
                });
            }

            return result;
        }

        public async Task<IEnumerable<UserDto>> GetUsersByRegionAsync(int idRegion)
        {
            var users = await _userManager.Users
                .Where(u => u.IdRegion == idRegion && !u.IsDeleted)
                .ToListAsync();

            var result = new List<UserDto>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                result.Add(new UserDto
                {
                    Id          = u.Id,
                    Name        = u.Name,
                    Email       = u.Email ?? "",
                    PhoneNumber = u.PhoneNumber ?? "",
                    Adresse     = u.Adresse,
                    Role        = roles.FirstOrDefault() ?? "",
                    IdRegion    = u.IdRegion,
                    IsDeleted   = u.IsDeleted,
                    TypeClient  = GetTypeClient(u)
                });
            }
            return result;
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
                    Role = roles.FirstOrDefault() ?? "",
                    IsDeleted = user.IsDeleted,
                    TypeClient = GetTypeClient(user),
                    IdRegion = user.IdRegion
                });
            }
            return result;
        }
        private static string? GetTypeClient(Utilisateur user) => user switch
        {
            Pharmacien => "PHARMACIEN",
            Grossiste  => "GROSSISTE",
            _          => null
        };

        public async Task<ResponseDto> UpdateProfileAsync(UpdateProfileDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || user.IsDeleted)
            {
                return new ResponseDto
                {
                    IsSuccess = false,
                    Message = "Utilisateur introuvable."
                };
            }

            // Mise à jour des champs modifiables
            user.Name = model.Name ?? user.Name;
            user.PhoneNumber = model.PhoneNumber ?? user.PhoneNumber;
            user.Adresse = model.Adresse ?? user.Adresse;
            if (model.IdRegion.HasValue)
                user.IdRegion = model.IdRegion;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return new ResponseDto
                {
                    IsSuccess = false,
                    Message = result.Errors.FirstOrDefault()?.Description
                };
            }

            // Retourner les données mises à jour
            var roles = await _userManager.GetRolesAsync(user);
            return new ResponseDto
            {
                IsSuccess = true,
                Message = "Profil mis à jour avec succès.",
                Result = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    PhoneNumber = user.PhoneNumber,
                    Adresse = user.Adresse,
                    Role = roles.FirstOrDefault() ?? "",
                    IdRegion = user.IdRegion
                }
            };
        }
        
    }
}

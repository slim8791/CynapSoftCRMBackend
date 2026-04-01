using CynapCRM.Services.AuthAPI.Data;
using CynapCRM.Services.AuthAPI.Models;
using CynapCRM.Services.AuthAPI.Models.Dto;
using CynapCRM.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Identity;


namespace CynapCRM.Services.AuthAPI.Service
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<Utilisateur> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        public AuthService(AppDbContext db,
            UserManager<Utilisateur> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;
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
                // Création de l'utilisateur avec hashage auto du mot de passe
                var result = await _userManager.CreateAsync(user, registrationRequestDto.Password);

                if (result.Succeeded)
                {
                    // Attribution du rôle choisi dans la liste déroulante
                    if (!string.IsNullOrEmpty(registrationRequestDto.Role))
                    {
                        await _userManager.AddToRoleAsync(user, registrationRequestDto.Role);
                    }
                    return ""; // Succès
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

            // Vérification du mot de passe
            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);

            if (user == null || !isValid)
            {
                return new LoginResponseDto() { User = null, Token = "" };
            }

            // Récupération des rôles pour le Token
            var roles = await _userManager.GetRolesAsync(user);

            // Génération du Jeton JWT
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

        public async Task<bool> AssignRole(string email, string roleName)
        {
            var user = _db.Utilisateurs.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
            if (user != null)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole<int>(roleName));
                }
                await _userManager.AddToRoleAsync(user, roleName);
                return true;
            }
            return false;
        }
    }
}

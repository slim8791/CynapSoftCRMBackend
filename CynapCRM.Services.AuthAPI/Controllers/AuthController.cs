using CynapCRM.Services.AuthAPI.Models;
using CynapCRM.Services.AuthAPI.Models.Dto;
using CynapCRM.Services.AuthAPI.Service;
using CynapCRM.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace CynapCRM.Services.AuthAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        protected ResponseDto _response;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _env;
        private readonly TurnstileService _turnstileService;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, IEmailService emailService, IWebHostEnvironment env, TurnstileService turnstileService, IConfiguration configuration)
        {
            _authService = authService;
            _response = new();
            _emailService = emailService;
            _env = env;
            _turnstileService = turnstileService;
            _configuration = configuration;
        }
        [HttpPost("register")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")] 
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDto model)
        {
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            // The admin can create any account

            // SUPERVISEUR can only create DELEGUE and CLIENT
            if (currentUserRole == UserRole.SUPERVISEUR.ToString())
            {
                if (model.Role != UserRole.DELEGUE &&
                    model.Role != UserRole.CLIENT)
                {
                    _response.IsSuccess = false;
                    _response.Message = $"Vous n'êtes pas autorisé à créer un compte avec le rôle {model.Role}.";
                    return Forbid();
                }
            }

            // DELEGUE can create CLIENT and MEDECIN
            if (currentUserRole == UserRole.DELEGUE.ToString())
            {
                if (model.Role != UserRole.CLIENT && model.Role != UserRole.MEDECIN)
                {
                    _response.IsSuccess = false;
                    _response.Message = $"Vous n'êtes pas autorisé à créer un compte avec le rôle {model.Role}.";
                    return Forbid();
                }
            }
            var result = await _authService.Register(model);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            _response.IsSuccess = true;
            _response.Message = "Utilisateur créé avec succès.";
            return Ok(_response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            // Verify Turnstile CAPTCHA token
            // Vérifie Turnstile seulement si le token est fourni
            var clientType = Request.Headers["X-Client-Type"];

            if (clientType != "mobile")
            {
                if (string.IsNullOrEmpty(model.TurnstileToken))
                {
                    return BadRequest("Captcha requis");
                }

                var isHuman = await _turnstileService.VerifyAsync(model.TurnstileToken);
                if (!isHuman)
                {
                    return BadRequest("Vérification échouée");
                }
            }

            var loginResponse = await _authService.Login(model);
            if (loginResponse.User == null )
            {
                _response.IsSuccess = false;
                _response.Message = "Identifiants incorrects ";
                return Unauthorized(_response);
            }
            if (loginResponse.User.IsDeleted)
            {
                _response.IsSuccess = false;
                _response.Message = "Compte désactivé.";
                return Forbid();
            }
            _response.Result = loginResponse;
            return Ok(_response);
        }
        [HttpGet("users/search")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> SearchUsers(
            [FromQuery] string keyword,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword) || keyword.Trim().Length < 3)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Le mot-clé doit contenir au moins 3 caractères.";
                    return BadRequest(_response);
                }

                var users = await _authService.SearchUsersAsync(keyword.Trim(), isActive);
                _response.IsSuccess = true;
                _response.Result = users.ToList();
                _response.Message = $"{users.Count()} utilisateur(s) trouvé(s).";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Erreur de recherche : {ex.Message}";
                return StatusCode(515, _response);
            }
        }
        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données invalides.";
                    return BadRequest(_response);
                }

                var result = await _authService.UpdateProfileAsync(model);

                if (!result.IsSuccess)
                {
                    _response.IsSuccess = false;
                    _response.Message = result.Message;
                    return BadRequest(_response);
                }

                _response.IsSuccess = true;
                _response.Message = result.Message;
                _response.Result = result.Result;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }
        [HttpGet("users")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _authService.GetAllUsersAsync();
                if (users == null || !users.Any())
                {
                    _response.IsSuccess = true;
                    _response.Result = new List<UserDto>();
                    _response.Message = "Aucun utilisateur trouvé.";
                    return Ok(_response);
                }

                // SUPERVISEUR sees only DELEGUE + CLIENT users
                var callerRole = User.FindFirstValue(ClaimTypes.Role);
                var userList = users.ToList();
                if (callerRole == UserRole.SUPERVISEUR.ToString())
                {
                    userList = userList.Where(u =>
                        u.Role == UserRole.DELEGUE.ToString() ||
                        u.Role == UserRole.CLIENT.ToString()
                    ).ToList();
                }

                _response.IsSuccess = true;
                _response.Result = userList;
                _response.Message = "Liste de tous les utilisateurs.";

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Server error: {ex.Message}";
                return StatusCode(515, _response);
            }
        }
        [HttpPost("AssignRole")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var assignRoleSuccessful = await _authService.AssignRole(model.UserId, model.Role);
            if (!assignRoleSuccessful)
            {
                _response.IsSuccess = false;
                _response.Message = "Erreur lors de l'attribution du rôle";
                return BadRequest(_response);
            }
            _response.IsSuccess = true;
            _response.Message = "Rôle attribué avec succès";
            return Ok(_response);
        }
        [HttpPut("add-role")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> AddRole([FromBody] RegistrationRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.AddRole(model.Email, model.Role);
            if (!result)
            {
                _response.IsSuccess = false;
                _response.Message = "Échec de l’ajout du rôle.";
                return NotFound(_response); 
            }

            _response.IsSuccess = true;
            _response.Message = $"Rôle {model.Role} ajouté avec succès à l’utilisateur {model.Email}.";
            return Ok(_response); 
        }


        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Retrieve the email from the JWT token
                var emailFromToken = User.FindFirstValue(ClaimTypes.Email);

                if (string.IsNullOrWhiteSpace(emailFromToken))
                {
                    _response.IsSuccess = false;
                    _response.Message = "Utilisateur non authentifié.";
                    return Unauthorized(_response);
                }

                // Verify that the user is changing THEIR own password
                if (!string.Equals(emailFromToken, model.Email, StringComparison.OrdinalIgnoreCase))
                {
                    _response.IsSuccess = false;
                    _response.Message = "Vous ne pouvez changer que votre propre mot de passe.";
                    return Forbid();
                }

                // Call to the service (business logic)
                var result = await _authService.ChangePassword(model);

                // Incorrect current password
                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Mot de passe actuel incorrect.";
                    return BadRequest(_response);
                }

                _response.IsSuccess = true;
                _response.Message = "Mot de passe changé avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Erreur lors du changement de mot de passe : {ex.Message}";
                return StatusCode(515, _response);
            }

        }
[HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.GeneratePasswordResetToken(model.Email);

            if (!response.IsSuccess)
            {
                return NotFound(response);
            }
            var token = response.Result.ToString();

            var encodedToken = Uri.EscapeDataString(token);

            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:4200";
            string resetLink = $"{frontendUrl}/reset-password?email={Uri.EscapeDataString(model.Email)}&token={encodedToken}";

            string subject = "Réinitialisation de mot de passe - CynapCRM";
            string message = $@"
        <div style='font-family: Arial, sans-serif; border: 1px solid #ddd; padding: 20px;'>
            <h2 style='color: #2c3e50;'>CynapCRM</h2>
            <p>Bonjour,</p>
            <p>Vous avez demandé la réinitialisation de votre mot de passe.</p>
            <p>Veuillez cliquer sur le bouton ci-dessous pour continuer :</p>
            <a href='{resetLink}' 
               style='display: inline-block; padding: 10px 20px; background-color: #3498db; color: white; text-decoration: none; border-radius: 5px;'>
               Réinitialiser mon mot de passe
            </a>
            <p style='margin-top: 20px; font-size: 0.8em; color: #7f8c8d;'>
                Si le bouton ne fonctionne pas, copiez ce lien : <br/> {resetLink}
            </p>
        </div>";

            await _emailService.SendEmailAsync(model.Email, subject, message);

            response.Message = "Un e-mail de réinitialisation a été envoyé.";
            response.Result = null;
            return Ok(response);
        }

[HttpPut("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.ResetPassword(model);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("change-role")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> ChangeRole([FromBody] ChangeRoleDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.ChangeRole(model);
            if (result == null)
            {
                _response.IsSuccess = false;
                _response.Message = "Échec du changement de rôle.";
                return NotFound(_response);
            }
            _response.IsSuccess = true;
            _response.Message = "Rôle changé avec succès.";
            return Ok(_response);
        }
        [HttpPut("enable-user/{email}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> EnableUser(string email)
        {
            var result = await _authService.EnableUser(email);
            if (!result)
            {
                _response.IsSuccess = false;
                _response.Message = "Échec de la réactivation.";
                return NotFound(_response);
            }

            _response.IsSuccess = true;
            _response.Message = "Utilisateur réactivé.";
            return Ok(_response);
        }
        [HttpPut("delete-user/{email}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteUser(string email)
        {
            var result = await _authService.DisableUser(email);
            if (!result)
            {
                _response.IsSuccess = false;
                _response.Message = "Échec de la suppression.";
                return NotFound(_response);
            }

            _response.IsSuccess = true;
            _response.Message = "Utilisateur supprimé.";
            return Ok(_response);
        }
        [HttpGet("users/by-role/{role}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,MEDECIN")]
        public async Task<IActionResult> GetUsersByRole(string role)
        {
            // DELEGUE can query CLIENT and MEDECIN roles
            var callerRole = User.FindFirstValue(ClaimTypes.Role);
            if (callerRole == UserRole.DELEGUE.ToString() &&
                !string.Equals(role, UserRole.CLIENT.ToString(), StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(role, UserRole.MEDECIN.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                _response.IsSuccess = false;
                _response.Message = "Accès refusé. Vous ne pouvez consulter que les clients et les médecins.";
                return Forbid();
            }

            // MEDECIN can only query DELEGUE roles
            if (callerRole == UserRole.MEDECIN.ToString() &&
                !string.Equals(role, UserRole.DELEGUE.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                _response.IsSuccess = false;
                _response.Message = "Accès refusé. Un médecin ne peut voir que les délégués.";
                return Forbid();
            }

            // SUPERVISEUR can only query DELEGUE or CLIENT roles
            if (callerRole == UserRole.SUPERVISEUR.ToString() &&
                !string.Equals(role, UserRole.DELEGUE.ToString(), StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(role, UserRole.CLIENT.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                _response.IsSuccess = false;
                _response.Message = "Accès refusé.";
                return Forbid();
            }

            var result = await _authService.GetUsersByRoleAsync(role);
            return Ok(new ResponseDto { IsSuccess = true, Result = result });
        }

        [HttpGet("users/{id}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,MEDECIN")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _authService.GetUserByIdAsync(id);
                if (user == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Utilisateur non trouvé.";
                    return NotFound(_response);
                }

                var callerRole = User.FindFirstValue(ClaimTypes.Role);
                if (callerRole == UserRole.DELEGUE.ToString() &&
                    user.Role != UserRole.CLIENT.ToString() &&
                    user.Role != UserRole.MEDECIN.ToString())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Acces refuse.";
                    return Forbid();
                }

                // MEDECIN can only look up DELEGUE users (to see who visited them)
                if (callerRole == UserRole.MEDECIN.ToString() &&
                    user.Role != UserRole.DELEGUE.ToString())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Acces refuse.";
                    return Forbid();
                }

                _response.IsSuccess = true;
                _response.Result = user;
                _response.Message = "Détails de l'utilisateur récupérés.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Erreur: {ex.Message}";
                return StatusCode(515, _response);
            }
        }

        [HttpGet("users/by-region/{idRegion}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetUsersByRegion(int idRegion)
        {
            try
            {
                var filtered = await _authService.GetUsersByRegionAsync(idRegion);
                var list = filtered.ToList();
                _response.IsSuccess = true;
                _response.Result    = list;
                _response.Message   = $"{list.Count} utilisateur(s) trouvé(s) dans la région {idRegion}.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message   = ex.Message;
                return StatusCode(500, _response);
            }
        }

        [HttpGet("disabled-users")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetDisabledUsers()
        {
            try
            {
                var users = await _authService.GetDisabledUsersAsync();
                if (users == null || !users.Any())
                {
                    _response.IsSuccess = true;
                    _response.Result = new List<UserDto>();
                    _response.Message = "Aucun utilisateur désactivé trouvé.";
                    return Ok(_response);
                }

                _response.IsSuccess = true;
                _response.Result = users;
                _response.Message = "Liste des utilisateurs désactivés.";

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "Erreur lors de la récupération des utilisateurs désactivés.";
                return StatusCode(515, _response);
            }
        }
    }
}

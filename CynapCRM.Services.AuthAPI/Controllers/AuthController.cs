using CynapCRM.Services.AuthAPI.Models;
using CynapCRM.Services.AuthAPI.Models.Dto;
using CynapCRM.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        public AuthController(IAuthService authService, IEmailService emailService, IWebHostEnvironment env)
        {
            _authService = authService;
            _response = new();
            _emailService = emailService;
            _env = env;
        }
        [HttpPost("register")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")] 
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDto model)
        {
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            // The admin can create any account

            // The supervisor can only create DELEGUE, MEDECIN, CLIENT

            if (currentUserRole == UserRole.SUPERVISEUR.ToString())
            {

                if (model.Role != UserRole.DELEGUE &&
                            model.Role != UserRole.MEDECIN &&
                            model.Role != UserRole.CLIENT)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Vous n’avez pas le droit d’exécuter cette opération.";
                    return Forbid();
                }
            }
            // The delegate can create CLIENT and MEDECIN

            if (currentUserRole == UserRole.DELEGUE.ToString())
            {

                if (model.Role != UserRole.CLIENT && model.Role != UserRole.MEDECIN)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Vous n’avez pas le droit d’exécuter cette opération.";
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
        [Authorize(Roles = "ADMIN")]
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

                _response.IsSuccess = true;
                _response.Result = users.ToList();
                _response.Message = "Liste de tous les utilisateurs.";

                return Ok(_response);
            }
            catch (Exception ex)
            {
                // FULL DEBUG
                var errorDetails = $@"
GetAllUsers EXCEPTION:
Type: {ex.GetType().Name}
Message: {ex.Message}
Stack: {ex.StackTrace}
Inner: {ex.InnerException?.Message}";
                
                Console.WriteLine(errorDetails);
                
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

            var encodedToken = System.Web.HttpUtility.UrlEncode(token);

            // ✅ MODIF: lien vers FRONTEND Angular
            string resetLink = $"http://localhost:4200/reset-password?email={model.Email}&token={encodedToken}";

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
        [HttpGet("users/{id}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
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

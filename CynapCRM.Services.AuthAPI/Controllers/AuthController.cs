using CynapCRM.Services.AuthAPI.Models;
using CynapCRM.Services.AuthAPI.Models.Dto;
using CynapCRM.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            // l'admin peut créer n’importe quel compte

            // le superviseur peut créer uniquement DELEGUE, MEDECIN, CLIENT

            if (currentUserRole == "SUPERVISEUR")
            {
                if (!(model.Role.ToUpper() == "DELEGUE" || model.Role.ToUpper() == "MEDECIN" || model.Role.ToUpper() == "CLIENT"))
                {
                    _response.IsSuccess = false;
                    _response.Message = "Vous n’avez pas le droit d’exécuter cette opération.";
                    return Forbid();
                }
            }
            // le delegue peut créer  CLIENT et MEDECIN

            if (currentUserRole == "DELEGUE")
            {
                if (!(model.Role.ToUpper() == "CLIENT" || model.Role.ToUpper()== "MEDECIN"))
                {
                    _response.IsSuccess = false;
                    _response.Message = "Vous n’avez pas le droit d’exécuter cette opération.";
                    return Forbid();
                }
            }
            var errorMessage = await _authService.Register(model);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                _response.IsSuccess = false;
                _response.Message = errorMessage;
                return BadRequest(_response);
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

        [HttpPost("AssignRole")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var assignRoleSuccessful = await _authService.AssignRole(model.UserId, model.Role.ToUpper());
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
        [Authorize(Roles = "ADMIN")]
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _authService.ChangePassword(model);
            if (!result)
            {
                _response.IsSuccess = false;
                _response.Message = "Échec du changement de mot de passe.";
                return BadRequest(_response);
            }

            _response.IsSuccess = true;
            _response.Message = "Mot de passe changé avec succès.";
            return Ok(_response);

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

            // ce lien vers l'interface Frontend
            
            string resetLink = $"https://localhost:7000/api/auth/reset-password?email={model.Email}&token={encodedToken}";

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

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.ResetPassword(model.Email, model.Token, model.NewPassword);

            if (!response.IsSuccess) return BadRequest(response);

            return Ok(response);
        }


        [HttpPut("change-role")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> ChangeRole([FromBody] ChangeRoleDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.ChangeRole(model);
            if (!result)
            {
                _response.IsSuccess = false;
                _response.Message = "Échec du changement de rôle.";
                return BadRequest(_response);
            }

            _response.IsSuccess = true;
            _response.Message = "Rôle changé avec succès.";
            return Ok(_response);
        }
        [HttpPut("enable-user/{email}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> EnableUser([FromBody] string email)
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
        public async Task<IActionResult> DeleteUser([FromBody] string email)
        {
            var result = await _authService.DeleteUser(email);
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

    }
}
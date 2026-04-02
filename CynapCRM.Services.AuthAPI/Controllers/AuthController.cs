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

        public AuthController(IAuthService authService)
        {
            _authService = authService;
            _response = new(); 
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
                    _response.Message = "Un superviseur ne peut créer que des comptes délégué, médecin ou client.";
                    return BadRequest(_response);
                }
            }
            // le delegue peut créer uniquement CLIENT

            if (currentUserRole == "DELEGUE")
            {
                if (!(model.Role.ToUpper() == "CLIENT" || model.Role.ToUpper()== "MEDECIN"))
                {
                    _response.IsSuccess = false;
                    _response.Message = "Un délégué ne peut créer que des comptes client.";
                    return BadRequest(_response);
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
            if (loginResponse.User == null)
            {
                _response.IsSuccess = false;
                _response.Message = "Email ou mot de passe incorrect";
                return BadRequest(_response);
            }
            _response.Result = loginResponse;
            return Ok(_response);
        }

        [HttpPost("AssignRole")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> AssignRole([FromBody] RegistrationRequestDto model)
        {
            var assignRoleSuccessful = await _authService.AssignRole(model.Email, model.Role.ToUpper());
            if (!assignRoleSuccessful)
            {
                _response.IsSuccess = false;
                _response.Message = "Erreur lors de l'attribution du rôle";
                return BadRequest(_response);
            }
            return Ok(_response);
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

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
        [HttpPut("forgot-password")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.ForgotPassword(model);
            if (!result)
            {
                _response.IsSuccess = false;
                _response.Message = "Échec de la réinitialisation du mot de passe.";
                return BadRequest(_response);
            }

            _response.IsSuccess = true;
            _response.Message = "Mot de passe réinitialisé avec succès.";
            return Ok(_response);
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
        [HttpPut("delete-user")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> DeleteUser([FromBody] DeleteUserDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.DeleteUser(model);
            if (!result)
            {
                _response.IsSuccess = false;
                _response.Message = "Échec de la suppression .";
                return BadRequest(_response);
            }

            _response.IsSuccess = true;
            _response.Message = "Utilisateur supprimé .";
            return Ok(_response);
        }

    }
}
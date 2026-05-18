using Azure;
using CynapCRM.Services.OrderAPI.Models;
using CynapCRM.Services.OrderAPI.Models.Dto;
using CynapCRM.Services.OrderAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CynapCRM.Services.OrderAPI.Controllers
{
    // ═══════════════════════════════════════
    // ReclamationController.cs
    // ═══════════════════════════════════════

    [ApiController]
    [Route("api/reclamations")]
    [Authorize]
    public class ReclamationController : ControllerBase
    {
        private readonly IReclamationService _reclamationService;
        protected ResponseDto _response;

        public ReclamationController(IReclamationService reclamationService)
        {
            _reclamationService = reclamationService;
            _response = new();
        }

        // FIX: retourner ResponseDto cohérent
        [HttpGet]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetAllReclamations()
        {
            try
            {
                var result = await _reclamationService.GetAllReclamationsAsync();
                _response.Result = result;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        // FIX: ajout rôles CLIENT pour voir leurs propres réclamations
        [HttpGet("by-commande/{orderId:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]
        public async Task<IActionResult> GetReclamationsByOrder(int orderId)
        {
            try
            {
                if (orderId <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de commande invalide.";
                    return BadRequest(_response);
                }
                var result = await _reclamationService.GetReclamationsByOrderAsync(orderId);
                _response.Result = result;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        // FIX: ajout rôles CLIENT
        [HttpGet("by-client/{idClient:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]
        public async Task<IActionResult> GetReclamationsByClient(int idClient)
        {
            try
            {
                if (idClient <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de client invalide.";
                    return BadRequest(_response);
                }
                var result = await _reclamationService.GetReclamationsByClientAsync(idClient);
                _response.Result = result;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        // FIX: ajout rôles CLIENT
        [HttpGet("{idReclamation:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]
        public async Task<IActionResult> GetReclamationById(int idReclamation)
        {
            try
            {
                if (idReclamation <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de réclamation invalide.";
                    return BadRequest(_response);
                }
                var result = await _reclamationService.GetReclamationByIdAsync(idReclamation);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Réclamation introuvable.";
                    return NotFound(_response);
                }
                _response.Result = result;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]
        public async Task<IActionResult> CreateUpdateReclamation(
            [FromBody] ReclamationDto reclamationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données invalides.";
                    return BadRequest(_response);
                }

                var clientIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (clientIdClaim == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Identité du client introuvable.";
                    return Unauthorized(_response);
                }

                reclamationDto.Id_Client = int.Parse(clientIdClaim.Value);

                var result = await _reclamationService
                    .CreateUpdateReclamationAsync(reclamationDto);

                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = reclamationDto.Id_Rec == 0
                        ? "Échec de la création. Vérifiez la commande et la ligne."
                        : "Réclamation introuvable ou non modifiable.";
                    return BadRequest(_response);
                }

                _response.Result = result;
                _response.Message = reclamationDto.Id_Rec == 0
                    ? "Réclamation créée avec succès."
                    : "Réclamation modifiée avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpPut("{reclamationId:int}/status")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> UpdateReclamationStatus(
            int reclamationId,
            [FromBody] StatutReclamation newStatus)
        {
            try
            {
                if (reclamationId <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de réclamation invalide.";
                    return BadRequest(_response);
                }
                bool isUpdated = await _reclamationService
                    .UpdateReclamationStatusAsync(reclamationId, newStatus);
                if (!isUpdated)
                {
                    _response.IsSuccess = false;
                    // FIX: message précis — machine d'état
                    _response.Message = "Transition de statut invalide ou réclamation introuvable.";
                    return BadRequest(_response); // FIX: BadRequest plus approprié que NotFound
                }
                _response.Message = "Statut mis à jour avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        // FIX: CLIENT peut supprimer sa propre réclamation ouverte
        [HttpDelete("{reclamationId:int}")]
        [Authorize(Roles = "ADMIN,PHARMACIEN,GROSSISTE,CLIENT")]
        public async Task<IActionResult> DeleteReclamation(int reclamationId)
        {
            try
            {
                if (reclamationId <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de réclamation invalide.";
                    return BadRequest(_response);
                }
                bool isDeleted = await _reclamationService
                    .DeleteReclamationAsync(reclamationId);
                if (!isDeleted)
                {
                    _response.IsSuccess = false;
                    // FIX: message précis — règle métier
                    _response.Message = "Suppression impossible (réclamation non ouverte ou introuvable).";
                    return BadRequest(_response);
                }
                _response.Message = "Réclamation supprimée avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }
    }
}

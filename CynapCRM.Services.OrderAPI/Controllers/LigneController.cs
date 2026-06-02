using Azure;
using CynapCRM.Services.OrderAPI.Models;
using CynapCRM.Services.OrderAPI.Models.Dto;
using CynapCRM.Services.OrderAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.OrderAPI.Controllers
{

    // ═══════════════════════════════════════
    // LigneController.cs
    // ═══════════════════════════════════════

    [ApiController]
    [Route("api/lignes")]
    [Authorize]
    public class LigneController : ControllerBase
    {
        private readonly ILigneService _ligneService;
        protected ResponseDto _response;

        public LigneController(ILigneService ligneService)
        {
            _ligneService = ligneService;
            _response = new();
        }

        // FIX: CLIENT + DELEGUE + ADMIN peuvent créer/modifier des lignes
        [HttpPost]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]
        public async Task<IActionResult> CreateOrUpdateLigneCommande(
            [FromBody] CreateOrUpdateLigneCommandeDto ligneDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de ligne de commande invalides.";
                    return BadRequest(_response);
                }

                var result = await _ligneService.CreateOrUpdateLigneCommandeAsync(ligneDto);

                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = ligneDto.Id_Ligne == 0
                        ? "Échec de la création. Vérifiez la commande associée."
                        : "Ligne introuvable ou commande non modifiable.";
                    return BadRequest(_response);
                }

                _response.Result = result;
                _response.Message = ligneDto.Id_Ligne == 0
                    ? "Ligne créée avec succès."
                    : "Ligne modifiée avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response); // FIX: 515 → 515
            }
        }

        // FIX: CLIENT peut aussi supprimer ses propres lignes
        [HttpDelete("{ligneId:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]
        public async Task<IActionResult> DeleteLigneCommande(int ligneId)
        {
            try
            {
                if (ligneId <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID ligne invalide.";
                    return BadRequest(_response);
                }
                bool isDeleted = await _ligneService.RemoveLigneCommandeAsync(ligneId);
                if (!isDeleted)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Ligne introuvable ou commande non modifiable.";
                    return NotFound(_response);
                }
                _response.Message = "Ligne supprimée avec succès.";
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

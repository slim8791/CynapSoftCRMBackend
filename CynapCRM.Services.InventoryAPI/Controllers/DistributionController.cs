using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.InventoryAPI.Controllers
{

    [Route("api/distributions")]
    [ApiController]
    [Authorize]

    public class DistributionController : ControllerBase
    {
        private readonly IDistributionService _distributionService;
        protected ResponseDto _response;

        public DistributionController(IDistributionService distributionService)
        {
            _distributionService = distributionService;
            _response = new ResponseDto();
        }
        // 1. Enregistrer ou modifier une distribution (Don d'échantillon)
        [HttpPost("distribution")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> CreateOrUpdateDistribution([FromBody] EchantillonDto echantillonDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données invalides. Veuillez vérifier les informations fournies.";
                    return BadRequest(ModelState);
                }

                var result = await _distributionService.CreateOrUpdateEchantillonAsync(echantillonDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Erreur lors de l'enregistrement de la distribution.";
                    return BadRequest(_response);
                }

                _response.Result = result;
                _response.Message = "Distribution enregistrée avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        // 2. Récupérer une distribution spécifique par son ID

        [HttpGet("{idDistribution:int}")]
        public async Task<IActionResult> GetDistributionById(int idDistribution)
        {
            try
            {
                if (idDistribution <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id distribution invalide.";
                    return BadRequest(_response);
                }
                var result = await _distributionService.GetEchantillonByIdAsync(idDistribution);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Distribution non trouvée.";
                    return NotFound(_response);
                }
                _response.Result = result;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        // 3. Historique des distributions pour un Médecin

        [HttpGet("by-medecin/{idMedecin:int}")]

        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]

        public async Task<IActionResult> GetDistributionsByMedecin(int idMedecin)
        {
            try
            {
                if (idMedecin <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id médecin invalide.";
                    return BadRequest(_response);
                }
                var result = await _distributionService.GetDistributionsByMedecinAsync(idMedecin);
                if (result == null || !result.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucune distribution trouvée pour ce médecin.";
                    return NotFound(_response);
                }
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        // 4. Historique des distributions pour un Pharmacien

        [HttpGet("by-pharmacien/{idPharmacien:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetDistributionsByPharmacien(int idPharmacien)
        {
            try
            {
                if (idPharmacien <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id pharmacien invalide.";
                    return BadRequest(_response);
                }
                var result = await _distributionService.GetDistributionsByPharmacienAsync(idPharmacien);
                if (result == null || !result.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucune distribution trouvée pour ce pharmacien.";
                    return NotFound(_response);
                }
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        // 5. Supprimer une distribution (Annulation)

        [HttpDelete("{idDistribution:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> DeleteDistribution(int idDistribution)
        {
            try
            {
                if (idDistribution <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id distribution invalide.";
                    return BadRequest(_response);
                }
                bool isDeleted = await _distributionService.DeleteEchantillonAsync(idDistribution);
                if (!isDeleted)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Impossible de supprimer : distribution inexistante.";
                    return NotFound(_response);
                }

                _response.Message = "Distribution supprimée de l'historique.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }
    }
}
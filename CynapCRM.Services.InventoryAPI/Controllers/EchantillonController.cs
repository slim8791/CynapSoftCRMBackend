using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.InventoryAPI.Controllers
{
    [Route("api/echantillon")]
    [ApiController]
    public class EchantillonController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        protected ResponseDto _response;

        public EchantillonController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
            _response = new ResponseDto();
        }
        // 1. Enregistrer ou modifier une distribution (Don d'échantillon)
        [HttpPost("distribution")]

        public async Task<IActionResult> CreateUpdateDistribution([FromBody] EchantillonDto echantillonDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données invalides. Veuillez vérifier les informations fournies.";
                    return BadRequest(ModelState);
                }

                var result = await _inventoryService.CreateUpdateEchantillonAsync(echantillonDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Erreur lors de l'enregistrement de la distribution.";
                    return StatusCode(500, _response);
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
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id distribution invalide.";
                    return BadRequest(_response);
                }
                var result = await _inventoryService.GetEchantillonByIdAsync(id);
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
        [HttpGet("medecin/{idMedecin:int}")]
        public async Task<IActionResult> GetByMedecin(int idMedecin)
        {
            try
            {
                if (idMedecin <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id médecin invalide.";
                    return BadRequest(_response);
                }
                var result = await _inventoryService.GetDistributionsByMedecinAsync(idMedecin);
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
        [HttpGet("pharmacien/{idPharmacien:int}")]
        public async Task<IActionResult> GetByPharmacien(int idPharmacien)
        {
            try
            {
                if (idPharmacien <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id pharmacien invalide.";
                    return BadRequest(_response);
                }
                var result = await _inventoryService.GetDistributionsByPharmacienAsync(idPharmacien);
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
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id distribution invalide.";
                    return BadRequest(_response);
                }
                bool isDeleted = await _inventoryService.DeleteEchantillonAsync(id);
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
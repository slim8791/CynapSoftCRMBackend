using CynapCRM.Services.InventoryAPI.Models;
using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.InventoryAPI.Controllers
{

    // ═══════════════════════════════════════
    // DistributionController.cs
    // ═══════════════════════════════════════

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

        // FIX: route cohérente + accept DTO pas l'entité
        [HttpPost]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> CreateOrUpdateDistribution(
            [FromBody] EchantillonDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données invalides.";
                    return BadRequest(_response);
                }

                // FIX: mapper DTO vers entité avant d'appeler le service
                var echantillon = new Echantillon
                {
                    Id_Distribution = dto.Id_Distribution,
                    Id_Delegue = dto.Id_Delegue,
                    Id_Medecin = dto.Id_Medecin,
                    Id_Pharmacien = dto.Id_Pharmacien,
                    Id_Stock = dto.Id_Stock,
                    Qte = dto.Qte,
                    NumeroLot = dto.NumeroLot,
                    DateDistribution = DateTime.UtcNow,
                    IsDeleted = false
                };

                var result = await _distributionService
                    .CreateOrUpdateEchantillonAsync(echantillon);

                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Erreur lors de l'enregistrement de la distribution.";
                    return BadRequest(_response);
                }

                _response.Message = "Distribution enregistrée avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response); // FIX: 515 → 515
            }
        }

        // FIX: ajout endpoint GetAll manquant
        [HttpGet]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetAllDistributions(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (pageNumber <= 0 || pageSize <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Paramètres de pagination invalides.";
                    return BadRequest(_response);
                }
                var result = await _distributionService
                    .GetAllDistributionsAsync(pageNumber, pageSize);
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

        [HttpGet("{idDistribution:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")] // FIX: ajout restriction
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
                var result = await _distributionService
                    .GetEchantillonByIdAsync(idDistribution);
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
                return StatusCode(515, _response);
            }
        }

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
                var result = await _distributionService
                    .GetDistributionsByMedecinAsync(idMedecin);
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

        // FIX: appelait GetDistributionsByMedecinAsync au lieu de GetDistributionsByDelegueAsync
        [HttpGet("by-delegue/{idDelegue:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetDistributionsByDelegue(int idDelegue)
        {
            try
            {
                if (idDelegue <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id délégué invalide.";
                    return BadRequest(_response);
                }
                // FIX: appel correct GetDistributionsByDelegueAsync
                var result = await _distributionService
                    .GetDistributionsByDelegueAsync(idDelegue);
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
                var result = await _distributionService
                    .GetDistributionsByPharmacienAsync(idPharmacien);
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
                bool isDeleted = await _distributionService
                    .DeleteEchantillonAsync(idDistribution);
                if (!isDeleted)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Distribution inexistante.";
                    return NotFound(_response);
                }
                _response.Message = "Distribution supprimée.";
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
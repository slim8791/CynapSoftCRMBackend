using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.InventoryAPI.Controllers
{

    // ═══════════════════════════════════════
    // InventoryBusinessController.cs
    // ═══════════════════════════════════════

    [Route("api/inventory-business")]
    [ApiController]
    [Authorize]
    public class InventoryBusinessController : ControllerBase
    {
        private readonly IInventoryBusinessService _inventoryBusinessService;
        protected ResponseDto _response;

        public InventoryBusinessController(
            IInventoryBusinessService inventoryBusinessService)
        {
            _inventoryBusinessService = inventoryBusinessService;
            _response = new ResponseDto();
        }

        [HttpGet("check-availability")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> CheckStockAvailability(
            [FromQuery] int idStock,
            [FromQuery] int qte)
        {
            try
            {
                if (idStock <= 0 || qte <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de vérification invalides.";
                    return BadRequest(_response);
                }
                bool available = await _inventoryBusinessService
                    .CheckStockAvailabilityAsync(idStock, qte);
                _response.Result = available;
                _response.IsSuccess = available;
                _response.Message = available
                    ? "Stock disponible."
                    : "Stock insuffisant.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        // FIX: idPharmacien et idMedecin peuvent être 0 (optionnels)
        [HttpPost("distribute-echantillon")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> DistributeEchantillon(
            [FromQuery] int idDelegue,
            [FromQuery] int idStock,
            [FromQuery] int qte,
            [FromQuery] int idPharmacien = 0, // optionnel
            [FromQuery] int idMedecin = 0)    // optionnel
        {
            try
            {
                if (idDelegue <= 0 || idStock <= 0 || qte <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "IdDelegue, IdStock et Qte sont obligatoires.";
                    return BadRequest(_response);
                }

                // Au moins un destinataire requis
                if (idPharmacien <= 0 && idMedecin <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Un médecin ou un pharmacien destinataire est requis.";
                    return BadRequest(_response);
                }

                var result = await _inventoryBusinessService.DistributeEchantillonAsync(
                    idDelegue, idPharmacien, idMedecin, idStock, qte);

                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Distribution impossible (stock insuffisant).";
                    return BadRequest(_response);
                }

                _response.Message = "Distribution effectuée avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpPost("apply-gratuite")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> ApplyGratuite(
            [FromQuery] int idStock,
            [FromQuery] int quantiteAchetee,
            [FromQuery] int seuilPromo)
        {
            try
            {
                if (idStock <= 0 || quantiteAchetee <= 0 || seuilPromo <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de promotion invalides.";
                    return BadRequest(_response);
                }
                bool result = await _inventoryBusinessService
                    .ApplyGratuiteAsync(idStock, quantiteAchetee, seuilPromo);
                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Quantité achetée insuffisante pour déclencher la gratuité.";
                    return BadRequest(_response);
                }
                _response.Message = "Gratuité appliquée avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpPost("reserve-stock")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> ReserveStock(
            [FromQuery] int idStock,
            [FromQuery] int quantite)
        {
            try
            {
                if (idStock <= 0 || quantite <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de réservation invalides.";
                    return BadRequest(_response);
                }
                bool result = await _inventoryBusinessService
                    .ReserveStockAsync(idStock, quantite);
                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Réservation impossible : stock insuffisant.";
                    return BadRequest(_response);
                }
                _response.Message = "Quantité réservée avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        // FIX: endpoint manquant
        [HttpGet("summary/{idDelegue:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetStockSummary(int idDelegue)
        {
            try
            {
                if (idDelegue <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id délégué invalide.";
                    return BadRequest(_response);
                }
                var result = await _inventoryBusinessService
                    .GetStockSummaryByDelegueAsync(idDelegue);
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
    }

}

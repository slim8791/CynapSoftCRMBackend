using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.InventoryAPI.Controllers
{
    [Route("api/stockProcess")]
    [ApiController]
    public class StockProcessController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        protected ResponseDto _response;

        public StockProcessController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
            _response = new ResponseDto();
        }
        // 1. Vérifier si un lot est périmé
        [HttpGet("expired/{numeroLot}")]
        public async Task<IActionResult> IsLotExpired(string numeroLot)
        {
            try
            {
                if (string.IsNullOrEmpty(numeroLot))
                {
                    _response.IsSuccess = false;
                    _response.Message = "Numéro de lot invalide.";
                    return BadRequest(_response);
                }
                bool isExpired = await _inventoryService.IsLotExpiredAsync(numeroLot);
                if (isExpired == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Lot introuvable.";
                    return NotFound(_response);
                }
                _response.Result = isExpired;
                _response.Message = isExpired ? "Attention : Ce lot est périmé." : "Le lot est encore valide.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        // 2. Réserver du stock (bloquer une quantité)
        [HttpPost("Reserve")]
        public async Task<IActionResult> ReserveStock([FromBody] StockMovementDto reserveDto)
        {
            try
            {
                if (reserveDto == null || reserveDto.IdStock <= 0 || reserveDto.Quantity <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de réservation invalides.";
                    return BadRequest(_response);
                }

                bool result = await _inventoryService.ReserveStockAsync(reserveDto.IdStock, reserveDto.Quantity);
                
                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Réservation impossible : Stock disponible insuffisant.";
                    return BadRequest(_response);
                }
                _response.Message = "Quantité réservée avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        // 3. Appliquer la règle de gratuité (Promotion)
        [HttpPost("ApplyPromotion")]
        public async Task<IActionResult> ApplyPromotion(int idStock, int qteAchetee, int seuil)
        {
            try
            {
                if (idStock <= 0 || qteAchetee <= 0 || seuil <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de promotion invalides.";
                    return BadRequest(_response);
                }
                bool result = await _inventoryService.ApplyGratuiteAsync(idStock, qteAchetee, seuil);
                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Erreur lors de l'application de la gratuité.";
                    return BadRequest(_response);
                }
                _response.Message = "Gratuité calculée et ajoutée au stock.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        // 4. Historique des mouvements d'une ligne de stock précise
        [HttpGet("movements/{idStock:int}")]
        public async Task<IActionResult> GetMovements(int idStock)
        {
            try
            {
                if (idStock <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id stock invalide.";
                    return BadRequest(_response);
                }
                var result = await _inventoryService.GetStockMovementsAsync(idStock);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Ligne de stock introuvable.";
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

        // 5. Disponibilité rapide (pour vérification UI avant validation)
        [HttpGet("checkAvailability")]
        public async Task<IActionResult> CheckAvailability(int idStock, int qte)
        {
            try
            {
                if (idStock <= 0 || qte <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de vérification invalides.";
                    return BadRequest(_response);
                }
                bool available = await _inventoryService.CheckStockAvailabilityAsync(idStock, qte);
                if (!available)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Stock disponible insuffisant pour la quantité demandée.";
                    return BadRequest(_response);
                }
                _response.Result = available;
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

using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.InventoryAPI.Controllers
{



    // ═══════════════════════════════════════
    // StockMovementController.cs
    // ═══════════════════════════════════════

    [Route("api/stock-movements")]
    [ApiController]
    [Authorize(Roles = "ADMIN,SUPERVISEUR")] // correct — niveau controller
    public class StockMovementController : ControllerBase
    {
        private readonly IStockMovementService _stockMovementService;
        protected ResponseDto _response;

        public StockMovementController(IStockMovementService stockMovementService)
        {
            _stockMovementService = stockMovementService;
            _response = new ResponseDto();
        }

        [HttpPost("decrement")]
        public async Task<IActionResult> DecrementStock(
            [FromQuery] int idStock,
            [FromQuery] int qte)
        {
            try
            {
                // FIX: validation idStock manquante
                if (idStock <= 0 || qte <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "IdStock et Qte doivent être supérieurs à zéro.";
                    return BadRequest(_response);
                }
                bool result = await _stockMovementService.DecrementStockAsync(idStock, qte);
                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Stock insuffisant ou inexistant.";
                    return BadRequest(_response);
                }
                _response.Message = "Stock décrémenté et mouvement enregistré.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpPost("increment")]
        public async Task<IActionResult> IncrementStock(
            [FromQuery] int idStock,
            [FromQuery] int qte)
        {
            try
            {
                // FIX: validation idStock manquante
                if (idStock <= 0 || qte <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "IdStock et Qte doivent être supérieurs à zéro.";
                    return BadRequest(_response);
                }
                bool result = await _stockMovementService.IncrementStockAsync(idStock, qte);
                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Stock introuvable.";
                    return BadRequest(_response);
                }
                _response.Message = "Stock incrémenté et mouvement enregistré.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> TransferStock(
            [FromQuery] int idStockSource,
            [FromQuery] int idStockDestination,
            [FromQuery] int qte)
        {
            try
            {
                // FIX: validation ids manquante
                if (idStockSource <= 0 || idStockDestination <= 0 || qte <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Paramètres de transfert invalides.";
                    return BadRequest(_response);
                }
                // FIX: vérifier source ≠ destination
                if (idStockSource == idStockDestination)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Source et destination ne peuvent pas être identiques.";
                    return BadRequest(_response);
                }
                bool result = await _stockMovementService.TransferStockAsync(
                    idStockSource, idStockDestination, qte);
                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Transfert impossible : vérifiez les stocks.";
                    return BadRequest(_response);
                }
                _response.Message = "Transfert effectué et mouvements tracés.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpGet("{idStock:int}")]
        public async Task<IActionResult> GetStockMovements(int idStock)
        {
            try
            {
                if (idStock <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id stock invalide.";
                    return BadRequest(_response);
                }
                var result = await _stockMovementService.GetStockMovementsAsync(idStock);
                _response.Result = result; // FIX: résultat non assigné dans l'original
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        // FIX: endpoint manquant — historique par délégué
        [HttpGet("by-delegue/{idDelegue:int}")]
        public async Task<IActionResult> GetMovementsByDelegue(int idDelegue)
        {
            try
            {
                if (idDelegue <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id délégué invalide.";
                    return BadRequest(_response);
                }
                var result = await _stockMovementService
                    .GetMovementHistoryByDelegueAsync(idDelegue);
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

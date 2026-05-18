using CynapCRM.Services.InventoryAPI.Models;
using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.InventoryAPI.Controllers
{

    // ═══════════════════════════════════════
    // StocksDelegueController.cs
    // ═══════════════════════════════════════

    [Route("api/stocks-delegue")]
    [ApiController]
    [Authorize]
    public class StocksDelegueController : ControllerBase
    {
        private readonly IStockDelegueService _stockDelegueService;
        protected ResponseDto _response;

        public StocksDelegueController(IStockDelegueService stockDelegueService)
        {
            _stockDelegueService = stockDelegueService;
            _response = new ResponseDto();
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetAllStocks(
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
                _response.Result = await _stockDelegueService
                    .GetAllStocksAsync(pageNumber, pageSize);
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
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetStockById(int idStock)
        {
            try
            {
                if (idStock <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id stock invalide.";
                    return BadRequest(_response);
                }
                var stock = await _stockDelegueService.GetStockByIdAsync(idStock);
                if (stock == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Stock introuvable.";
                    return NotFound(_response);
                }
                _response.Result = stock;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpGet("by-delegue/{idDelegue:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetStocksByDelegue(int idDelegue)
        {
            try
            {
                if (idDelegue <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id délégué invalide.";
                    return BadRequest(_response);
                }
                var result = await _stockDelegueService.GetStocksByDelegueAsync(idDelegue);
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

        [HttpGet("by-produit/{idProduit:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetStocksByProduit(int idProduit)
        {
            try
            {
                if (idProduit <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id produit invalide.";
                    return BadRequest(_response);
                }
                var result = await _stockDelegueService.GetStockByProduitAsync(idProduit);
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

        [HttpGet("by-lot/{numeroLot}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetStockByLot(string numeroLot)
        {
            try
            {
                // FIX: string.IsNullOrWhiteSpace au lieu de == null
                if (string.IsNullOrWhiteSpace(numeroLot))
                {
                    _response.IsSuccess = false;
                    _response.Message = "Numéro de lot invalide.";
                    return BadRequest(_response);
                }
                var stock = await _stockDelegueService.GetStockByLotAsync(numeroLot);
                if (stock == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = $"Aucun stock trouvé pour le lot {numeroLot}.";
                    return NotFound(_response);
                }
                _response.Result = stock;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        // FIX: route cohérente
        [HttpPost]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> CreateOrUpdateStock(
            [FromBody] StockDelegueDto stockDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de stock invalides.";
                    return BadRequest(_response);
                }
                var result = await _stockDelegueService.CreateUpdateStockAsync(stockDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Erreur lors de la mise à jour du stock.";
                    return BadRequest(_response);
                }
                _response.Result = result;
                _response.Message = "Stock enregistré avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpDelete("{idStock:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteStock(
            int idStock,
            [FromQuery] StockType type)
        {
            try
            {
                if (idStock <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id stock invalide.";
                    return BadRequest(_response);
                }
                bool isDeleted = await _stockDelegueService.DeleteStockAsync(idStock, type);
                if (!isDeleted)
                {
                    _response.IsSuccess = false;
                    // FIX: message plus précis
                    _response.Message = "Suppression impossible (stock inexistant ou quantité restante > 0).";
                    return BadRequest(_response);
                }
                _response.Message = "Stock supprimé.";
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
using CynapCRM.Services.InventoryAPI.Models;
using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.InventoryAPI.Controllers
{

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
        public async Task<IActionResult> GetAllStocks([FromQuery] int pageNumber = 1,
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

                _response.Result = await _stockDelegueService.GetAllStocksAsync(pageNumber, pageSize);
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
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
                    _response.Message = "Id stock invalide.";
                    return BadRequest(_response);
                }
                var result = await _stockDelegueService.GetStocksByDelegueAsync(idDelegue);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Ligne de stock introuvable.";
                    return NotFound(_response);
                }
                _response.Result = result;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
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
                    _response.Message = "Ligne de stock introuvable.";
                    return NotFound(_response);
                }
                _response.Result = stock;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
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
                    _response.Message = "Id stock invalide.";
                    return BadRequest(_response);
                }
                var result = await _stockDelegueService.GetStockByProduitAsync(idProduit);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Ligne de stock introuvable.";
                    return NotFound(_response);
                }
                _response.Result = result;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        [HttpGet("by-lot/{numeroLot}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetStockByLot(string numeroLot)
        {
            try
            {
                if (numeroLot == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id stock invalide.";
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
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }
        [HttpPost("stock")]

        [Authorize(Roles = "ADMIN,SUPERVISEUR")]

        public async Task<IActionResult> CreateOrUpdateStock([FromBody] StockDelegueDto stockDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de stocks invalides.";
                    return BadRequest(ModelState);
                }

                var result = await _stockDelegueService.CreateUpdateStockAsync( stockDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Erreur lors de la mise à jour.";
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
                return StatusCode(500, _response);
            }
        }

        [HttpDelete("{idStock:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteStock(int idStock,[FromQuery] StockType type)

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
                    _response.Message = "Impossible de supprimer : stock inexistant.";
                    return BadRequest(_response);
                }

                _response.Message = "Ligne de stock supprimée.";
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
using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.InventoryAPI.Controllers
{
    [Route("api/stockDelegue")]
    [ApiController]
    public class StocksDelegueController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        protected ResponseDto _response;

        public StocksDelegueController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
            _response = new ResponseDto();
        }
        [HttpGet]
        public async Task<IActionResult> GetAllStocks(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                _response.Result = await _inventoryService.GetAllStocksAsync(pageNumber, pageSize);
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        // 1. Récupérer tout le stock d'un délégué
        [HttpGet("delegue/{idDelegue:int}")]
        public async Task<IActionResult> GetByDelegue(int idDelegue)
        {
            try
            {
                if (idDelegue <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id stock invalide.";
                    return BadRequest(_response);
                }
                var result = await _inventoryService.GetStocksByDelegueAsync(idDelegue);
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

        // 2. Récupérer le stock par ID
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id stock invalide.";
                    return BadRequest(_response);
                }
                var stock = await _inventoryService.GetStockByIdAsync(id);
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

        // 3. Récupérer le stock d'un produit spécifique
        [HttpGet("produit/{idProduit:int}")]
        public async Task<IActionResult> GetByProduit(int idProduit)
        {
            try
            {
                if (idProduit <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id stock invalide.";
                    return BadRequest(_response);
                }
                var result = await _inventoryService.GetStockByProduitAsync(idProduit);
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

        // 4. Récupérer par Numéro de Lot
        [HttpGet("lot/{lot:int}")]
        public async Task<IActionResult> GetByLot(string lot)
        {
            try
            {
                if (lot == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id stock invalide.";
                    return BadRequest(_response);
                }
                var stock = await _inventoryService.GetStockByLotAsync(lot);
                if (stock == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = $"Aucun stock trouvé pour le lot {lot}.";
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

        // 5. Créer ou Mettre à jour un stock
        [HttpPost("stock")]
        public async Task<IActionResult> CreateUpdateStock([FromBody] StockDelegueDto stockDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de stocks invalides.";
                    return BadRequest(ModelState);
                }

                var result = await _inventoryService.CreateUpdateStockAsync(stockDto);
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

        // 6. Supprimer une ligne de stock
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int idStock, string type)
        {
            try
            {
                if (idStock <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id stock invalide.";
                    return BadRequest(_response);
                }
                bool isDeleted = await _inventoryService.DeleteStockAsync(idStock, type);
                if (!isDeleted)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Impossible de supprimer : stock inexistant.";
                    return NotFound(_response);
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
using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.InventoryAPI.Controllers
{
    [Route("api/stockEchantillon")]
    [ApiController]
    public class StockEchantillonController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        protected ResponseDto _response;

        public StockEchantillonController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
            _response = new ResponseDto();
        }
        // 1. Créer ou Mettre à jour un stock d'échantillon
        [HttpPost("echantillon")]
        public async Task<IActionResult> CreateUpdateEchantillonStock([FromBody] StockEchantillonDto echantillonDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données d'entrée invalides.";
                    return BadRequest(ModelState);
                }

                var result = await _inventoryService.CreateUpdateStockEchantillonAsync(echantillonDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Erreur lors du traitement du stock échantillon.";
                    return BadRequest(_response);
                }

                _response.Result = result;
                _response.Message = "Stock échantillon mis à jour avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        // 2. Récupérer un échantillon par ID
        [HttpGet("echantillon/{id:int}")]
        public async Task<IActionResult> GetEchantillonStockById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id stock invalide.";
                    return BadRequest(_response);
                }
                var result = await _inventoryService.GetStockEchantillonByIdAsync(id);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Stock échantillon introuvable.";
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
    }
}

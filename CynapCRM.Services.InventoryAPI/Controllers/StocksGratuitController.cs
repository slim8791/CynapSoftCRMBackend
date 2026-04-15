using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.InventoryAPI.Controllers
{
    [Route("api/stockGratuit")]
    [ApiController]
    public class StocksGratuitController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        protected ResponseDto _response;

        public StocksGratuitController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
            _response = new ResponseDto();
        }
        [HttpPost("Gratuite")]
        public async Task<IActionResult> CreateUpdateGratuite([FromBody] StockGratuiteDto gratuiteDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données invalides.";
                    return BadRequest(ModelState);
                }

                var result = await _inventoryService.CreateUpdateStockGratuiteAsync(gratuiteDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Erreur lors du traitement de la gratuité.";
                    return BadRequest(_response);
                }

                _response.Result = result;
                _response.Message = "Stock de gratuité mis à jour.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        // 2. Récupérer une gratuité par ID
        [HttpGet("gratuite/{id:int}")]
        public async Task<IActionResult> GetGratuiteById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id stock invalide.";
                    return BadRequest(_response);
                }
                var result = await _inventoryService.GetStockGratuiteByIdAsync(id);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Gratuité introuvable.";
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
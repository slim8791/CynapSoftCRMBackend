using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.InventoryAPI.Controllers
{
    [Route("api/stockMovement")]
    [ApiController]

    public class StockMovementController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        protected ResponseDto _response;

        public StockMovementController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
            _response = new ResponseDto();
        }
        // 1. Décrémenter le stock (Ajustement négatif)
        [HttpPost("decrement")]
        public async Task<IActionResult> Decrement([FromBody] StockMovementDto movementDto)
        {
            try
            {
                if (movementDto.Quantity <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "La quantité doit être supérieure à zéro pour un décrément.";
                    return BadRequest(_response);
                }
                // On utilise movementDto.IdStock et movementDto.Quantity
                bool result = await _inventoryService.DecrementStockAsync(movementDto.IdStock, movementDto.Quantity);

                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Échec du décrément : Stock insuffisant ou inexistant.";
                    return BadRequest(_response);
                }

                _response.Message = "Stock décrémenté et mouvement enregistré.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        // 2. Incrémenter le stock (Ajustement positif)
        [HttpPost("increment")]
        public async Task<IActionResult> Increment([FromBody] StockMovementDto movementDto)
        {
            try
            {
                if (movementDto.Quantity <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "La quantité doit être supérieure à zéro pour un incrément.";
                    return BadRequest(_response);
                }

                bool result = await _inventoryService.IncrementStockAsync(movementDto.IdStock, movementDto.Quantity);

                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Échec de l'incrément : Stock introuvable.";
                    return BadRequest(_response);
                }

                _response.Message = "Stock incrémenté et mouvement enregistré.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        // 3. Transférer du stock
        // Note : Pour le transfert, on a besoin de deux IDs. 
        // Si ton DTO n'a qu'un seul IdStock, tu peux passer l'Id destination dans l'URL
        [HttpPost("transfer/{idStockDestination:int}")]
        public async Task<IActionResult> Transfer([FromBody] StockMovementDto movementDto, int idStockDestination)
        {
            try
            {
                if (movementDto.Quantity <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "La quantité doit être supérieure à zéro pour un transfert.";
                    return BadRequest(_response);
                }
                // movementDto.IdStock sert d'ID Source
                bool result = await _inventoryService.TransferStockAsync(
                    movementDto.IdStock,
                    idStockDestination,
                    movementDto.Quantity);

                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Transfert impossible : vérifiez les stocks source/destination.";
                    return BadRequest(_response);
                }

                _response.Message = "Transfert effectué et mouvements tracés.";
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


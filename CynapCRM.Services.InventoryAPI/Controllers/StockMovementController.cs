using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.InventoryAPI.Controllers
{

    [Route("api/stock-movements")]
    [ApiController]
    [Authorize(Roles = "ADMIN,SUPERVISEUR")]

    public class StockMovementController : ControllerBase
    {
        private readonly IStockMovementService _stockMovementService;
        protected ResponseDto _response;

        public StockMovementController(IStockMovementService stockMovementService)
        {
            _stockMovementService = stockMovementService    ;
            _response = new ResponseDto();
        }
        
        [HttpPost("decrement")]
        public async Task<IActionResult> DecrementStock([FromQuery] int idStock, [FromQuery] int qte)
        {
            try
            {
                if (qte <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "La quantité doit être supérieure à zéro pour un décrément.";
                    return BadRequest(_response);
                }
                bool result = await _stockMovementService.DecrementStockAsync(idStock, qte);

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
                return StatusCode(515, _response);
            }
        }

        [HttpPost("increment")]
        public async Task<IActionResult> IncrementStock([FromQuery] int idStock,[FromQuery] int qte)
        {

            try
            {
                if (qte <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "La quantité doit être supérieure à zéro pour un incrément.";
                    return BadRequest(_response);
                }

                bool result = await _stockMovementService.IncrementStockAsync(idStock, qte);

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
                return StatusCode(515, _response);
            }
        }
        [HttpPost("transfer")]
        public async Task<IActionResult> TransferStock([FromQuery] int idStockSource,
                    [FromQuery] int idStockDestination,
                    [FromQuery] int qte)
        {

            try
            {
                if (qte <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "La quantité doit être supérieure à zéro pour un transfert.";
                    return BadRequest(_response);
                }
                bool result = await _stockMovementService.TransferStockAsync(idStockSource,idStockDestination,
                    qte);

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
                return StatusCode(515, _response);
            }
        }
    }
}


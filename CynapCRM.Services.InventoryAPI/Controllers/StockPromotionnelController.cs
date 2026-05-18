using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.InventoryAPI.Controllers
{

    // ═══════════════════════════════════════
    // StockPromotionnelController.cs
    // ═══════════════════════════════════════

    [ApiController]
    [Route("api/stocks-promotionnels")]
    [Authorize]
    public class StockPromotionnelController : ControllerBase
    {
        private readonly IStockPromotionnelService _stockPromotionnelService;
        protected ResponseDto _response;

        public StockPromotionnelController(
            IStockPromotionnelService stockPromotionnelService)
        {
            _stockPromotionnelService = stockPromotionnelService;
            _response = new ResponseDto();
        }

        // FIX: ajout restriction de rôle
        [HttpPost("gratuite")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> CreateOrUpdateGratuite(
            [FromBody] StockGratuiteDto gratuiteDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données invalides.";
                    return BadRequest(_response);
                }
                var result = await _stockPromotionnelService
                    .CreateUpdateStockGratuiteAsync(gratuiteDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Erreur lors du traitement de la gratuité.";
                    return BadRequest(_response); // FIX: NotFound → BadRequest
                }
                _response.Result = result;
                _response.Message = "Stock de gratuité mis à jour.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpGet("gratuite/{idStock:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetStockGratuiteById(int idStock)
        {
            try
            {
                if (idStock <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id stock invalide.";
                    return BadRequest(_response);
                }
                var result = await _stockPromotionnelService
                    .GetStockGratuiteByIdAsync(idStock);
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
                return StatusCode(515, _response);
            }
        }

        [HttpPost("echantillon")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> CreateOrUpdateEchantillonStock(
            [FromBody] StockEchantillonDto echantillonDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données invalides.";
                    return BadRequest(_response);
                }
                var result = await _stockPromotionnelService
                    .CreateUpdateStockEchantillonAsync(echantillonDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Erreur lors du traitement du stock échantillon.";
                    return BadRequest(_response); // FIX: NotFound → BadRequest
                }
                _response.Result = result;
                _response.Message = "Stock échantillon mis à jour avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpGet("echantillon/{idStock:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetStockEchantillonById(int idStock)
        {
            try
            {
                if (idStock <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id stock invalide.";
                    return BadRequest(_response);
                }
                var result = await _stockPromotionnelService
                    .GetStockEchantillonByIdAsync(idStock);
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
                return StatusCode(515, _response);
            }
        }
    }


}

using CynapCRM.Services.ProductAPI.Models.Dto;
using CynapCRM.Services.ProductAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.ProductAPI.Controllers
{
    [Route("api/promos")]
    [ApiController]
    [Authorize]
    public class PromoController : ControllerBase
    {
        private readonly IPromoService _promoService;
        protected ResponseDto _response;

        public PromoController(IPromoService promoService)
        {
            _promoService = promoService;
            _response = new ResponseDto();
        }

        // Gestion des promotions

        [HttpGet]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetAllPromotions()
        {
            try
            {
                var result = await _promoService.GetAllPromotionsAsync();

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

        [HttpGet("{promotionId:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]

        public async Task<IActionResult> GetPromotionById(int promotionId)
        {
            try
            {
                var result = await _promoService.GetPromotionByIdAsync(promotionId);

                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Promotion introuvable.";
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

        [HttpPost]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> CreateOrUpdatePromotion([FromBody] PromotionDto promotionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de promotion invalides.";
                    return BadRequest(_response);
                }

                var result = await _promoService.CreateOrUpdatePromotionAsync(promotionDto);
                _response.Result = result;
                _response.Message = "Promotion enregistrée avec succès.";

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpDelete("{promotionId:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeletePromotion(int promotionId)
        {
            try
            {
                var result = await _promoService.DeletePromotionAsync(promotionId);

                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Promotion introuvable ou déjà supprimée.";
                    return NotFound(_response);
                }

                _response.Message = "Promotion supprimée avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        //  Application / logique métier

        [HttpGet("product/{productId:int}/apply")]
        public async Task<IActionResult> ApplyBestPromotion(int productId,  [FromQuery] decimal initialPrice)
        {
            try
            {
                if (initialPrice <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Prix initial invalide.";
                    return BadRequest(_response);
                }

                var finalPrice = await _promoService.ApplyBestPromotionAsync(productId, initialPrice);

                _response.Result = finalPrice;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpGet("product/{productId:int}/in-promotion")]
        public async Task<IActionResult> IsProductInPromotion(int productId)
        {
            try
            {
                var result = await _promoService.IsProductInPromotionAsync(productId);
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


        [HttpGet("product/{productId:int}")]
        public async Task<IActionResult> GetPromotionsByProduct(int productId)
        {
            try
            {
                var result = await _promoService.GetPromotionsByProductAsync(productId);

                if (result == null || !result.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucune promotion trouvée pour ce produit.";
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

        [HttpGet("lot/{numeroLot}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetPromotionsByLot(string numeroLot)
        {
            try
            {
                var result = await _promoService.GetPromotionsByLotAsync(numeroLot);

                if (result == null || !result.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucune promotion trouvée pour ce lot.";
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
    
        //  Validation / pilotage

        [HttpGet("{promotionId:int}/valid")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> IsPromotionValid(int promotionId)
        {
            try
            {
                var result = await _promoService.IsPromotionValidAsync(promotionId);
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
        [HttpGet("{promotionId:int}/applicable")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> IsPromotionApplicable( int promotionId,[FromQuery] DateTime referenceDate)
        {
            try
            {
                var result = await _promoService.IsPromotionApplicableAsync(promotionId,referenceDate);
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

        [HttpGet("coverage-rate")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetPromotionCoverageRate()
        {
            try
            {
                var result = await _promoService.GetPromotionCoverageRateAsync();
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

        [HttpGet("active-count")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetActivePromotionsCount()
        {
            try
            {
                var result = await _promoService.GetActivePromotionsCountAsync();
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
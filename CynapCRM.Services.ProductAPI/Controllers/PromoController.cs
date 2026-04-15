using CynapCRM.Services.ProductAPI.Models.Dto;
using CynapCRM.Services.ProductAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Eventing.Reader;

namespace CynapCRM.Services.ProductAPI.Controllers
{
    [Route("api/promo")]
    [ApiController]
    [Authorize]
    public class PromoController : ControllerBase
    {
        private readonly IProductService _productService;

        protected ResponseDto _response;
        public PromoController(IProductService productService)

        {

            _productService = productService;
            _response = new();
        }

        [HttpGet("promotions")]
        public async Task<IActionResult> GetAllPromotions()
        {
            try
            {
                _response.Result = await _productService.GetPromotionsAsync();
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        [HttpPost("promotion")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateUpdatePromotion([FromBody] PromotionDto promotionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données invalides.";
                    return BadRequest(_response);
                }
                var result = await _productService.CreateUpdatePromotionAsync(promotionDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Erreur lors de la création ou de la mise à jour de la promotion.";
                    return BadRequest(_response);
                }
                _response.Result = result;
                _response.Message = promotionDto.Id_Promo == 0 ? "Promotion créée avec succès." : "Promotion mise à jour avec succès.";
                return Ok(_response);



            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        [HttpDelete("promotion/{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeletePromotion(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de promotion invalide.";
                    return BadRequest(_response);
                }
                bool isDeleted = await _productService.DeletePromotionAsync(id);
                if (!isDeleted)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Promotion non trouvée ou déjà supprimée.";
                    return NotFound(_response);
                }
                _response.Message = "Promotion supprimée avec succès.";
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

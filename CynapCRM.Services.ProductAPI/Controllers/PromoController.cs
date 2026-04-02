using CynapCRM.Services.ProductAPI.Models.Dto;
using CynapCRM.Services.ProductAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ResponseDto> GetAllPromotions()
        {
            try
            {
                _response.Result = await _productService.GetPromotionsAsync();
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpPost("promotion")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ResponseDto> CreateUpdatePromotion([FromBody] PromotionDto promotionDto)
        {
            try
            {
                _response.Result = await _productService.CreateUpdatePromotionAsync(promotionDto);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpDelete("promotion/{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ResponseDto> DeletePromotion(int id)
        {
            try
            {
                _response.Result = await _productService.DeletePromotionAsync(id);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

    }
}

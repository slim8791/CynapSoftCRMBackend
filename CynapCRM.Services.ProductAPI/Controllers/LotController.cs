using CynapCRM.Services.ProductAPI.Models.Dto;
using CynapCRM.Services.ProductAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.ProductAPI.Controllers
{
    [Route("api/lot")]
    [ApiController]
    [Authorize]
    public class LotController : ControllerBase
    {
        private readonly IProductService _productService;

        protected ResponseDto _response; 
        public LotController(IProductService productService)

        {
            _productService = productService;

            _response = new();
        }

        [HttpGet("{id:int}/lots")]
        public async Task<ResponseDto> GetLotsByIdProduct(int id)
        {
            try
            {
                _response.Result = await _productService.GetLotsByProductIdAsync(id);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpPost("lot")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<ResponseDto> CreateUpdateLot([FromBody] LotDto lotDto)
        {
            try
            {
                _response.Result = await _productService.CreateUpdateLotAsync(lotDto);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpDelete("lot/{numeroLot}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ResponseDto> DeleteLot(string numeroLot)
        {
            try
            {
                _response.Result = await _productService.DeleteLotAsync(numeroLot);
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

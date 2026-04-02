using CynapCRM.Services.ProductAPI.Models.Dto;
using CynapCRM.Services.ProductAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.ProductAPI.Controllers
{
    [Route("api/support")]
    [ApiController]
    [Authorize]
    public class SupportController : ControllerBase

    {

        private readonly IProductService _productService;

        protected ResponseDto _response;

        public SupportController(IProductService productService)
        {
            _productService = productService;
            _response = new();
        }


        [HttpGet("{id:int}/supports")]
        public async Task<ResponseDto> GetSupportsByIdProduct(int id)
        {
            try
            {
                _response.Result = await _productService.GetSupportsByProductIdAsync(id);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpPost("support")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<ResponseDto> CreateUpdateSupport([FromBody] SupportMarketingDto supportDto)
        {
            try
            {
                _response.Result = await _productService.CreateUpdateSupportAsync(supportDto);
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

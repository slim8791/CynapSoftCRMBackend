using CynapCRM.Services.ProductAPI.Models.Dto;
using CynapCRM.Services.ProductAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.ProductAPI.Controllers
{
    [Route("api/fichier")]
    [ApiController]
    [Authorize]
    public class FichierController : ControllerBase
    {
        private readonly IProductService _productService;
        protected ResponseDto _response;
        public FichierController(IProductService productService)
        {
            _productService = productService;
            _response = new();
        }
        [HttpPost("fichier")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<ResponseDto> AddFichier([FromBody] FichierDto fichierDto)
        {
            try
            {
                _response.Result = await _productService.AddFichierToSupportAsync(fichierDto);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpDelete("fichier/{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ResponseDto> DeleteFichier(int id)
        {

            try
            {
                _response.Result = await _productService.DeleteFichierAsync(id);
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

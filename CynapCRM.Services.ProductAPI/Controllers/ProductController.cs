using CynapCRM.Services.ProductAPI.Models.Dto;
using CynapCRM.Services.ProductAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.ProductAPI.Controllers
{
    [Route("api/produit")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        protected ResponseDto _response;

        public ProductController(IProductService productService)
        {
            _productService = productService;

            _response = new();
        }

        [HttpGet]
        public async Task<ResponseDto> GetAllProducts()
        {
            try
            {
                _response.Result = await _productService.GetProductsAsync();
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpGet("{id:int}")]
        public async Task<ResponseDto> GetProductById(int id)
        {
            try
            {
                _response.Result = await _productService.GetProductByIdAsync(id);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
        [HttpPost]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<ResponseDto> CreateUpdateProduct([FromBody] ProduitDto produitDto)
        {
            try
            {
                _response.Result = await _productService.CreateUpdateProductAsync(produitDto);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ResponseDto> DeleteProduct(int id)
        {
            try
            {
                _response.Result = await _productService.DeleteProductAsync(id);
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
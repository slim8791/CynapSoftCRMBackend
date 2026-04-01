using CynapCRM.Services.ProductAPI.Models.Dto;
using CynapCRM.Services.ProductAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.ProductAPI.Controllers
{
    [Route("api/produit")]
    [ApiController]
    [Authorize]
    public class ProduitController : ControllerBase
    {
        private readonly IProductService _productService;
        protected ResponseDto _response;

        public ProduitController(IProductService productService)
        {
            _productService = productService;
            _response = new();
        }

        // 1. Route produit
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

        // 2. Gestion des lots
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

        // 3. Gestion des promotions
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

        // 4. Gestion marketting
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
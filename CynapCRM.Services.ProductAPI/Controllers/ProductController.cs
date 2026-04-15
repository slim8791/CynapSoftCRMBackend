using CynapCRM.Services.ProductAPI.Models.Dto;
using CynapCRM.Services.ProductAPI.Service;
using CynapCRM.Services.ProductAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using System.Diagnostics.Eventing.Reader;

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
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                _response.Result = await _productService.GetProductsAsync();
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de produit invalide.";
                    return BadRequest(_response);
                }
                var result = await _productService.GetProductByIdAsync(id);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Produit non trouvé.";
                    return NotFound(_response);
                }
                _response.Result = result;
                _response.IsSuccess = true;
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }
        [HttpPost]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> CreateUpdateProduct([FromBody] ProduitDto produitDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de produit invalides.";
                    return BadRequest(_response);
                }
                var result = await _productService.CreateUpdateProductAsync(produitDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Erreur lors de la création ou de la mise à jour du produit.";
                    return StatusCode(500, _response);
                }
                _response.Result = result;
                _response.Message = produitDto.Id_Produit == 0 ? "Produit créé avec succès." : "Produit mis à jour avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);

            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de produit invalide.";
                    return BadRequest(_response);
                }
                bool isDeleted = await _productService.DeleteProductAsync(id);
                if (!isDeleted)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Produit non trouvé ou déjà supprimé.";
                    return NotFound(_response);
                }
                _response.Message = "Produit supprimé avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string keyword)
        {
            try
            {
                // On récupère les 10 meilleurs résultats
                var results = await _productService.SearchProductsAsync(keyword);

                _response.Result = results;
                _response.IsSuccess = true;

                // Si aucun résultat, on peut envoyer un message informatif
                if (results == null || !results.Any())
                {
                    _response.Message = "Aucun produit ne correspond à votre recherche.";
                }
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "Erreur lors de la recherche : " + ex.Message;
                return StatusCode(500, _response);
            }
        }
    }
}
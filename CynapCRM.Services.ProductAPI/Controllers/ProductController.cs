using CynapCRM.Services.ProductAPI.Models.Dto;
using CynapCRM.Services.ProductAPI.Service;
using CynapCRM.Services.ProductAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using System.Diagnostics.Eventing.Reader;

namespace CynapCRM.Services.ProductAPI.Controllers
{
    [Route("api/products")]
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
                _response.Result = await _productService.GetAllProductsAsync();
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
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
                return StatusCode(515, _response);
            }
        }
        [HttpGet("visible")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetVisibleProducts()
        {
            try
            {
                var result = await _productService.GetVisibleProductsAsync();

                if (result == null || !result.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucun produit visible.";
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
        public async Task<IActionResult> CreateOrUpdateProduct([FromBody] ProduitDto produitDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données produit invalides.";
                    return BadRequest(_response);
                }

                var result = await _productService.CreateOrUpdateProductAsync(produitDto);

                _response.Result = result;
                _response.Message = "Produit enregistré avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }
        [HttpPut("{productId:int}/archive")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> ArchiveProduct(int productId)
        {
            try
            {
                var result = await _productService.ArchiveProductAsync(productId);

                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Produit introuvable ou non archivable.";
                    return BadRequest(_response);
                }

                _response.Message = "Produit archivé avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }
        [HttpPut("{productId:int}/unarchive")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UnarchiveProduct(int productId)
        {
            try
            {
                var result = await _productService.UnarchiveProductAsync(productId);

                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Produit introuvable ou non désarchivable.";
                    return BadRequest(_response);
                }

                _response.Message = "Produit désarchivé avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }
        [HttpPut("{productId:int}/activate")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> ActivateProduct(int productId)
        {
            try
            {
                var result = await _productService.ActivateProductAsync(productId);

                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Impossible d’activer le produit.";
                    return BadRequest(_response);
                }

                _response.Message = "Produit activé avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpPut("{id:int}/deactivate")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeactivateProduct(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de produit invalide.";
                    return BadRequest(_response);
                }
                bool isDeleted = await _productService.DeactivateProductAsync(id);
                if (!isDeleted)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Produit non trouvé ou déjà supprimé.";
                    return NotFound(_response);
                }
                _response.Message = "Produit désactivé avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }
        [HttpGet("{productId:int}/available")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> IsProductAvailable(int productId)
        {
            try
            {
                var result = await _productService.IsProductAvailableAsync(productId);
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
        [HttpGet("available")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetAvailableProducts()
        {
            try
            {
                var result = await _productService.GetAvailableProductsAsync();

                if (result == null || !result.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucun produit disponible.";
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
        [HttpGet("unavailable")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetUnavailableProducts()
        {
            try
            {
                var result = await _productService.GetUnavailableProductsAsync();

                if (result == null || !result.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucun produit indisponible.";
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
        [HttpGet("{productId:int}/stock")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetTotalStock(int productId)
        {
            try
            {
                var result = await _productService.GetTotalStockAsync(productId);
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
        [HttpGet("stock-status")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetStockStatus()
        {
            try
            {
                var result = await _productService.GetStockStatusAsync();
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
        [HttpGet("low-stock")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetLowStockProducts([FromQuery] int seuil)
        {
            try
            {
                if (seuil < 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Seuil de stock invalide.";

                    return BadRequest(_response);
                }

                var result = await _productService.GetLowStockProductsAsync(seuil);

                if (result == null || !result.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucun produit avec stock faible.";
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
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string keyword, [FromQuery] bool isActive = true, [FromQuery] bool allowArchived = false, [FromQuery] int limit = 10)
        {
            try
            {
                // On récupère les 10 meilleurs résultats
                var results = await _productService.SearchProductsAsync(keyword, isActive, allowArchived, limit);

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
                return StatusCode(515, _response);
            }
        }
        [HttpGet("filter")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> FilterProducts([FromQuery] string? keyword,
                                                        [FromQuery] string? category,
                                                        [FromQuery] bool? allowArchived,
                                                        [FromQuery] bool? isActive,
                                                        [FromQuery] int page = 1,
                                                        [FromQuery] int pageSize = 20)
        {
            try
            {
                if (page <= 0 || pageSize <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Paramètres de pagination invalides.";
                    return BadRequest(_response);
                }

                var result = await _productService.FilterProductsAsync(
                    keyword,
                    isActive,
                    allowArchived,
                    category,
                    page,
                    pageSize);

                if (result == null || !result.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucun produit trouvé selon les critères.";
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
        [HttpGet("categories")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var result = await _productService.GetCategoriesAsync();

                if (result == null || !result.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucune catégorie trouvée.";
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
        [HttpGet("category/{category}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetProductsByCategory(string category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category))
                {
                    _response.IsSuccess = false;
                    _response.Message = "Catégorie invalide.";
                    return BadRequest(_response);
                }

                var result = await _productService.GetProductsByCategoryAsync(category);

                if (result == null || !result.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucun produit trouvé pour cette catégorie.";
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
        [HttpGet("exists")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> ProductExists([FromQuery] string productName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(productName))
                {
                    _response.IsSuccess = false;
                    _response.Message = "Nom de produit invalide.";
                    return BadRequest(_response);
                }

                var result = await _productService.ProductExistsAsync(productName);
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
        [HttpGet("{productId:int}/valid")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> IsProductValid(int productId)
        {
            try
            {
                var result = await _productService.IsProductValidAsync(productId);
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
        [HttpGet("{productId:int}/can-archive")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CanArchiveProduct(int productId)
        {
            try
            {
                var result = await _productService.CanArchiveProductAsync(productId);
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
        [HttpGet("top")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetTopProducts([FromQuery] int topN = 5)
        {
            try
            {
                if (topN <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Paramètre topN invalide.";
                    return BadRequest(_response);
                }

                var result = await _productService.GetTopProductsAsync(topN);
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
        [HttpGet("dashboard")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetProductDashboard()
        {
            try
            {
                var result = await _productService.GetProductDashboardAsync();
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
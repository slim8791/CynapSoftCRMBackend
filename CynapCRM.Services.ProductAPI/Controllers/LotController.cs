using CynapCRM.Services.ProductAPI.Models.Dto;
using CynapCRM.Services.ProductAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.ProductAPI.Controllers
{
    [Route("api/lots")]
    [ApiController]
    [Authorize]
    public class LotController : ControllerBase
    {
        private readonly ILotService _lotService;
        private readonly IProductService _productService;

        protected ResponseDto _response;
        public LotController(ILotService lotService, IProductService productService)

        {
            _lotService = lotService;

            _response = new();
            _productService = productService;
        }

        [HttpGet("{id:int}/lots")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetLotsByIdProduct(int id)
        {
            if (id <= 0)
            {
                _response.IsSuccess = false;
                _response.Message = "ID de produit invalide.";
                return BadRequest(_response);
            }

            // Vérifier si le produit existe réellement
            var productExists = await _productService.IsProductValidAsync(id);
            if (!productExists)
            {
                _response.IsSuccess = false;
                _response.Message = "Produit introuvable.";
                return NotFound(_response);
            }

            // Produit existe → récupérer les lots (même s’il n’y en a aucun)
            var result = await _lotService.GetLotsByProductIdAsync(id);

            _response.Result = result;   // [] si vide
            _response.IsSuccess = true;
            _response.Message = "Lots récupérés avec succès.";

            return Ok(_response);
        }

        [HttpPost("lot")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> CreateOrUpdateLot([FromBody] LotDto lotDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de lot invalides.";
                    return BadRequest(_response);
                }
                var result = await _lotService.CreateOrUpdateLotAsync(lotDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Erreur, vérifier les références.";
                    return BadRequest(_response);
                }
                _response.Result = result;  // ✅ Retourner les données
                _response.Message = lotDto.Numero == null ? "Lot créé avec succès." : "Lot mis à jour avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);

            }
        }

        [HttpDelete("lot/{numeroLot}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteLot(string numeroLot)
        {
            try
            {
                if (string.IsNullOrEmpty(numeroLot))
                {
                    _response.IsSuccess = false;
                    _response.Message = "Numéro de lot invalide.";
                    return BadRequest(_response);
                }
                bool idDeleted = await _lotService.DeleteLotAsync(numeroLot);
                if (!idDeleted)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Lot non trouvé ou déjà supprimé.";
                    return NotFound(_response);
                }
                _response.Message = "Lot supprimé avec succès.";
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);

            }
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetAllLots()
        {
            try
            {
                var result = await _lotService.GetAllLotsAsync();
                _response.Result = result;
                _response.IsSuccess = true;
                _response.Message = "Tous les lots récupérés avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpPut("product/{productId}/adjust-stock")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> AdjustStock(int productId, [FromQuery] int quantityChange)
        {
            try
            {
                if (productId <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de produit invalide.";
                    return BadRequest(_response);
                }

                var result = await _lotService.AdjustStockAsync(productId, quantityChange);

                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Impossible d’ajuster le stock (stock insuffisant ou lots expirés).";
                    return BadRequest(_response);
                }

                _response.Message = "Stock ajusté avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpPut("lot/{numeroLot}/update-quantity")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> UpdateLotQuantity(string numeroLot, [FromQuery] int quantityChange)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(numeroLot))
                {
                    _response.IsSuccess = false;
                    _response.Message = "Numéro de lot invalide.";
                    return BadRequest(_response);
                }

                var result = await _lotService.UpdateLotQuantityAsync(numeroLot, quantityChange);

                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Impossible de mettre à jour la quantité du lot.";
                    return BadRequest(_response);
                }

                _response.Message = "Quantité du lot mise à jour avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }


        [HttpGet("lot/{numeroLot}/expired")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> IsLotExpired(string numeroLot)
        {
            try
            {
                var result = await _lotService.IsLotExpiredAsync(numeroLot);
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

        [HttpGet("near-expiration")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]

        public async Task<IActionResult> GetLotsNearExpiration([FromQuery] int daysThreshold)
        {
            try
            {
                var result = await _lotService.GetLotsNearExpirationAsync(daysThreshold);

                if (result == null || !result.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucun lot proche de l’expiration.";
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
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetLotByNumero(string numeroLot)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(numeroLot))
                {
                    _response.IsSuccess = false;
                    _response.Message = "Numéro de lot invalide.";
                    return BadRequest(_response);
                }

                var result = await _lotService.GetLotByNumeroAsync(numeroLot);

                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Lot introuvable.";
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
        [HttpGet("product/{productId}/available")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]

        public async Task<IActionResult> GetAvailableLots(int productId)
        {
            try
            {
                if (productId <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de produit invalide.";
                    return BadRequest(_response);
                }

                var result = await _lotService.GetAvailableLotsAsync(productId);

                if (result == null || !result.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucun lot disponible pour ce produit.";
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
        [HttpGet("lot/{numeroLot}/out-of-stock")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]

        public async Task<IActionResult> IsLotOutOfStock(string numeroLot)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(numeroLot))
                {
                    _response.IsSuccess = false;
                    _response.Message = "Numéro de lot invalide.";
                    return BadRequest(_response);
                }

                var result = await _lotService.IsLotOutOfStockAsync(numeroLot);

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
        [HttpGet("expired")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]

        public async Task<IActionResult> GetExpiredLots()
        {
            try
            {
                var result = await _lotService.GetExpiredLotsAsync();

                if (result == null || !result.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucun lot expiré trouvé.";
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

    }
}

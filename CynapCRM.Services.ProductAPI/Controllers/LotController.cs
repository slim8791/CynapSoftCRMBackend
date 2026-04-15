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
        public async Task<IActionResult> GetLotsByIdProduct(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de produit invalide.";
                    return BadRequest(_response);
                }

                var result = await _productService.GetLotsByProductIdAsync(id);
                if (result == null || !result.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucun lot trouvé pour ce produit.";
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

        [HttpPost("lot")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> CreateUpdateLot([FromBody] LotDto lotDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de lot invalides.";
                    return BadRequest(_response);
                }
                var result = await _productService.CreateUpdateLotAsync(lotDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Erreur, vérifier les références.";
                    return BadRequest(_response);
                }
                _response.Message = lotDto.Numero == null ? "Lot créé avec succès." : "Lot mis à jour avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);

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
                bool idDeleted = await _productService.DeleteLotAsync(numeroLot);
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
                return StatusCode(500, _response);

            }
        }

    }
}

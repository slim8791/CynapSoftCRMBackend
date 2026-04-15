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
        public async Task<IActionResult> GetSupportsByIdProduct(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de produit invalide.";
                    return BadRequest(_response);
                }
                var result = await _productService.GetSupportsByProductIdAsync(id);
                if (result == null || !result.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucun support trouvé pour ce produit.";
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

        [HttpPost("support")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> CreateUpdateSupport([FromBody] SupportMarketingDto supportDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de support invalides.";
                    return BadRequest(_response);
                }
                var result = await _productService.CreateUpdateSupportAsync(supportDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    if (supportDto.Id_SupportMarketting == 0)
                    {
                        _response.Message = "Erreur lors de la création du support. Vérifiez les données.";
                    }
                    else
                    {
                        _response.Message = "Erreur lors de la mise à jour du support. Support introuvable.";
                    }
                    return BadRequest(_response);
                }
                _response.Result = result;
                if (supportDto.Id_SupportMarketting == 0)
                {
                    _response.Message = "Support créé avec succès.";
                }
                else
                {
                    _response.Message = "Support mis à jour avec succès.";
                }
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

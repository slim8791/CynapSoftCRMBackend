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
        public async Task<IActionResult> AddFichier([FromBody] FichierDto fichierDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données invalides.";
                    return BadRequest(_response);
                }
                var result = await _productService.AddFichierToSupportAsync(fichierDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Le support spécifié n'existe pas.";
                    return BadRequest(_response);

                }
                _response.Message = "Fichier ajouté avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        [HttpDelete("fichier/{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteFichier(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de fichier invalide.";
                    return BadRequest(_response);
                }
                bool isDeleted = await _productService.DeleteFichierAsync(id);
                if (!isDeleted)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Fichier non trouvé.";
                    return NotFound(_response);
                }
                _response.Result = true;
                _response.Message = "Fichier supprimé avec succès.";
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

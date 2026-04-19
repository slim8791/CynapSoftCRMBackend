using CynapCRM.Services.ProductAPI.Models.Dto;
using CynapCRM.Services.ProductAPI.Service;
using CynapCRM.Services.ProductAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.ProductAPI.Controllers
{

    [Route("api/marketting")]
    [ApiController]
    [Authorize]
    public class MarkettingController : ControllerBase

    {

        private readonly IMarkettingService _markettingService;

        protected ResponseDto _response;

        public MarkettingController(IMarkettingService markettingService)
        {
            _markettingService = markettingService;
            _response = new();
        }



        // ==================================================
        // 🔹 Supports marketing
        // ==================================================

        [HttpGet("product/{productId}/supports")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetSupportsByProduct(int productId)
        {
            try
            {
                if (productId <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID produit invalide.";
                    return BadRequest(_response);
                }

                var result = await _markettingService.GetSupportsByProductAsync(productId);

                if (result == null || !result.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucun support marketing trouvé pour ce produit.";
                    return NotFound(_response);
                }

                _response.Result = result;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        [HttpGet("support/{supportId}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]

        public async Task<IActionResult> GetSupportById(int supportId)
        {
            try
            {
                var result = await _markettingService.GetSupportByIdAsync(supportId);

                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Support marketing introuvable.";
                    return NotFound(_response);
                }

                _response.Result = result;
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
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> CreateOrUpdateSupport([FromBody] SupportMarketingDto supportDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données du support invalides.";
                    return BadRequest(_response);
                }

                var result = await _markettingService.CreateOrUpdateSupportAsync(supportDto);
                _response.Result = result;
                _response.Message = "Support marketing enregistré avec succès.";

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        [HttpPut("support/{supportId}/disable")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DisableSupport(int supportId)
        {
            try
            {
                var result = await _markettingService.DisableSupportAsync(supportId);

                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Impossible de désactiver le support.";
                    return NotFound(_response);
                }

                _response.Message = "Support marketing désactivé.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        // ==================================================
        // 🔹 Fichiers marketing
        // ==================================================

        [HttpPost("support/file")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> AddFileToSupport([FromBody] FichierDto fichierDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données du fichier invalides.";
                    return BadRequest(_response);
                }

                var result = await _markettingService.AddFileToSupportAsync(fichierDto);
                _response.Result = result;
                _response.Message = "Fichier ajouté au support avec succès.";

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        [HttpDelete("file/{fichierId}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteFile(int fichierId)
        {
            try
            {
                var result = await _markettingService.DeleteFileAsync(fichierId);

                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Fichier introuvable.";
                    return NotFound(_response);
                }

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

        [HttpGet("support/{supportId}/files")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]

        public async Task<IActionResult> GetFilesBySupport(int supportId)
        {
            try
            {
                var result = await _markettingService.GetFilesBySupportAsync(supportId);

                _response.Result = result;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        // ==================================================
        // 🔹 Visibilité & campagnes
        // ==================================================

        [HttpGet("support/{supportId}/active")]
        public async Task<IActionResult> IsSupportActive(int supportId)
        {
            try
            {
                var result = await _markettingService.IsSupportActiveAsync(supportId);
                _response.Result = result;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        [HttpGet("product/{productId}/visible-supports")]
        public async Task<IActionResult> GetVisibleSupportsByProduct(int productId)
        {
            try
            {
                var result = await _markettingService.GetVisibleSupportsByProductAsync(productId);
                _response.Result = result;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        [HttpGet("campaign/{campaignName}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]

        public async Task<IActionResult> GetSupportsByCampaign(string campaignName)
        {
            try
            {
                var result = await _markettingService.GetSupportsByCampaignAsync(campaignName);
                _response.Result = result;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        [HttpGet("campaigns")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]

        public async Task<IActionResult> GetCampaigns()
        {
            try
            {
                var result = await _markettingService.GetCampaignsAsync();
                _response.Result = result;
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

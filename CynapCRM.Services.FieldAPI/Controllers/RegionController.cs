using CynapCRM.Services.FieldAPI.Models.Dto;
using CynapCRM.Services.FieldAPI.Service;
using CynapCRM.Services.FieldAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.FieldAPI.Controllers
{

    [ApiController]
    [Route("api/regions")]
    [Authorize]
    public class RegionController : ControllerBase
    {
        private readonly IRegionService _regionService;
        protected ResponseDto _response;

        public RegionController(IRegionService regionService)
        {
            _regionService = regionService;
            _response = new ResponseDto();
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllRegions()
        {
            try
            {
                var regions = await _regionService.GetAllRegionsAsync();
                
                return Ok(regions);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Une erreur est survenue : {ex.Message}";
                return StatusCode(500, _response);
            }
        }
        
        [HttpPost]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> CreateOrUpdateRegion(
            [FromBody] RegionDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de la région invalides.";
                    return BadRequest(_response);
                }

                var result = await _regionService.CreateOrUpdateRegionAsync(dto);

                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Impossible de créer ou modifier la région.";
                    return BadRequest(_response);
                }

                _response.Result = result;
                _response.Message = "Région enregistrée avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }
        [HttpGet("{idRegion:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetRegionById(int idRegion)
        {
            try
            {
                var result = await _regionService.GetRegionByIdAsync(idRegion);

                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Région introuvable.";
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

        [HttpGet("by-delegue/{idDelegue:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetRegionsByDelegue(int idDelegue)
        {
            try
            {
                var result = await _regionService.GetRegionsByDelegueAsync(idDelegue);

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
        [HttpGet("count/{idDelegue:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetNombreRegionsCouvre(int idDelegue)
        {
            try
            {
                var result = await _regionService.GetNombreRegionsCouvreAsync(idDelegue);

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
        [HttpDelete("{idRegion:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteRegion(int idRegion)
        {
            try
            {
                var result = await _regionService.DeleteRegionAsync(idRegion);

                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Suppression impossible (région introuvable).";
                    return BadRequest(_response);
                }

                _response.Message = "Région supprimée avec succès.";
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



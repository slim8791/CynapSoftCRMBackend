using CynapCRM.Services.FieldAPI.Models.Dto;
using CynapCRM.Services.FieldAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.FieldAPI.Controllers
{

    // ═══════════════════════════════════════
    // ObjectifController.cs
    // ═══════════════════════════════════════

    [ApiController]
    [Route("api/objectifs")]
    [Authorize]
    public class ObjectifController : ControllerBase // FIX: Controller → ControllerBase
    {
        private readonly IObjectifService _objectifService;
        protected ResponseDto _response;

        public ObjectifController(IObjectifService objectifService)
        {
            _objectifService = objectifService;
            _response = new ResponseDto();
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetAllObjectifs()
        {
            try
            {
                var objectifs = await _objectifService.GetAllObjectifsAsync();
                _response.Result = objectifs;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpGet("{idObjectif:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetObjectifById(int idObjectif)
        {
            try
            {
                var objectif = await _objectifService.GetObjectifsByIdAsync(idObjectif);
                if (objectif == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Objectif introuvable.";
                    return NotFound(_response);
                }
                _response.Result = objectif;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        // FIX: ajout DELEGUE — peut voir ses propres objectifs
        [HttpGet("by-delegue/{idDelegue:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetObjectifsByDelegue(int idDelegue)
        {
            try
            {
                var result = await _objectifService.GetObjectifsByDelegueAsync(idDelegue);
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
        public async Task<IActionResult> CreateOrUpdateObjectif([FromBody] ObjectifDelegueDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de l'objectif invalides.";
                    return BadRequest(_response);
                }
                var result = await _objectifService.CreateOrUpdateObjectifAsync(dto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Impossible de créer ou modifier l'objectif.";
                    return BadRequest(_response);
                }
                _response.Result = result;
                _response.Message = "Objectif enregistré avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpPut("{idObjectif:int}/value")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> UpdateObjectifValue(
            int idObjectif,
            [FromQuery] int nouvelleValeur)
        {
            try
            {
                var result = await _objectifService
                    .UpdateObjectifValueAsync(idObjectif, nouvelleValeur);
                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Mise à jour impossible.";
                    return BadRequest(_response);
                }
                _response.Message = "Valeur réalisée mise à jour avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpDelete("{idObjectif:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteObjectif(int idObjectif)
        {
            try
            {
                var result = await _objectifService.DeleteObjectifAsync(idObjectif);
                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Suppression impossible (objectif introuvable).";
                    return NotFound(_response);
                }
                _response.Message = "Objectif supprimé avec succès.";
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
